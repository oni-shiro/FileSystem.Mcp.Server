using FileSystem.Mcp.Server.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileSystem.Mcp.Server.Extensions;

/// <summary>
/// Extension methods for configuring filesystem services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the FileSystemService with a configurable root directory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="rootDirectory">The root directory for filesystem operations. 
    /// If not provided, uses environment variable MCP_ROOT_DIR or current directory.</param>
    /// <returns>The service collection for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when root directory is invalid or inaccessible.</exception>
    public static IServiceCollection AddFileSystemService(
        this IServiceCollection services,
        string? rootDirectory = null)
    {
        // Resolve the root directory from provided argument, environment variable, or current directory
        string resolvedRootDirectory = rootDirectory
            ?? Environment.GetEnvironmentVariable("MCP_ROOT_DIR")
            ?? Directory.GetCurrentDirectory();

        // Normalize the path
        resolvedRootDirectory = Path.GetFullPath(resolvedRootDirectory);

        // Validate directory exists
        if (!Directory.Exists(resolvedRootDirectory))
            throw new DirectoryNotFoundException(
                $"The root directory '{resolvedRootDirectory}' does not exist.");

        // Log to stderr for debugging
        Console.Error.WriteLine(
            $"FileSystem MCP Server initialized with root directory: {resolvedRootDirectory}");

        // Register as singleton
        services.AddSingleton(new FileSystemService(resolvedRootDirectory));
        services.AddSingleton<IFileSystemService>(provider =>
            provider.GetRequiredService<FileSystemService>());

        return services;
    }

    /// <summary>
    /// Adds the BatchFileOperationService for efficient bulk file operations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for fluent chaining.</returns>
    /// <remarks>
    /// Requires AddFileSystemService to be called first.
    /// Provides token-efficient batch execution of multiple filesystem operations.
    /// </remarks>
    public static IServiceCollection AddBatchFileOperationService(
        this IServiceCollection services)
    {
        services.AddSingleton<IBatchFileOperationService, BatchFileOperationService>();
        return services;
    }
}
