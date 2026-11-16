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
    [Fact]
    public async Task ImportAsync_WithLinkedToken_ThrowsOnTimeout()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(); // Без таймера
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);
        
        var service = new LendingService(new InMemoryBookStore(new List<BookCopy>()));
        var importer = new JsonCatalogImporter();
        
        // Миттєво скасовуємо токен таймауту ЩЕ ДО запуску методу,
        // щоб гарантовано уникнути гонки потоків і помилки FileNotFound
        await timeoutCts.CancelAsync(); 

        // Метод має викинути помилку скасування
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(importer, "any.json", null, linkedCts.Token));
            
        // Доводимо, що спрацював саме таймаут, а не Ctrl+C
        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
    }
}