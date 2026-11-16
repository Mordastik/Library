using System;
using System.Collections.Generic;
using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static IReadOnlyList<BookCopy> Items()
    {
        var c1 = BookCopy.Create("c1", "ISBN-1");
        c1.Issue("R-1", DateTimeOffset.Now); // Читач R-1 взяв першу книгу
        
        var c2 = BookCopy.Create("c2", "ISBN-2");
        c2.Issue("R-1", DateTimeOffset.Now); // Читач R-1 взяв другу книгу
        
        var c3 = BookCopy.Create("c3", "ISBN-3");
        c3.Issue("R-2", DateTimeOffset.Now); // Читач R-2 взяв 1 книгу
        
        var c4 = BookCopy.Create("c4", "ISBN-4");
        c4.Issue("R-3", DateTimeOffset.Now); // Читач R-3 взяв 1 книгу
        
        return new[] { c1, c2, c3, c4 };
    }
}