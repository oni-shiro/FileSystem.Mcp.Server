namespace FileSystem.Mcp.Server.Models;

/// <summary>
/// The result of executing a batch of operations.
/// </summary>
public record BatchExecutionResult
{
    /// <summary>
    /// Whether all operations in the batch succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Array of individual operation results.
    /// Includes successful operations and any that failed.
    /// </summary>
    public required BatchOperationResult[] Results { get; init; }

    /// <summary>
    /// Summary of any errors that occurred.
    /// Non-null only if Success is false.
    /// </summary>
    public string? ErrorSummary { get; init; }

    /// <summary>
    /// Execution statistics.
    /// </summary>
    public required BatchExecutionStatistics Statistics { get; init; }
}

/// <summary>
/// Statistics about the batch execution.
/// </summary>
public record BatchExecutionStatistics
{
    /// <summary>
    /// Total number of operations in the batch.
    /// </summary>
    public int TotalOperations { get; init; }

    /// <summary>
    /// Number of operations that succeeded.
    /// </summary>
    public int SuccessfulOperations { get; init; }

    /// <summary>
    /// Number of operations that failed.
    /// </summary>
    public int FailedOperations { get; init; }

    /// <summary>
    /// Number of operations that were skipped (e.g., due to FailFast mode).
    /// </summary>
    public int SkippedOperations { get; init; }

    /// <summary>
    /// Total time taken to execute the batch in milliseconds.
    /// </summary>
    public long ExecutionTimeMilliseconds { get; init; }
}
