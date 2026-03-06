using FileSystem.Mcp.Server.Models;

namespace FileSystem.Mcp.Server.Services;

/// <summary>
/// Defines the interface for executing batch filesystem operations.
/// Provides token-efficient and consistent execution of multiple file operations.
/// </summary>
public interface IBatchFileOperationService
{
    /// <summary>
    /// Executes a batch of filesystem operations with specified execution mode.
    /// </summary>
    /// <param name="operations">Array of operations to execute.</param>
    /// <param name="options">Execution options. If null, uses defaults.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing individual operation results and statistics.</returns>
    Task<BatchExecutionResult> ExecuteBatchAsync(
        IEnumerable<BatchOperation> operations,
        BatchExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a batch of operations without executing them.
    /// Checks for missing required fields, invalid paths, and batch constraints.
    /// </summary>
    /// <param name="operations">Array of operations to validate.</param>
    /// <param name="options">Execution options to validate against.</param>
    /// <returns>null if valid, or a collection of validation error messages.</returns>
    IEnumerable<string>? ValidateBatch(
        IEnumerable<BatchOperation> operations,
        BatchExecutionOptions? options = null);
}
