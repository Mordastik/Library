using System;
using System.IO;
using System.Linq;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Copies());

var service = new LendingService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

// 1. Додаємо новий примірник та видаємо його
var created = service.AddBook("978-3-16-148410-0");
service.IssueCopy(created.Id);

// 2. Знаходимо за ID
var found = service.Find(created.Id);
Console.WriteLine($"Знайдено за ID ({created.Id}): {found}");
Console.WriteLine("\nУсі примірники у сховищі:");

foreach (var copy in service.All())
{
    Console.WriteLine($" {copy}");
}

// 3. Сценарій відмови (Крок 6)
Console.WriteLine("\n--- Сценарій відмови ---");
try
{
    Console.WriteLine("Спроба видати примірник з неіснуючим ID...");
    service.IssueCopy("FAKE-ID-999");
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка ({ex.GetType().Name}): {ex.Message}");
}

try
{
    Console.WriteLine("Спроба додати дублікат ID напряму в сховище...");
    store.Add(BookCopy.Create(created.Id, "978-0-00-000000-0"));
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка ({ex.GetType().Name}): {ex.Message}");
}