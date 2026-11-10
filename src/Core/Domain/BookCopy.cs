using System;
using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    private bool _isIssued; // Класичне приватне поле з підкресленням

    public string Id { get; }
    public string Isbn { get; }
    public bool IsIssued => _isIssued; // Властивість лише для читання (стрілка)

    // ДОДАНО: Поля для збереження інформації про читача та дедлайн (для звітів)
    public string? ReaderId { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }

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

    // ЗМІНЕНО: Тепер приймає і зберігає дані про видачу
    public void Issue(string readerId, DateTimeOffset dueDate)
    {
        if (_isIssued)
            throw new InvalidOperationException($"Примірник {Id} вже виданий, повторна видача неможлива.");
        
        _isIssued = true;
        ReaderId = readerId;
        DueDate = dueDate;
    }

    // ЗМІНЕНО: Очищає дані про видачу при поверненні
    public void Return()
    {
        if (!_isIssued)
            throw new InvalidOperationException($"Примірник {Id} не можна повернути, бо він не виданий.");
        
        _isIssued = false;
        ReaderId = null;
        DueDate = null;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, _isIssued);

    public static BookCopy FromDto(BookCopyDto dto)
    {
        var copy = Create(dto.Id, dto.Isbn);
        if (dto.IsIssued) 
        {
            // Звертаємося напряму до приватного поля, 
            // щоб не ламати сумісність зі старим DTO
            copy._isIssued = true; 
        }
        return copy;
    }

    public override string ToString() => $"{Id,-10} | ISBN: {Isbn,-18} | Видано: {(_isIssued ? "Так" : "Ні")}";
}