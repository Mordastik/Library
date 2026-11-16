using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public event EventHandler<LendingChangedEventArgs>? Changed;

    private void OnChanged(string copyId, LendingChangeKind kind, string? readerId = null, DateTimeOffset? dueDate = null) => 
        Changed?.Invoke(this, new LendingChangedEventArgs(copyId, kind, readerId, dueDate));

    // --- ДОДАНИЙ АСИНХРОННИЙ ІМПОРТ ---
    public async Task<int> ImportAsync(ICatalogImporter importer, string path, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        var added = 0;
        
        // Читаємо дані потоково по мірі їх появи
        await foreach (var item in importer.LoadAsync(path, ct))
        {
            ct.ThrowIfCancellationRequested();
            
            if (_store.GetById(item.Id) is null) 
            {
                _store.Add(item);
                OnChanged(item.Id, LendingChangeKind.BookAdded);
                added++;
            }
            
            // Звітуємо підписнику про прогрес
            progress?.Report(added);
        }
        return added;
    }

    public BookCopy AddBook(string isbn)
    {
        var copy = BookCopy.Create(Guid.NewGuid().ToString("N")[..8], isbn);
        _store.Add(copy);
        OnChanged(copy.Id, LendingChangeKind.BookAdded);
        return copy;
    }

    public void IssueCopy(string id, string readerId, DateTimeOffset dueDate)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        copy.Issue(readerId, dueDate); 
        _store.Update(copy);
        OnChanged(copy.Id, LendingChangeKind.CopyIssued, readerId, dueDate);
    }

    public void ReturnCopy(string id)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        copy.Return(); 
        _store.Update(copy);
        OnChanged(copy.Id, LendingChangeKind.CopyReturned);
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string id) => _store.GetById(id);

    public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate) => 
        _store.List().Where(predicate).ToList();
}