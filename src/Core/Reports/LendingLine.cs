using System;

namespace Core.Reports;

// Рядок для звіту по прострочених/виданих книгах
public sealed record LendingLine(string CopyId, string Isbn, string ReaderId, DateTimeOffset DueDate);