using System;
using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public bool IsIssued { get; private set; } // Властивість із private set для інкапсуляції

    private BookCopy(string id, string isbn, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        IsIssued = isIssued;
    }

    // Фабричний метод
    public static BookCopy Create(string id, string isbn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim(), false); // Новий примірник не виданий
    }

    // Метод зміни стану
    public void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException($"Примірник {Id} вже виданий, повторна видача неможлива");
        IsIssued = true;
    }

    // Метод зміни стану
    public void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException($"Примірник {Id} не можна повернути, бо він не виданий");
        IsIssued = false;
    }

    // Мапінг у формат DTO і назад
    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued);

    public static BookCopy FromDto(BookCopyDto dto)
    {
        var copy = Create(dto.Id, dto.Isbn);
        if (dto.IsIssued) copy.Issue();
        return copy;
    }
}