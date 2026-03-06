namespace FileSystem.Mcp.Server.Exceptions;

/// <summary>
/// Raised when batch operation validation fails.
/// </summary>
public class BatchValidationException : FileSystemException
{
    public BatchValidationException(string message) : base(message) { }
    
    public BatchValidationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
