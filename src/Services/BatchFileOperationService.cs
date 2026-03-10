using System.Diagnostics;
using FileSystem.Mcp.Server.Exceptions;
using FileSystem.Mcp.Server.Models;
using FileSystem.Mcp.Server.Utils;
using ModelContextProtocol.Protocol;

namespace FileSystem.Mcp.Server.Services;

/// <summary>
/// Implements batch filesystem operations with support for different execution modes.
/// </summary>
internal class BatchFileOperationService : IBatchFileOperationService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ExecutionOrderUtil _executionOrderUtil;
    private readonly RootProvider _rootProvider;

    public BatchFileOperationService(
        IFileSystemService fileSystemService,
        ExecutionOrderUtil executionOrderUtil,
        RootProvider rootProvider)
    {
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _executionOrderUtil = executionOrderUtil ?? throw new ArgumentNullException(nameof(executionOrderUtil));
        _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
    }

    public async Task<BatchExecutionResult> ExecuteBatchAsync(
        IEnumerable<BatchOperation> operations,
        BatchExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        options ??= new BatchExecutionOptions();

        var operationList = operations.ToList();

        // Validate batch
        var validationErrors = ValidateBatch(operationList, options)?.ToList();
        if (validationErrors?.Count > 0)
        {
            throw new BatchValidationException($"Batch validation failed: {string.Join("; ", validationErrors)}");
        }

        var results = new List<BatchOperationResult>();
        var executedOperations = new HashSet<string>();
        int skippedCount = 0;
        bool hasError = false;

        // Build dependency graph for ordering
        var operationDict = operationList.ToDictionary(op => op.Id);
        var executionOrder = _executionOrderUtil.SortOperationsByDependencies(operationList);

        foreach (var operationId in executionOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (hasError && options.Mode == BatchExecutionMode.FailFast)
            {
                // Skip remaining operations in FailFast mode
                skippedCount++;
                continue;
            }

            var operation = operationDict[operationId];
            try
            {
                var result = await ExecuteSingleOperationAsync(operation, cancellationToken);
                results.Add(result);
                executedOperations.Add(operationId);

                if (!result.Success)
                {
                    hasError = true;
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                results.Add(new BatchOperationResult
                {
                    OperationId = operation.Id,
                    Success = false,
                    Error = ex.Message,
                    Data = null
                });

                if (options.Mode == BatchExecutionMode.FailFast)
                {
                    // Mark remaining as skipped
                    skippedCount = operationList.Count - results.Count;
                    break;
                }
            }
        }

        stopwatch.Stop();

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);

        return new BatchExecutionResult
        {
            Success = !hasError,
            Results = results.ToArray(),
            ErrorSummary = hasError ? $"{failureCount} operation(s) failed out of {operationList.Count}" : null,
            Statistics = new BatchExecutionStatistics
            {
                TotalOperations = operationList.Count,
                SuccessfulOperations = successCount,
                FailedOperations = failureCount,
                SkippedOperations = skippedCount,
                ExecutionTimeMilliseconds = stopwatch.ElapsedMilliseconds
            }
        };
    }

    public IEnumerable<string>? ValidateBatch(
        IEnumerable<BatchOperation> operations,
        BatchExecutionOptions? options = null)
    {
        options ??= new BatchExecutionOptions();
        var errors = new List<string>();
        var operationList = operations.ToList();

        // Check batch size
        if (operationList.Count > options.MaxBatchSize)
        {
            errors.Add($"Batch size ({operationList.Count}) exceeds maximum ({options.MaxBatchSize})");
        }

        // Check for duplicate IDs
        var ids = operationList.Select(op => op.Id).ToList();
        var duplicateIds = ids.GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            errors.Add($"Duplicate operation IDs found: {string.Join(", ", duplicateIds)}");
        }

        long totalFileSize = 0;

        // Validate individual operations
        foreach (var operation in operationList)
        {
            var opErrors = ValidateSingleOperation(operation);
            if (opErrors?.Count > 0)
            {
                errors.AddRange(opErrors.Select(err => $"Operation '{operation.Id}': {err}"));
            }

            // Sum up file sizes for write operations
            if (operation.Type == BatchOperationType.Write && !string.IsNullOrEmpty(operation.Content))
            {
                totalFileSize += System.Text.Encoding.UTF8.GetByteCount(operation.Content);
            }
        }

        // Check total file size
        if (totalFileSize > options.MaxTotalFileSizeBytes)
        {
            errors.Add($"Total file size ({totalFileSize} bytes) exceeds maximum ({options.MaxTotalFileSizeBytes} bytes)");
        }

        // Validate dependencies
        var validIds = new HashSet<string>(ids);
        foreach (var operation in operationList)
        {
            if (operation.DependsOnOperationIds != null &&
                !operation.DependsOnOperationIds.All(depId => validIds.Contains(depId)))
            {
                errors.Add($"Operation '{operation.Id}' depends on non-existent operation '{operation.DependsOnOperationIds}'");
            }
        }

        return errors.Count > 0 ? errors : null;
    }

    /// <summary>
    /// Validates a single operation for required fields and consistency.
    /// </summary>
    private List<string>? ValidateSingleOperation(BatchOperation operation)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(operation.Id))
            errors.Add("Operation ID cannot be null or empty");

        if (string.IsNullOrWhiteSpace(operation.Path))
            errors.Add("Operation path cannot be null or empty");

        return operation.Type switch
        {
            BatchOperationType.Write when string.IsNullOrEmpty(operation.Content) =>
                new List<string> { "Write operations require Content" },
            
            BatchOperationType.CopyFile when string.IsNullOrEmpty(operation.TargetPath) =>
                new List<string> { "CopyFile operations require TargetPath" },
            
            BatchOperationType.MoveFile when string.IsNullOrEmpty(operation.TargetPath) =>
                new List<string> { "MoveFile operations require TargetPath" },
            
            _ => errors.Count > 0 ? errors : null
        };
    }

    /// <summary>
    /// Executes a single operation and returns the result.
    /// </summary>
    private async Task<BatchOperationResult> ExecuteSingleOperationAsync(
        BatchOperation operation,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var data = operation.Type switch
                {
                    BatchOperationType.Read => ExecuteRead(operation),
                    BatchOperationType.Write => ExecuteWrite(operation),
                    BatchOperationType.CreateDirectory => ExecuteCreateDirectory(operation),
                    BatchOperationType.DeleteFile => ExecuteDeleteFile(operation),
                    BatchOperationType.DeleteDirectory => ExecuteDeleteDirectory(operation),
                    BatchOperationType.CopyFile => ExecuteCopyFile(operation),
                    BatchOperationType.MoveFile => ExecuteMoveFile(operation),
                    BatchOperationType.FileExists => ExecuteFileExists(operation),
                    BatchOperationType.DirectoryExists => ExecuteDirectoryExists(operation),
                    _ => throw new InvalidOperationException($"Unknown operation type: {operation.Type}")
                };

                return new BatchOperationResult
                {
                    OperationId = operation.Id,
                    Success = true,
                    Data = data,
                    Error = null
                };
            }
            catch (Exception ex)
            {
                return new BatchOperationResult
                {
                    OperationId = operation.Id,
                    Success = false,
                    Data = null,
                    Error = ex.Message
                };
            }
        }, cancellationToken);
    }

    private object? ExecuteRead(BatchOperation operation)
    {
        return _fileSystemService.ReadFile(operation.Path);
    }

    private object? ExecuteWrite(BatchOperation operation)
    {
        _fileSystemService.WriteFile(operation.Path, operation.Content ?? "");
        return null;
    }

    private object? ExecuteCreateDirectory(BatchOperation operation)
    {
        _fileSystemService.CreateDirectory(operation.Path);
        return null;
    }

    private object? ExecuteDeleteFile(BatchOperation operation)
    {
        _fileSystemService.DeleteFile(operation.Path);
        return null;
    }

    private object? ExecuteDeleteDirectory(BatchOperation operation)
    {
        _fileSystemService.DeleteDirectory(operation.Path);
        return null;
    }

    private object? ExecuteCopyFile(BatchOperation operation)
    {
        _fileSystemService.CopyFile(operation.Path, operation.TargetPath!);
        return null;
    }

    private object? ExecuteMoveFile(BatchOperation operation)
    {
        _fileSystemService.MoveFile(operation.Path, operation.TargetPath!);
        return null;
    }
    private object? ExecuteFileExists(BatchOperation operation)
    {
        var fullPath = _rootProvider.Resolve(operation.Path);
        return File.Exists(fullPath);
    }

    private object? ExecuteDirectoryExists(BatchOperation operation)
    {
        var fullPath = _rootProvider.Resolve(operation.Path);
        return Directory.Exists(fullPath);
    }
}
