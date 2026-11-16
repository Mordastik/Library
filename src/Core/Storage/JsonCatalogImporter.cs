using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    public async IAsyncEnumerable<BookCopy> LoadAsync(
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, useAsync: true);
        
        var dtos = await JsonSerializer.DeserializeAsync<List<BookCopyDto>>(
            stream, cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            
            // Штучна затримка на 100мс для красивої візуалізації прогресу
            await Task.Delay(100, ct); 
            
            BookCopy copy;
            try { copy = BookCopy.FromDto(dtos[i]); }
            catch (ArgumentException ex) { throw new DomainException($"Запис {i + 1}: {ex.Message}", ex); }
            
            yield return copy;
        }
    }
}