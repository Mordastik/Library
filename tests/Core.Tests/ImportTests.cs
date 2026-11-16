using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    // 5) Скасування (відловлюємо OperationCanceledException)
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        // Ось ці два рядки загубилися:
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Скасовуємо токен ЩЕ ДО запуску
        
        var service = new LendingService(new InMemoryBookStore(new List<BookCopy>()));
        var importer = new JsonCatalogImporter();
        
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(importer, "any.json", cts.Token));
    }
}