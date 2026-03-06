namespace FileSystem.Mcp.Server.Exceptions;

/// <summary>
/// Base exception for filesystem operations.
/// </summary>
public class FileSystemException : Exception
{
    public FileSystemException(string message) : base(message) { }
    
    public FileSystemException(string message, Exception innerException) 
        : base(message, innerException) { }
}
