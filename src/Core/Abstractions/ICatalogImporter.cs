using System.Collections.Generic;
using System.Threading;
using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    // Замість Task<List> тепер повертаємо потоковий IAsyncEnumerable
    IAsyncEnumerable<BookCopy> LoadAsync(string path, CancellationToken ct = default);
}