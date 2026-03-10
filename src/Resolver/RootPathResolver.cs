using FileSystem.Mcp.Server.Services;

namespace FileSystem.Mcp.Server.Resolver;

internal class RootPathResolver
{
    private readonly RootProvider _rootProvider;

    public RootPathResolver(RootProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public string Resolve(string path)
    {
        var trimmed = path.TrimStart('/'); // Remove leading slash if present
        var combinedPath = Path.GetFullPath(Path.Combine(_rootProvider.RootPath, trimmed));
        return _rootProvider.Resolve(combinedPath);
    }
}