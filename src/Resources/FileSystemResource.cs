using System.ComponentModel;
using System.Text.Json;
using FileSystem.Mcp.Server.Services;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Resources;

[McpServerResourceType]
internal class FileSystemResource
{
    private readonly IFileSystemService _fileSystemService;
    private readonly RootProvider _rootProvider;
    private const int MaxDepth = 5;

    public FileSystemResource(IFileSystemService fileSystemService, RootProvider rootProvider)
    {
        _fileSystemService = fileSystemService;
        _rootProvider = rootProvider;
    }

    [McpServerResource(
        UriTemplate = "fs://root/directoryMap",
        Name = "GetDirectoryMap",
        MimeType = "application/json"
    )]
    [Description("Returns a JSON representation of the directory structure starting from the root path. This only includes files and directories up to a depth of 5 levels to prevent excessive data transfer.")]
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

    [McpServerResource(
        UriTemplate = "fs://lens/{filePath}",
        Name = "GetFileContents",
        MimeType = "text/plain"
    )]
    [Description("Returns the contents of the specified file as plain text. The filePath parameter should be a relative path from the root directory. For example, if the root directory is 'C:/Files' and you want to access 'C:/Files/Documents/report.txt', you would use 'Documents/report.txt' as the filePath. Don't always need to use this, you can also use the 'fs://root/directoryMap' endpoint to explore the directory structure and find the exact path to the file you want to access.")]
    public TextResourceContents TryGetFileContents(string filePath)
    {
        try
        {

            var fullPath = _rootProvider.Resolve(filePath);
            var fileContents = _fileSystemService.ReadFile(fullPath);

            return new TextResourceContents
            {
                Uri = $"fs://lens/file?path={filePath}",
                MimeType = "text/markdown",
                Text = fileContents
            };
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error accessing file at path '{filePath}': {ex.Message}", ex);
        }
    }
}