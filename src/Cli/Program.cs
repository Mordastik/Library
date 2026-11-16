using System;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Reports;
using Core;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;
using Core.Domain; 

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Copies());

var service = new LendingService(store);
Console.WriteLine($"Сховище: {store.GetType().Name}");

// =================================================================
//                 ЛАБОРАТОРНА 8: ДОДАТКОВІ ЗАВДАННЯ
// =================================================================
using var userCts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; userCts.Cancel(); };

// Таймаут на 5 секунд. Якщо імпорт триватиме довше — скасується автоматично
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); 
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

try
{
    Console.WriteLine("\nПочинаємо потоковий імпорт...");
    
    // IProgress малює динамічний рядок у консолі
    var progress = new Progress<int>(p => 
    {
        Console.Write($"\r[Прогрес імпорту]: {p} примірників оброблено...");
    });

    // Передаємо linkedCts.Token та progress
    var added = await service.ImportAsync(new JsonCatalogImporter(), dataPath, progress, linkedCts.Token);
    Console.WriteLine($"\nІмпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    if (timeoutCts.IsCancellationRequested)
        Console.WriteLine("Імпорт скасовано автоматично через ТАЙМАУТ.");
    else if (userCts.IsCancellationRequested)
        Console.WriteLine("Імпорт скасовано користувачем (Ctrl+C).");
    else
        Console.WriteLine("Імпорт скасовано.");
}
catch (DomainException ex) { Console.WriteLine($"\nДані відхилено: {ex.Message}"); }
catch (FileNotFoundException) { Console.WriteLine($"\nФайл не знайдено: {dataPath}"); }


// =================================================================
//                 ЛАБОРАТОРНА 6: ПОДІЇ ТА ПІДПИСНИКИ
// =================================================================
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnLendingChanged(object? sender, LendingChangedEventArgs e)
{
    string details = e.Kind switch
    {
        LendingChangeKind.BookAdded => "додано новий примірник",
        LendingChangeKind.CopyIssued => $"видано читачу {e.ReaderId} до {e.DueDate:yyyy-MM-dd}",
        LendingChangeKind.CopyReturned => "примірник успішно повернуто",
        _ => string.Empty
    };

    var line = $"{e.At:HH:mm:ss} | {e.Kind,-12} | Примірник: {e.CopyId} | {details}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnLendingChanged;

int changes = 0;
service.Changed += (_, _) => changes++;

Console.WriteLine("\n--- Активні підписки (працюють обидва підписники) ---");
var copy1 = service.AddBook("978-3-16-148410-0");
service.IssueCopy(copy1.Id, "R-101", DateTimeOffset.Now.AddDays(14));
service.ReturnCopy(copy1.Id);

Console.WriteLine($"Подій отримано: {changes}"); 

Console.WriteLine("\n--- Відписка від логування в консоль/файл (-=) ---");
service.Changed -= OnLendingChanged;

var copy2 = service.AddBook("978-0-261-10328-3");
service.IssueCopy(copy2.Id, "R-202", DateTimeOffset.Now.AddDays(7));

Console.WriteLine($"Подій отримано (лише лямбда-лічильник): {changes}"); 

Console.WriteLine("\n--- Сценарій відмови ---");
try
{
    Console.WriteLine("Спроба повторно видати вже виданий примірник copy2..."); 
    service.IssueCopy(copy2.Id, "R-303", DateTimeOffset.Now.AddDays(3));
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.GetType().Name}\n{ex.Message}");
}
Console.WriteLine($"Подій після помилки (не змінилося): {changes}"); 

Console.WriteLine("\n--- Пошук вільних примірників через Func (Where) ---");
var availableCopies = service.FindAll(c => !c.IsIssued); 
Console.WriteLine($"Знайдено вільних примірників: {availableCopies.Count}");
foreach (var c in availableCopies.Take(3))
{
    Console.WriteLine($" - {c}");
}


// =================================================================
//                 ЛАБОРАТОРНА 7: ЗВІТИ ТА LINQ
// =================================================================
Console.WriteLine("\n=== ДЕМО ВІДКЛАДЕНОГО ВИКОНАННЯ ===");
var sourceList = service.All().ToList();
var query = sourceList.Where(p => !p.IsIssued); 

Console.WriteLine($"Вільних примірників до змін: {query.Count()}");
sourceList.Add(BookCopy.Create("tmp001", "978-0-000000-0"));
Console.WriteLine($"Вільних примірників після змін: {query.Count()}");

var snapshot = query.ToList(); 
sourceList.Clear();
Console.WriteLine($"У знімку лишилось: {snapshot.Count}\n"); 


Console.WriteLine("=== ЗВІТИ (LINQ) ===");
var items = service.All();
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-groups";

switch (command)
{
    case "report-overdue":
        var overdue = ReportService.Overdue(items, DateTimeOffset.Now);
        foreach (var l in overdue)
            Console.WriteLine($" Боржник: {l.ReaderId,-8} | Копія: {l.CopyId,-8} | ISBN: {l.Isbn,-18} | Прострочено з: {l.DueDate:yyyy-MM-dd}");
        break;

    case "report-top-readers":
        var top = ReportService.TopReaders(items, 5);
        foreach (var t in top)
            Console.WriteLine($" Читач {t.Key,-8} тримає книг: {t.TotalCopies}");
        break;

    case "report-groups":
        var groups = ReportService.ByIsbn(items);
        foreach (var g in groups)
            Console.WriteLine($" ISBN {g.Key,-18} | Всього: {g.TotalCopies,3} | Видано: {g.IssuedCopies,3}");
        break;

    case "report-sum":
        var totals = ReportService.Totals(items);
        Console.WriteLine($" Всього примірників: {totals.TotalCopies}");
        Console.WriteLine($" Унікальних видань (ISBN): {totals.UniqueIsbns}");
        Console.WriteLine($" Середня кількість копій на видання: {totals.AvgPerIsbn:F2}");
        Console.WriteLine($" Максимум копій одного видання: {totals.MaxPerIsbn}");
        break;

    default:
        Console.WriteLine("Доступні звіти: report-overdue | report-top-readers | report-groups | report-sum");
        break;
}