using System.Collections.Generic;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class LendingServiceTests
{
    // 1) Інваріант за доменом Бібліотека: тест на повернення невиданого примірника
    [Fact]
    public void ReturnCopy_WhenNotIssued_Throws_AndKeepsState()
    {
        var store = new InMemoryBookStore(new List<BookCopy>());
        var service = new LendingService(store);
        var p = service.AddBook("ISBN-123"); // Створюємо новий, ще НЕ виданий примірник
        
        // Перевіряємо, що кидається саме доменна помилка
        Assert.Throws<DomainException>(() => service.ReturnCopy(p.Id));
        
        // Перевіряємо, що стан не зламався (примірник так і лишився невиданим)
        Assert.False(store.GetById(p.Id)!.IsIssued);
    }

    // 2) Подія як спосіб перевірки
    [Fact]
    public void AddBook_RaisesChangedExactlyOnce()
    {
        var service = new LendingService(new InMemoryBookStore(new List<BookCopy>()));
        var events = new List<LendingChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
        
        service.AddBook("ISBN-456");
        
        Assert.Single(events); // Переконуємось, що подія була рівно одна
        Assert.Equal(LendingChangeKind.BookAdded, events[0].Kind);
    }
}