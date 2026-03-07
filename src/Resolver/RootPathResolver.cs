using FileSystem.Mcp.Server.Services;

namespace FileSystem.Mcp.Server.Resolver;

internal class RootPathResolver
{
    private readonly RootProvider _rootProvider;

    public RootPathResolver(RootProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public string Resolve(string uri)
    {
        var schema = new Uri(uri, UriKind.RelativeOrAbsolute);
        if (!schema.Scheme.Contains("fs", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported URI scheme '{schema.Scheme}'. Only 'fs' scheme is supported for file system paths.");
        }

        var rootName = schema.Host;
        if(rootName != "root")
        {
            throw new InvalidOperationException($"Unsupported root name '{rootName}'. Only 'root' is supported as the host in the URI.");
        }

        var path = schema.AbsolutePath.TrimStart('/'); // Remove leading slash if present
        var combinedPath = Path.GetFullPath(Path.Combine(_rootProvider.RootPath, path));

        if(combinedPath.StartsWith("..") ||!combinedPath.StartsWith(_rootProvider.RootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Resolved path '{combinedPath}' escapes the configured root directory.");
        }

        return combinedPath;
    }
}