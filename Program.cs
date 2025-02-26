using System; //Importuoja funkcijas i sistema kaip input/output
using System.Collections.Generic; //Importuoja namespace norint naudotis List<T>
using System.IO;//Importuoja failu operacijoms
using Newtonsoft.Json; //Importuoja failu serializacijos biblioteka

// Single Responability (SOLID) 
public interface IStorageService //Abstraction//Apibrėžia saugojimo mechanizmų abstraction
{
    void SaveTasks(List<TaskItem> tasks); //Metodas Issaugoti darbus
    List<TaskItem> LoadTasks(); //Metodas Ikelti darbus
}

// Single Responsibility
public class TaskItem //Nurodo uzduoti darbu sarase
{
    public int Id { get; private set; }
    public string Aprasymas { get; private set; } //Encapsulation - kontroliuoja prieeiga prie klases elementu
    public bool IsCompleted { get; private set; }

    public TaskItem(int id, string aprasymas) //Encapsulation - suteikia prieeiga prie nepasiekiamu klases elementu//Konstruktorius inicijuoja nauja uzduoti su tokiais parametrais
    {
        Id = id;
        Aprasymas = aprasymas;
        IsCompleted = false;
    }
    public void MarkComplete() //Encapsulation - suteikia prieeiga prie nepasiekiamu klases elementu//Pazymi uzduoti kaip atlikta
    {
        IsCompleted = true;
    }
    public override string ToString() //Overrides default ToString, kad formatuotu uzduoti
    {
        return $"{Id}. [{(IsCompleted ? "✔" : " ")}] {Aprasymas}"; //parodo uzduoti
    }
}

public class TaskManager //Single Responsibility - valdo tik su uzduotimis susijusia logika
{
    private readonly List<TaskItem> _tasks = new(); //Encapsulation - privatus uzduociu sarasas//Issaugo uzduociu sarasa
    private readonly IStorageService _storageService; //Priklausomybe nuo Storage paslaugos

    public TaskManager(IStorageService storageService) //Dependecy Inversion - Pakeiciam kodo funkcija nepakeisdamas koda//Konstruktorius suleidzia Storage paslauga
    {
        _storageService = storageService;
        _tasks = _storageService.LoadTasks() ?? new List<TaskItem>(); //Ikelia uzduotis is saugyklos arba inicijuoja nauja sarasa
    }

    public void AddTask(string aprasymas) //Itraukia nauja uzduoti i sarasa
    {
        int newId = _tasks.Count + 1; //Automatiskai generuoja nauja uzduoties ID
        _tasks.Add(new TaskItem(newId, aprasymas)); //Sukuria ir prideda nauja uzduoti
        _storageService.SaveTasks(_tasks); //Isaugoja atnaujinta sarasa
    }

    public void MarkTaskComplete(int taskId) //Pazymi uzduoti atlikta
    {
        var task = _tasks.Find(t => t.Id == taskId); //Rasti uzduoti naudojant pagal ID
        if (task != null) //Isitikina kad uzduotis egzistuoja
        {
            task.MarkComplete(); //Encapsulation - iskviecia metoda busenai pakeisti//Pazymi kaip atlikta
            _storageService.SaveTasks(_tasks); //Isaugoja atnaujinta sarasa
        }
    }

    public void ShowTasks() //Parodo visas uzduotis
    {
        Console.WriteLine("\n--- To-Do List ---"); 
        _tasks.ForEach(Console.WriteLine); //Spausdina visas uzduotis naudojant overriden ToString()
    }
}

// Implementation of IStorageService
public class FileStorageService : IStorageService //Inheratence - Paveldzia is IStorageService //Igyvendina Storage paslauga naudojant tekstini faila
{
    private readonly string _filePath = "tasks.txt"; //Encapsuliacion - paslepia koda kad neredaguotu//Failo kelias uzduotis saugoti

    public void SaveTasks(List<TaskItem> tasks) //Issaugo uzduotis faile
    {
        using StreamWriter writer = new StreamWriter(_filePath); //Atidaro faila rasymui
        writer.WriteLine(JsonConvert.SerializeObject(tasks)); //Serialization - konvertuoja uzdsuociu sarasa i texta kuris formatuotas Json formatu tam kad butu galima paskui perskaityti
    }

    public List<TaskItem> LoadTasks() //Ikelia uzduotis is failo
    {
        if (!File.Exists(_filePath)) return new List<TaskItem>(); //Grazina tuscia sarasa jei failo nera

        return (List<TaskItem>)JsonConvert.DeserializeObject(File.ReadAllText(_filePath))?? throw new ArgumentNullException("Error while reading Json file"); //Exception - jeigu nutinka klaida pranesama sistemai//Serialzation - konvertuoja texta is Json failo atgal i uzduociu sarasa
    }
}


public class Program  
{
    public static void Main() //Programos iejimo taskas
    {
        IStorageService storageService = new FileStorageService();//Naudoja failo saugykla
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
                    taskManager.AddTask(aprasymas); //Prideda nauja uzduoti
                    break;
                case "2": //Mark Task As Complete
                    Console.Write("Enter task ID to mark complete: ");
                    if (int.TryParse(Console.ReadLine(), out int taskId)) //Patvirtina ivedima
                        taskManager.MarkTaskComplete(taskId); //Pazymi kaip atlikta
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
