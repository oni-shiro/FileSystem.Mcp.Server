namespace FileSystem.Mcp.Server.Models;

public class FileNode
{
    public string? Name { get; set; }
    public string? RelativePath { get; set; }
    public long Size { get; set; }
    public string Extension { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public bool IsReadonly { get; set; }
}