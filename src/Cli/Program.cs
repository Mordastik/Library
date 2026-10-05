using Core.Domain;

Console.WriteLine("Сценарій 1: успіх");
var copy = BookCopy.Create("C-001", "978-3-16-148410");
var loan = Loan.Open("L-001", copy.Id, "R-123", DateTime.Now);

copy.Issue(); // Змінюємо стан примірника
Console.WriteLine(loan);
Console.WriteLine($"Примірник видано: {copy.IsIssued}");

loan.Close(DateTime.Now.AddDays(7)); // Успішно закриваємо
copy.Return(); // Повертаємо примірник
Console.WriteLine(loan);
Console.WriteLine($"Примірник видано: {copy.IsIssued}");
Console.WriteLine();

Console.WriteLine("Сценарій 2: порушення інваріантів");

// Намагаємось видати вже виданий примірник
var copy2 = BookCopy.Create("C-002", "978-0-261-10328");
copy2.Issue();
TryDo("повторна видача", () => copy2.Issue());

// Намагаємось створити примірник без порожнього ISBN
TryDo("порожній ISBN", () => BookCopy.Create("C-003", ""));

// Намагаємось закрити видачу з датою в минулому
var loan2 = Loan.Open("L-002", "C-004", "R-456", DateTime.Now);
TryDo("дата повернення в минулому", () => loan2.Close(DateTime.Now.AddDays(-1)));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} - {ex.Message}");
    }
}