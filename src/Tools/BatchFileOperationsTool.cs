using System.ComponentModel;
using System.Text.Json;
using FileSystem.Mcp.Server.Exceptions;
using FileSystem.Mcp.Server.Models;
using FileSystem.Mcp.Server.Services;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Tools;

/// <summary>
/// MCP Server tool for batch filesystem operations.
/// Enables efficient, token-optimized execution of multiple file operations in a single tool call.
/// </summary>
[McpServerToolType]
public class BatchFileOperationsTool
{
    private readonly IBatchFileOperationService _batchService;

    public BatchFileOperationsTool(IBatchFileOperationService batchService)
    {
        _batchService = batchService ?? throw new ArgumentNullException(nameof(batchService));
    }

    [McpServerTool]
    [Description("Execute multiple filesystem operations in a single batch call. " +
        "Supports read, write, copy, move, delete, and directory operations. " +
        "Returns individual results and execution statistics. " +
        "Operations are executed in dependency order if specified. " +
        "Use this for token-efficient bulk operations instead of multiple single-operation tools.")]
    public async Task<Dictionary<string, object>> ExecuteBatchFileOperations(
        [Description("JSON array of operations. Each operation must have: id, type, and path. " +
            "Types: 'Read', 'Write', 'CreateDirectory', 'DeleteFile', 'DeleteDirectory', 'CopyFile', 'MoveFile', 'GetDirectoryStructure', 'FileExists', 'DirectoryExists'. " +
            "Example: [{\"id\":\"op1\",\"type\":\"Write\",\"path\":\"file.txt\",\"content\":\"hello\"},{\"id\":\"op2\",\"type\":\"Read\",\"path\":\"file.txt\"}]")] 
        string operationsJson,
        
        [Description("Execution mode: 'FailFast' (stops at first error), 'ContinueOnError' (executes all, reports all), or 'Transactional' (all-or-nothing). Default: 'FailFast'")] 
        string mode = "FailFast",
        
        [Description("Maximum number of operations allowed (default: 100)")] 
        int maxBatchSize = 100,
        
        [Description("Maximum total file size in bytes for write operations (default: 100MB)")] 
        long maxTotalFileSizeBytes = 100 * 1024 * 1024,
        
        [Description("Timeout in milliseconds (default: 30000)")] 
        int timeoutMilliseconds = 30000)
    {
        try
        {
            // Parse operations from JSON
            var operations = ParseOperationsJson(operationsJson);
            
            // Parse execution mode
            if (!Enum.TryParse<BatchExecutionMode>(mode, out var executionMode))
            {
                throw new BatchValidationException(
                    $"Invalid execution mode '{mode}'. Valid modes: FailFast, ContinueOnError, Transactional");
            }

            var options = new BatchExecutionOptions
            {
                Mode = executionMode,
                MaxBatchSize = maxBatchSize,
                MaxTotalFileSizeBytes = maxTotalFileSizeBytes,
                TimeoutMilliseconds = timeoutMilliseconds
            };

            // Validate batch before execution
            var validationErrors = _batchService.ValidateBatch(operations, options);
            if (validationErrors != null)
            {
                return new Dictionary<string, object>
                {
                    { "success", false },
                    { "error", "Batch validation failed" },
                    { "validationErrors", validationErrors.ToArray() },
                    { "results", Array.Empty<object>() }
                };
            }

            // Execute batch
            using (var cts = new System.Threading.CancellationTokenSource(timeoutMilliseconds))
            {
                var result = await _batchService.ExecuteBatchAsync(operations, options, cts.Token);

                return new Dictionary<string, object>
                {
                    { "success", result.Success },
                    { "results", FormatResults(result.Results) },
                    { "errorSummary", result.ErrorSummary ?? "No errors" },
                    { "statistics", FormatStatistics(result.Statistics) }
                };
            }
        }
        catch (OperationCanceledException)
        {
            return new Dictionary<string, object>
            {
                { "success", false },
                { "error", "Batch execution timed out" },
                { "results", Array.Empty<object>() }
            };
        }
        catch (BatchValidationException ex)
        {
            return new Dictionary<string, object>
            {
                { "success", false },
                { "error", ex.Message },
                { "results", Array.Empty<object>() }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "success", false },
                { "error", ex.Message },
                { "results", Array.Empty<object>() }
            };
        }
    }

    /// <summary>
    /// Validates a batch of operations without executing them.
    /// Useful for checking if a batch is valid before committing to execution.
    /// </summary>
    [McpServerTool]
    [Description("Validate a batch of operations without executing them. " +
        "Returns validation errors if found, otherwise confirms the batch is valid.")]
    public Dictionary<string, object> ValidateBatchFileOperations(
        [Description("JSON array of operations to validate (same format as ExecuteBatchFileOperations)")] 
        string operationsJson,
        
        [Description("Maximum batch size (default: 100)")] 
        int maxBatchSize = 100,
        
        [Description("Maximum total file size in bytes (default: 100MB)")] 
        long maxTotalFileSizeBytes = 100 * 1024 * 1024)
    {
        try
        {
            var operations = ParseOperationsJson(operationsJson);
            var options = new BatchExecutionOptions
            {
                MaxBatchSize = maxBatchSize,
                MaxTotalFileSizeBytes = maxTotalFileSizeBytes
            };

            var validationErrors = _batchService.ValidateBatch(operations, options);

            if (validationErrors == null)
            {
                return new Dictionary<string, object>
                {
                    { "valid", true },
                    { "errors", Array.Empty<string>() },
                    { "operationCount", operations.Count }
                };
            }

            return new Dictionary<string, object>
            {
                { "valid", false },
                { "errors", validationErrors.ToArray() },
                { "operationCount", operations.Count }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "valid", false },
                { "errors", new[] { ex.Message } },
                { "operationCount", 0 }
            };
        }
    }

    /// <summary>
    /// Parses JSON string into batch operations.
    /// </summary>
    private List<BatchOperation> ParseOperationsJson(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json, options);
            var operations = new List<BatchOperation>();

            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var operation = ParseOperationElement(item);
                    operations.Add(operation);
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                // Single operation wrapped as array
                var operation = ParseOperationElement(jsonElement);
                operations.Add(operation);
            }

            return operations;
        }
        catch (JsonException ex)
        {
            throw new BatchValidationException($"Failed to parse operations JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses a single operation from JSON element.
    /// </summary>
    private BatchOperation ParseOperationElement(JsonElement element)
    {
        var obj = element.EnumerateObject().ToDictionary(
            prop => prop.Name.ToLowerInvariant(),
            prop => prop.Value);

        // Extract required fields
        if (!obj.TryGetValue("id", out var idElement) || idElement.ValueKind != JsonValueKind.String)
            throw new BatchValidationException("Operation must have a 'id' (string) field");

        if (!obj.TryGetValue("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            throw new BatchValidationException("Operation must have a 'type' (string) field");

        if (!obj.TryGetValue("path", out var pathElement) || pathElement.ValueKind != JsonValueKind.String)
            throw new BatchValidationException("Operation must have a 'path' (string) field");

        var id = idElement.GetString() ?? throw new BatchValidationException("Operation ID cannot be empty");
        var typeStr = typeElement.GetString() ?? throw new BatchValidationException("Operation type cannot be empty");
        var path = pathElement.GetString() ?? throw new BatchValidationException("Operation path cannot be empty");

        if (!Enum.TryParse<BatchOperationType>(typeStr, ignoreCase: true, out var type))
        {
            throw new BatchValidationException(
                $"Invalid operation type '{typeStr}'. Valid types: {string.Join(", ", Enum.GetNames(typeof(BatchOperationType)))}");
        }

        // Extract optional fields
        var targetPath = obj.TryGetValue("targetpath", out var tpElement) && tpElement.ValueKind == JsonValueKind.String 
            ? tpElement.GetString() 
            : null;

        var content = obj.TryGetValue("content", out var cElement) && cElement.ValueKind == JsonValueKind.String 
            ? cElement.GetString() 
            : null;

        var dependsOn = obj.TryGetValue("dependsonoperationid", out var depElement) && depElement.ValueKind == JsonValueKind.String 
            ? depElement.GetString() 
            : null;

        return new BatchOperation
        {
            Id = id,
            Type = type,
            Path = path,
            TargetPath = targetPath,
            Content = content,
            DependsOnOperationId = dependsOn
        };
    }

    /// <summary>
    /// Formats operation results for JSON output.
    /// </summary>
    private object[] FormatResults(BatchOperationResult[] results)
    {
        return results.Select(r => new
        {
            operationId = r.OperationId,
            success = r.Success,
            data = r.Data,
            error = r.Error
        }).Cast<object>().ToArray();
    }

    /// <summary>
    /// Formats execution statistics for JSON output.
    /// </summary>
    private object FormatStatistics(BatchExecutionStatistics stats)
    {
        return new
        {
            totalOperations = stats.TotalOperations,
            successfulOperations = stats.SuccessfulOperations,
            failedOperations = stats.FailedOperations,
            skippedOperations = stats.SkippedOperations,
            executionTimeMilliseconds = stats.ExecutionTimeMilliseconds
        };
    }
}
