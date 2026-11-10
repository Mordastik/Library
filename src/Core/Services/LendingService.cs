using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // Подія сповіщення про зміну стану
    public event EventHandler<LendingChangedEventArgs>? Changed; //1

    private void OnChanged(string copyId, LendingChangeKind kind, string? readerId = null, DateTimeOffset? dueDate = null) => //1
        Changed?.Invoke(this, new LendingChangedEventArgs(copyId, kind, readerId, dueDate));

    public BookCopy AddBook(string isbn)
    {
        var copy = BookCopy.Create(Guid.NewGuid().ToString("N")[..8], isbn);
        _store.Add(copy);

        // Підняття події ПІСЛЯ збереження
        OnChanged(copy.Id, LendingChangeKind.BookAdded);
        return copy;
    }

    public void IssueCopy(string id, string readerId, DateTimeOffset dueDate)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        // ЗМІНЕНО: Передаємо параметри у метод сутності
        copy.Issue(readerId, dueDate); // Якщо сутність кине виняток (вже видано) — подія не підніметься
        _store.Update(copy);

        OnChanged(copy.Id, LendingChangeKind.CopyIssued, readerId, dueDate);
    }

    public void ReturnCopy(string id)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        copy.Return(); // Якщо сутність кине виняток (не був виданий) — подія не підніметься
        _store.Update(copy);

        OnChanged(copy.Id, LendingChangeKind.CopyReturned);
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string id) => _store.GetById(id);

    // Крок 7: використання Func для вибірки за критерієм
    public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate) => //6
        _store.List().Where(predicate).ToList();
}