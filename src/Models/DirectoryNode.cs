namespace FileSystem.Mcp.Server.Models;

public class DirectoryNode
{
    public string? Path { get; set; }
    public List<DirectoryNode> Children { get; set; } = new List<DirectoryNode>();
    public List<FileNode> Files { get; set; } = new List<FileNode>();
    public int NumberOfFiles => Files.Count;
    public DateTime LastModified { get; set; }
}