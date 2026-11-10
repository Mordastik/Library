namespace Core.Reports;

// Рядок для статистики (наприклад, скільки примірників певного ISBN маємо)
public sealed record GroupSummary(string Key, int TotalCopies, int IssuedCopies);