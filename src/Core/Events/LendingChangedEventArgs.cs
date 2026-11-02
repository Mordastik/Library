using System;

namespace Core.Events;

public sealed class LendingChangedEventArgs(
    string copyId,
    LendingChangeKind kind,
    string? readerId = null,
    DateTimeOffset? dueDate = null) : EventArgs
{
    public string CopyId { get; } = copyId;
    public LendingChangeKind Kind { get; } = kind;
    public string? ReaderId { get; } = readerId;
    public DateTimeOffset? DueDate { get; } = dueDate;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}