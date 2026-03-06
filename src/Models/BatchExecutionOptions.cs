namespace FileSystem.Mcp.Server.Models;

/// <summary>
/// Configuration options for batch execution.
/// </summary>
public record BatchExecutionOptions
{
    /// <summary>
    /// How to handle execution flow. Default: FailFast.
    /// </summary>
    public BatchExecutionMode Mode { get; init; } = BatchExecutionMode.FailFast;

    /// <summary>
    /// Maximum number of operations allowed in a single batch. Default: 100.
    /// </summary>
    public int MaxBatchSize { get; init; } = 100;

    /// <summary>
    /// Maximum total size in bytes for all file write operations. Default: 100MB.
    /// </summary>
    public long MaxTotalFileSizeBytes { get; init; } = 100 * 1024 * 1024;

    /// <summary>
    /// Timeout for the entire batch execution in milliseconds. Default: 30000ms.
    /// </summary>
    public int TimeoutMilliseconds { get; init; } = 30000;

    /// <summary>
    /// Whether to validate all paths before executing any operations. Default: true.
    /// </summary>
    public bool ValidatePathsBeforeBatch { get; init; } = true;
}
