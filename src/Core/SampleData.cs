using System;
using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<BookCopy> Copies()
    {
        var copies = new List<BookCopy>
        {
            BookCopy.Create("BC-001", "978-966-10-1234-1"),
            BookCopy.Create("BC-002", "978-966-10-1234-2"),
            BookCopy.Create("BC-003", "978-617-12-8351-0"),
            BookCopy.Create("BC-004", "978-617-12-8351-0"),
            BookCopy.Create("BC-005", "978-966-03-8911-3"),
            BookCopy.Create("BC-006", "978-966-03-8911-3"),
            BookCopy.Create("BC-007", "978-617-75-4412-7"),
            BookCopy.Create("BC-008", "978-617-679-652-7"),
            BookCopy.Create("BC-009", "978-966-948-215-0"),
            BookCopy.Create("BC-010", "978-966-948-215-0"),
            BookCopy.Create("BC-011", "978-617-585-190-6"),
            BookCopy.Create("BC-012", "978-966-2355-89-5"),
            BookCopy.Create("BC-013", "978-617-7808-15-1"),
            BookCopy.Create("BC-014", "978-966-978-112-3"),
            BookCopy.Create("BC-015", "978-617-12-7420-4")
        };

        // Видаємо книги для перевірки звітів:
        
        // 1. Боржник R-100 (Прострочено на 10 і 5 днів)
        copies[0].Issue("R-100", DateTimeOffset.Now.AddDays(-10));
        copies[1].Issue("R-100", DateTimeOffset.Now.AddDays(-5));
        
        // 2. Боржник R-200 (Прострочено на 2 дні)
        copies[3].Issue("R-200", DateTimeOffset.Now.AddDays(-2));
        
        // 3. Нормальні видачі (дедлайн ще не настав)
        copies[5].Issue("R-300", DateTimeOffset.Now.AddDays(7));
        copies[6].Issue("R-300", DateTimeOffset.Now.AddDays(14));
        copies[10].Issue("R-400", DateTimeOffset.Now.AddDays(5));
        copies[13].Issue("R-500", DateTimeOffset.Now.AddDays(2));

        return copies;
    }
}