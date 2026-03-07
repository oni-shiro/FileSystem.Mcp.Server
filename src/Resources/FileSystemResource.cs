using System.ComponentModel;
using System.Text.Json;
using FileSystem.Mcp.Server.Resolver;
using FileSystem.Mcp.Server.Services;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Resources;

[McpServerResourceType]
internal class FileSystemResource
{
    private readonly IFileSystemService _fileSystemService;
    private readonly RootPathResolver _rootPathResolver;
    private readonly RootProvider _rootProvider;
    private const int MaxDepth = 10;

    public FileSystemResource(IFileSystemService fileSystemService, RootPathResolver rootPathResolver, RootProvider rootProvider)
    {
        _fileSystemService = fileSystemService;
        _rootPathResolver = rootPathResolver;
        _rootProvider = rootProvider;

        Console.WriteLine(_fileSystemService.BuildDirectoryMap(_rootProvider.RootPath, 4).ToString());
    }

    [McpServerResource(
        UriTemplate = "fs://root/directoryMap",
        Name = "GetDirectoryMap",
        MimeType = "application/json"
    )]
    [Description("Returns a JSON representation of the directory structure starting from the root path. This only includes files and directories up to a depth of 10 levels to prevent excessive data transfer.")]
    public TextResourceContents TryGetDirectoryMap()
    {
        var directoryNode = _fileSystemService.BuildDirectoryMap(_rootProvider.RootPath, MaxDepth);

        return new TextResourceContents
        {
            Uri = "fs://root/directoryMap",
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(directoryNode, new JsonSerializerOptions { WriteIndented = true })
        };
    }
}