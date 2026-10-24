using System;
using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    private bool _isIssued; // Класичне приватне поле з підкресленням

    public string Id { get; }
    public string Isbn { get; }
    public bool IsIssued => _isIssued; // Властивість лише для читання (стрілка)

    private BookCopy(string id, string isbn, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        _isIssued = isIssued;
    }

    public static BookCopy Create(string id, string isbn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim(), false);
    }

    public void Issue()
    {
        if (_isIssued)
            throw new InvalidOperationException($"Примірник {Id} вже виданий, повторна видача неможлива.");
        _isIssued = true;
    }

    public void Return()
    {
        if (!_isIssued)
            throw new InvalidOperationException($"Примірник {Id} не можна повернути, бо він не виданий.");
        _isIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, _isIssued);

    public static BookCopy FromDto(BookCopyDto dto)
    {
        var copy = Create(dto.Id, dto.Isbn);
        if (dto.IsIssued) copy.Issue();
        return copy;
    }

    public override string ToString() => $"{Id,-10} | ISBN: {Isbn,-18} | Видано: {(_isIssued ? "Так" : "Ні")}";
}