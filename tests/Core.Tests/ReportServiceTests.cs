using System.Linq;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    // 3) Звіт на кількох наборах вхідних даних (перевірка ліміту для TopReaders)
    [Theory]
    [InlineData(1, 1)]  // Просимо топ-1 -> отримуємо 1 читача
    [InlineData(2, 2)]  // Просимо топ-2 -> отримуємо 2 читачів
    [InlineData(10, 3)] // Просимо топ-10, але у нас в TestData всього 3 унікальних читачі -> отримуємо 3
    public void TopReaders_LimitsResultsCorrectly(int limit, int expectedCount)
    {
        var result = ReportService.TopReaders(TestData.Items(), limit);
        Assert.Equal(expectedCount, result.Count);
    }

    // 4) Детермінований топ-N (стабільне сортування завдяки ThenBy)
    [Fact]
    public void TopReaders_IsDeterministic()
    {
        // У TestData R-1 має 2 книги, а R-2 та R-3 по одній книзі.
        // Завдяки ThenBy(s => s.Key) при однаковій кількості R-2 завжди буде перед R-3.
        var top = ReportService.TopReaders(TestData.Items(), 3).Select(r => r.Key).ToList();
        
        Assert.Equal(new[] { "R-1", "R-2", "R-3" }, top);
    }
}