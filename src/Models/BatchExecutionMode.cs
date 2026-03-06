namespace FileSystem.Mcp.Server.Models;

/// <summary>
/// Defines how batch operations should be executed.
/// </summary>
public enum BatchExecutionMode
{
    /// <summary>
    /// Stop execution at the first error.
    /// Operations after the failed one will not be executed.
    /// </summary>
    FailFast,
    
    /// <summary>
    /// Continue executing all operations even if some fail.
    /// All results are collected and returned with error information.
    /// </summary>
    ContinueOnError,
    
    /// <summary>
    /// Execute operations atomically - all succeed or all fail.
    /// Uses file staging and rollback for consistency.
    /// </summary>
    Transactional
}
