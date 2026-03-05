using System.ComponentModel;
using System.Diagnostics;
using FileSystem.Mcp.Server.Services;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Tools;

[McpServerToolType]
public class FileSystemTools
{
    [McpServerTool]
    [Description("Gets files and directories in the specified path.")]
    public List<string> GetFilesAndDirectories([Description("The path to list files and directories in.")] string path)
    {
        var entries = FileSystemService.ListDirectoriesAndFiles(path);
        return entries;
    }

    [McpServerTool]
    [Description("Reads content to a file at the specified path.")]
    public string ReadFileContent([Description("The path of the file to read.")] string path)
    {
        return FileSystemService.ReadFile(path);
    }

    [McpServerTool]
    [Description("Writes content to a file at the specified path.")]
    public string WriteFileContent([Description("The path of the file to write.")] string path, [Description("The content to write to the file.")] string content)
    {
        FileSystemService.WriteFile(path, content);
        return $"Content written to file at path '{path}' successfully.";
    }
}