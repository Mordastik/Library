using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileBookStore(string path) : IBookStore
{
    // Оновлені налаштування для коректного збереження кирилиці та формату CamelCase
    private static readonly JsonSerializerOptions Options = new() 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    private readonly Dictionary<string, BookCopy> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (!File.Exists(_path)) 
        { 
            _loaded = true; 
            return; 
        }
        
        // Використовуємо Options під час читання, щоб врахувати CamelCase
        var dtos = JsonSerializer.Deserialize<List<BookCopyDto>>(File.ReadAllText(_path), Options) ?? [];
        _cache.Clear();
        foreach (var dto in dtos)
        {
            var copy = BookCopy.FromDto(dto);
            _cache[copy.Id] = copy;
        }
        _loaded = true;
    }

    private void Flush()
    {
        var dtos = _cache.Values.Select(p => p.ToDto()).ToList();
        var tmp = _path + ".tmp";
        
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        
        // Атомарний запис: спочатку повністю пишемо в .tmp, потім миттєво підміняємо основний файл
        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, _path, overwrite: true);
    }

    public IReadOnlyList<BookCopy> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public BookCopy? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(BookCopy item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }
}