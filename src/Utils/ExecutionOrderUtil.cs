using FileSystem.Mcp.Server.Models;

namespace FileSystem.Mcp.Server.Utils;

/// <summary>
/// Utility class to determine execution order of batch operations based on their dependencies. 
/// </summary>
public class ExecutionOrderUtil
{
    /// <summary>
    /// Sorts batch operations by their dependencies.
    /// </summary>
    /// <param name="operations"></param>
    /// <returns></returns>
    public List<string> SortOperationsByDependencies(List<BatchOperation> operations)
    {
        var operationsDict = operations.ToDictionary(op => op.Id);
        var visited = new HashSet<string>();
        var orderedOperations = new List<string>();
        var visting = new HashSet<string>();

        foreach (var op in operations)
        {
            if (!visited.Contains(op.Id))
                VisitAndDetectCycle(op, operationsDict, visited, visting, orderedOperations);
        }
        return orderedOperations;
    }


    /// <summary>
    /// Recursion Helper function to visit each operation and its dependencies, while also detecting cycles in the dependency graph.
    /// </summary>
    /// <param name="op">Current Operation</param>
    /// <param name="operationsDict"> Operation Dictionary </param>
    /// 
    /// <param name="visited"> Set to track visited operations ( This is stored at end of the recurstion call)</param>
    /// 
    /// <param name="visiting"> Set to track operations in the current visiting path (This marks the current path) 
    /// If this is true without "visited" being true, means cycle exists as this should be the first time we visit this in current recursion stack </param>
    /// 
    /// <param name="orderedOperations"> List to store the ordered operations </param>
    /// TODO: This is basically doing cycle detection in directed graph using DFS. We can optimize this by using Kahn's algorithm for topological sorting which uses BFS and is more efficient for large graphs, or think of something more sophisticated if we want to allow some level of parallelism in execution.
    private void VisitAndDetectCycle(BatchOperation op, Dictionary<string, BatchOperation> operationsDict, HashSet<string> visited, HashSet<string> visiting, List<string> orderedOperations)
    {
        if (visited.Contains(op.Id))
            return;

        if (visiting.Contains(op.Id))
            throw new InvalidOperationException($"Cycle detected in operation dependencies involving operation '{op.Id}'.");

        visiting.Add(op.Id);

        if (op.DependsOnOperationIds != null)
        {
            foreach (var depId in op.DependsOnOperationIds)
            {
                if (!operationsDict.TryGetValue(depId, out var depOp))
                    throw new InvalidOperationException($"Operation '{op.Id}' depends on unknown operation ID '{depId}'.");

                VisitAndDetectCycle(depOp, operationsDict, visited, visiting, orderedOperations);
            }
        }

        visiting.Remove(op.Id);
        visited.Add(op.Id);
        orderedOperations.Add(op.Id);
    }
}