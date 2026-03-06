namespace FileSystem.Mcp.Server.Exceptions;

/// <summary>
/// Raised when a batch operation execution fails.
/// </summary>
public class BatchOperationException : FileSystemException
{
    /// <summary>
    /// The ID of the operation that caused the exception.
    /// </summary>
    public string OperationId { get; }

    public BatchOperationException(string operationId, string message) 
        : base(message)
    {
        OperationId = operationId;
    }

    public BatchOperationException(string operationId, string message, Exception innerException) 
        : base(message, innerException)
    {
        OperationId = operationId;
    }
}
