namespace Core.Dto;

public sealed record ImportResult(IReadOnlyList<BookDto> Items, IReadOnlyList<string> Errors);