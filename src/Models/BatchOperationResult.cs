namespace FileSystem.Mcp.Server.Models;

/// <summary>
/// Represents the result of a single operation in a batch execution.
/// </summary>
public record BatchOperationResult
{
    /// <summary>
    /// The ID of the operation this result corresponds to.
    /// </summary>
    public required string OperationId { get; init; }

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The result data from the operation.
    /// - For Read: the file content
    /// - For GetDirectoryStructure: the directory structure object
    /// - For FileExists/DirectoryExists: boolean value
    /// - For successful write/copy/move/delete operations: null
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Error message if the operation failed.
    /// Null or empty if operation succeeded.
    /// </summary>
    public string? Error { get; init; }
}
