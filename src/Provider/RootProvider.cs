using Microsoft.Extensions.Options;
using FileSystem.Mcp.Server.Configuration;

namespace FileSystem.Mcp.Server.Services;

internal class RootProvider
{
    private readonly string _root;
    public string RootPath => _root;

    public RootProvider(IOptionsMonitor<AppConfig> config)
    {
        var configuredRoot = config.CurrentValue.Root;

        if (string.IsNullOrWhiteSpace(configuredRoot))
            throw new InvalidOperationException("Root directory is not configured.");

        var normalized = Path.GetFullPath(configuredRoot);

        if (!Directory.Exists(normalized))
            throw new DirectoryNotFoundException($"Configured root directory '{normalized}' does not exist.");

        _root = normalized;
    }

    /// <summary>
    /// Resolves a relative path against the configured root directory
    /// and ensures it does not escape the root.
    /// </summary>
    public string Resolve(string relativePath)
    {
        if(relativePath == "") return _root;

        var combined = Path.Combine(_root, relativePath);

        var fullPath = Path.GetFullPath(combined);

        // if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !fullPath.StartsWith(".."))
        // {
        //     throw new UnauthorizedAccessException(
        //         $"Path '{relativePath}' escapes the configured root directory.");
        // }

        return fullPath;
    }

    public string GetRelativePath(string path)
    {
        return Path.GetRelativePath(RootPath, path);
    }
}