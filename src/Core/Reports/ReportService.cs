using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1. Фільтр + сортування + проєкція: Прострочені видачі
    public static IReadOnlyList<LendingLine> Overdue(IEnumerable<BookCopy> source, DateTimeOffset today) =>
        source.Where(p => p.IsIssued && p.DueDate < today)
              .OrderBy(p => p.DueDate)
              .ThenBy(p => p.Id, StringComparer.Ordinal)
              .Select(p => new LendingLine(p.Id, p.Isbn, p.ReaderId!, p.DueDate!.Value))
              .ToList();

    // 2. Топ-N зі стабільним порядком: Топ читачів за кількістю виданих книг
    public static IReadOnlyList<GroupSummary> TopReaders(IEnumerable<BookCopy> source, int n) =>
        source.Where(p => p.IsIssued && p.ReaderId != null)
              .GroupBy(p => p.ReaderId!, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Count())) // Для читача Total і Issued збігаються
              .OrderByDescending(s => s.TotalCopies)
              .ThenBy(s => s.Key, StringComparer.Ordinal)
              .Take(n)
              .ToList();

    // 3. Групування з агрегатами: Статистика за книгами (ISBN)
    public static IReadOnlyList<GroupSummary> ByIsbn(IEnumerable<BookCopy> source) =>
        source.GroupBy(p => p.Isbn, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Count(c => c.IsIssued)))
              .OrderByDescending(s => s.TotalCopies)
              .ToList();

    // 4. Агрегати з обробкою порожньої послідовності: Загальна статистика (кортеж)
    public static (int TotalCopies, int UniqueIsbns, double AvgPerIsbn, int MaxPerIsbn) Totals(IEnumerable<BookCopy> source)
    {
        var list = source.ToList();
        
        // Перевірка на порожнечу, щоб Average і Max не кинули InvalidOperationException
        if (list.Count == 0) return (0, 0, 0d, 0);

        // Рахуємо кількість примірників для кожного ISBN
        var copiesPerIsbn = list.GroupBy(p => p.Isbn).Select(g => g.Count()).ToList();

        return (list.Count, copiesPerIsbn.Count, copiesPerIsbn.Average(), copiesPerIsbn.Max());
    }
}