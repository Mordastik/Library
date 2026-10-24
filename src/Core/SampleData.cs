using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<BookCopy> Copies() =>
    [
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
    ];
}