using HeraclesIA.Application.Abstractions;
using System.Text;
using System.Text.Json;

namespace HeraclesIA.DataAccess.Storage;

public sealed class FileStore : IFileStore
{
    private readonly string _root;

    public FileStore(string rootFolder)
    {
        _root = string.IsNullOrWhiteSpace(rootFolder)
            ? throw new ArgumentException("Root folder inválido.", nameof(rootFolder))
            : rootFolder;
    }

    public async Task SaveTextAsync(string relativePath, string content, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, content ?? string.Empty, Encoding.UTF8, ct).ConfigureAwait(false);
    }

    public async Task SaveJsonAsync<T>(string relativePath, T data, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await SaveTextAsync(relativePath, json, ct).ConfigureAwait(false);
    }

    private string GetFullPath(string relativePath)
    {
        relativePath = relativePath.Replace('\\', '/').TrimStart('/');
        return Path.Combine(_root, relativePath);
    }
}
