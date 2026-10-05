namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public string CopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public bool IsClosed => ReturnedOn.HasValue;

    private Loan(string id, string copyId, string readerId, DateTime issuedOn, DateTime? returnedOn)
    {
        Id = id;
        CopyId = copyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
    }

    // Фабричний метод
    public static Loan Open(string id, string copyId, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(copyId))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(copyId));
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));
        return new Loan(id.Trim(), copyId.Trim(), readerId.Trim(), issuedOn, null);
    }

    // Метод зміни стану
    public void Close(DateTime returnedOn)
    {
        if (IsClosed)
            throw new InvalidOperationException($"Видача {Id} вже закрита, повторне закриття неможливе");
        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn, "Дата повернення не може бути раніше дати видачі");

        ReturnedOn = returnedOn;
    }

    public override string ToString() => $"Видача {Id}: Примірник {CopyId} для читача {ReaderId} ({(IsClosed ? $"Повернуто {ReturnedOn}" : "Відкрито")})";
}