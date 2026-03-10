using System.ComponentModel;
using FileSystem.Mcp.Server.Services;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Tools;

/// <summary>
/// MCP Server tools for safe filesystem operations within a sandboxed root directory.
/// All operations are validated to prevent directory traversal attacks.
/// </summary>
[McpServerToolType]
internal class FileSystemTools
{
    private readonly IFileSystemService _fileSystemService;
    private readonly RootProvider  _rootProvider;

    public FileSystemTools(IFileSystemService fileSystemService, RootProvider rootProvider)
    {
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
    }

    [McpServerTool]
    [Description("Gets the current root directory that all file operations are sandboxed to.")]
    public Dictionary<string, string> GetCurrentRootDirectory()
    {
        return new Dictionary<string, string>
        {
            { "rootDirectory", _rootProvider.RootPath },
            { "description", "All file operations are restricted to this directory and its subdirectories." }
        };
    }

    [McpServerTool]
    [Description("Reads content from a file at the specified path.")]
    public string ReadFileContent([Description("The path of the file to read.")] string path)
    {
        return _fileSystemService.ReadFile(path);
    }

    [McpServerTool]
    [Description("Writes content to a file at the specified path. Creates directories if they don't exist.")]
    public string WriteFileContent([Description("The path of the file to write.")] string path, [Description("The content to write to the file.")] string content)
    {
        _fileSystemService.WriteFile(path, content);
        return $"Content written to file at path '{path}' successfully.";
    }

    [McpServerTool]
    [Description("Creates a new directory at the specified path.")]
    public string CreateDirectory([Description("The path of the directory to create.")] string path)
    {
        _fileSystemService.CreateDirectory(path);
        return $"Directory created at path '{path}' successfully.";
    }

    [McpServerTool]
    [Description("Deletes a file at the specified path.")]
    public string DeleteFile([Description("The path of the file to delete.")] string path)
    {
        _fileSystemService.DeleteFile(path);
        return $"File at path '{path}' deleted successfully.";
    }

    [McpServerTool]
    [Description("Deletes a directory and all its contents at the specified path.")]
    public string DeleteDirectory([Description("The path of the directory to delete.")] string path)
    {
        _fileSystemService.DeleteDirectory(path);
        return $"Directory at path '{path}' deleted successfully.";
    }

    [McpServerTool]
    [Description("Copies a file from source to destination within the sandboxed root directory.")]
    public string CopyFile([Description("The source file path.")] string sourcePath, [Description("The destination file path.")] string destinationPath)
    {
        _fileSystemService.CopyFile(sourcePath, destinationPath);
        return $"File copied from '{sourcePath}' to '{destinationPath}' successfully.";
    }

    [McpServerTool]
    [Description("Moves a file from source to destination within the sandboxed root directory.")]
    public string MoveFile([Description("The source file path.")] string sourcePath, [Description("The destination file path.")] string destinationPath)
    {
        _fileSystemService.MoveFile(sourcePath, destinationPath);
        return $"File moved from '{sourcePath}' to '{destinationPath}' successfully.";
    }
}