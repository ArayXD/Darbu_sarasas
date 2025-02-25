using System; //Importuoja funkcijas i sistema kaip input/output
using System.Collections.Generic; //Importuoja namespace norint naudotis List<T>
using System.IO; //Importuoja failu operacijoms

// 1️ Interface Segregation & Dependency Inversion (DIP) - SOLID: Dependency Inversion (D)
public interface IStorageService //Abstraction//Apibrėžia saugojimo mechanizmų abstraction
{
    void SaveTasks(List<TaskItem> tasks); //Interface Segregation//Metodas Issaugoti darbus
    List<TaskItem> LoadTasks(); //Dependency Inversion//Metodas Ikelti darbus
}

// 2️ Entity class - Encapsulation & Abstraction 
public class TaskItem //Encapsulation//Nurodo uzduoti darbu sarase
{
    public int Id { get; set; }
    public string Aprasymas { get; set; }
    public bool IsCompleted { get; set; }

    public TaskItem(int id, string aprasymas) //Abstraction - slepia igyvendinimo datales//Konstruktorius inicijuoja nauja uzduoti
    {
        Id = id;
        Aprasymas = aprasymas;
        IsCompleted = false;
    }
    public void MarkComplete() //Encapsulation - apsaugo uzduoties busena//Pazymi uzduoti kaip atlikta
    {
        IsCompleted = true;
    }
    public override string ToString() //Polymorphysm//Overrides default ToString, kad formatuotu uzduoti
    {
        return $"{Id}. [{(IsCompleted ? "✔" : " ")}] {Aprasymas}"; //parodo uzduoti
    }
}
// 3️ Single Responsibility Principle (SRP) - TaskManager handles task-related operations
public class TaskManager //Single Responsibility - valdo tik su uzduotimis susijusia logika
{
    private readonly List<TaskItem> _tasks = new(); //Encapsulation - privatus uzduociu sarasas//Issaugo uzduociu sarasa
    private readonly IStorageService _storageService; //Dependency Inversion - naudoja abstrackcija//Priklausomybe nuo Storage paslaugos

    public TaskManager(IStorageService storageService) //Dependecy Injection//Konstruktorius suleidzia Storage paslauga
    {
        _storageService = storageService;
        _tasks = _storageService.LoadTasks() ?? new List<TaskItem>(); //Ikelia uzduotis is saugyklos arba inicijuoja nauja sarasa
    }

    public void AddTask(string aprasymas) //Single Responsibility//Itraukia nauja uzduoti i sarasa
    {
        int newId = _tasks.Count + 1; //Automatiskai generuoja nauja uzduoties ID
        _tasks.Add(new TaskItem(newId, aprasymas)); //Sukuria ir prideda nauja uzduoti
        _storageService.SaveTasks(_tasks); //Isaugoja atnaujinta sarasa
    }

    public void MarkTaskComplete(int taskId) //Single Responsibility//Pazymi uzduoti atlikta
    {
        var task = _tasks.Find(t => t.Id == taskId); //Rasti uzduoti naudojant pagal ID
        if (task != null) //Isitikina kad uzduotis egzistuoja
        {
            task.MarkComplete(); //Encapsulation - iskviecia metoda busenai pakeisti//Pazymi kaip atlikta
            _storageService.SaveTasks(_tasks); //Isaugoja atnaujinta sarasa
        }
    }

    public void ShowTasks() //Single Responsibility//Parodo visas uzduotis
    {
        Console.WriteLine("\n--- To-Do List ---"); 
        _tasks.ForEach(Console.WriteLine); //Polymorphysm//Spausdina visas uzduotis naudojant overriden ToString()
    }
}

// 4️ Implementation of IStorageService (Liskov Substitution & Open/Closed Principles)
public class FileStorageService : IStorageService //Inheratence - Paveldzia is IStorageService //Igyvendina Storage paslauga naudojant tekstini faila
{
    private readonly string _filePath = "tasks.txt"; //Failo kelias uzduotis saugoti

    public void SaveTasks(List<TaskItem> tasks) //Single Responsibility//Issaugo uzduotis faile
    {
        using StreamWriter writer = new StreamWriter(_filePath); //Atidaro faila rasymui
        foreach (var task in tasks) //Kartoja per uzduotis
        {
            writer.WriteLine($"{task.Id}|{task.Aprasymas}|{task.IsCompleted}"); //Parasyta uzduoties informacija vamzdziais atskirtu formatu
        }
    }

    public List<TaskItem> LoadTasks() //Single Responsibility//Ikelia uzduotis is failo
    {
        if (!File.Exists(_filePath)) return new List<TaskItem>(); //Grazina tuscia sarasa jei failo nera

        var tasks = new List<TaskItem>(); //Sukuria tuscia sarasa
        foreach (var line in File.ReadAllLines(_filePath)) //Skaito faila line by line
        {
            var parts = line.Split('|'); //Padalina line per pusia
            var task = new TaskItem(int.Parse(parts[0]), parts[1]) //Liskov Substitution - Objekto keitimas veikia//Sukuria uzduoti is failo duomenu
            {
                IsCompleted = bool.Parse(parts[2]) //Nustato uzbaigimo busena
            };
            tasks.Add(task); //Prideda uzduoti i sarasa
        }
        return tasks; //Grazina ikeltas uzduotis
    }
}

// 5️⃣ High-level Application Layer (Dependency Inversion Principle)
public class Program
{
    public static void Main() //Programos iejimo taskas
    {
        IStorageService storageService = new FileStorageService();//Dependency Inversion - naudoja abstrakcija//Naudoja failo saugykla
        TaskManager taskManager = new TaskManager(storageService);//Iveda saugyklos priklausomybe i TaskManageri

        while (true) //Begalinis ciklas, kad programa veiktu iki isjungimo
        {
            Console.WriteLine("\n1. Add Task\n2. Mark Task Complete\n3. Show Tasks\n4. Exit"); //Display
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine(); //Skaito vartotojo pasirinkima

            switch (choice) //Tvarko vartotojo ivedima
            {
                case "1": //Add Task
                    Console.Write("Enter task description: ");
                    string aprasymas = Console.ReadLine(); //Gauna uzduoties aprasyma
                    taskManager.AddTask(aprasymas); //Polymorphysm - Calls method dynamically//Prideda nauja uzduoti
                    break;
                case "2": //Mark Task As Complete
                    Console.Write("Enter task ID to mark complete: ");
                    if (int.TryParse(Console.ReadLine(), out int taskId)) //Patvirtina ivedima
                        taskManager.MarkTaskComplete(taskId); //Polymorphysm - Calls method dynamically//Pazymi kaip atlikta
                    break;
                case "3": //Show tasks
                    taskManager.ShowTasks(); //Rodo visas uzduotis
                    break;
                case "4": //Exit
                    return; //Iseina is aplikacijos
                default:
                    Console.WriteLine("Invalid choice, try again."); //Tvarko neteisinga ivedima pvz.(5,6,7...)
                    break;
            }
        }
    }
}
