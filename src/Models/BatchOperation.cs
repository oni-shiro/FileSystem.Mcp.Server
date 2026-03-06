namespace FileSystem.Mcp.Server.Models;

/// <summary>
/// Represents a single filesystem operation to be executed in a batch.
/// </summary>
public record BatchOperation
{
    /// <summary>
    /// Unique identifier for this operation within the batch.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Type of operation to execute.
    /// </summary>
    public required BatchOperationType Type { get; init; }

    /// <summary>
    /// Primary path for the operation.
    /// - For Read/Write/Delete/CreateDirectory: the file or directory path
    /// - For CopyFile/MoveFile: the source path
    /// - For GetDirectoryStructure/FileExists/DirectoryExists: the path to check
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Target path for copy and move operations.
    /// Required for CopyFile and MoveFile operations.
    /// </summary>
    public string? TargetPath { get; init; }

    /// <summary>
    /// Content to write for Write operations.
    /// Required for Write operations.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Optional List<ID> of other operations this one depends on.
    /// Ensures ordering within the batch.
    /// </summary>
    public List<string>? DependsOnOperationIds { get; init; }
}
