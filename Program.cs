using System; //Importuoja funkcijas i sistema kaip input/output
using System.Collections.Generic; //Importuoja namespace norint naudotis List<T>
using System.IO; //Importuoja failu operacijoms

public interface IStorageService //Apibrėžia saugojimo mechanizmų abstraction
{
    void SaveTasks(List<TaskItem> tasks); //Metodas Issaugoti darbus
    List<TaskItem> LoadTasks(); //Metodas Ikelti darbus
}

// 2️⃣ Entity class - Encapsulation & Abstraction 
public class TaskItem //Nurodo uzduoti darbu sarase
{
    public int Id { get; set; }
    public string Aprasymas { get; set; }
    public bool IsCompleted { get; set; }

    public TaskItem(int id, string aprasymas) //Konstruktorius inicijuoja nauja uzduoti
    {
        Id = id;
        Aprasymas = aprasymas;
        IsCompleted = false;
    }
    public void MarkComplete() //Pazymi uzduoti kaip atlikta
    {
        IsCompleted = true;
    }
    public override string ToString()
    {
        return $"{Id}. [{(IsCompleted ? "✔" : " ")}] {Aprasymas}";
    }
}
// 3️⃣ Single Responsibility Principle (SRP) - TaskManager handles task-related operations
public class TaskManager
{
    private readonly List<TaskItem> _tasks = new();
    private readonly IStorageService _storageService;

    public TaskManager(IStorageService storageService)
    {
        _storageService = storageService;
        _tasks = _storageService.LoadTasks() ?? new List<TaskItem>();
    }

    public void AddTask(string aprasymas)
    {
        int newId = _tasks.Count + 1;
        _tasks.Add(new TaskItem(newId, aprasymas));
        _storageService.SaveTasks(_tasks);
    }

    public void MarkTaskComplete(int taskId)
    {
        var task = _tasks.Find(t => t.Id == taskId);
        if (task != null)
        {
            task.MarkComplete();
            _storageService.SaveTasks(_tasks);
        }
    }

    public void ShowTasks()
    {
        Console.WriteLine("\n--- To-Do List ---");
        _tasks.ForEach(Console.WriteLine);
    }
}

// 4️⃣ Implementation of IStorageService (Liskov Substitution & Open/Closed Principles)
public class FileStorageService : IStorageService //Inheratence
{
    private readonly string _filePath = "tasks.txt";

    public void SaveTasks(List<TaskItem> tasks)
    {
        using StreamWriter writer = new StreamWriter(_filePath);
        foreach (var task in tasks)
        {
            writer.WriteLine($"{task.Id}|{task.Aprasymas}|{task.IsCompleted}");
        }
    }

    public List<TaskItem> LoadTasks()
    {
        if (!File.Exists(_filePath)) return new List<TaskItem>();

        var tasks = new List<TaskItem>();
        foreach (var line in File.ReadAllLines(_filePath))
        {
            var parts = line.Split('|');
            var task = new TaskItem(int.Parse(parts[0]), parts[1])
            {
                IsCompleted = bool.Parse(parts[2])
            };
            tasks.Add(task);
        }
        return tasks;
    }
}

// 5️⃣ High-level Application Layer (Dependency Inversion Principle)
public class Program
{
    public static void Main()
    {
        IStorageService storageService = new FileStorageService();//DIP
        TaskManager taskManager = new TaskManager(storageService);//asdasdasdasdasdasd

        while (true)
        {
            Console.WriteLine("\n1. Add Task\n2. Mark Task Complete\n3. Show Tasks\n4. Exit");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter task aprasymas: ");
                    string aprasymas = Console.ReadLine();
                    taskManager.AddTask(aprasymas);
                    break;
                case "2":
                    Console.Write("Enter task ID to mark complete: ");
                    if (int.TryParse(Console.ReadLine(), out int taskId))
                        taskManager.MarkTaskComplete(taskId);
                    break;
                case "3":
                    taskManager.ShowTasks();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid choice, try again.");
                    break;
            }
        }
    }
}
