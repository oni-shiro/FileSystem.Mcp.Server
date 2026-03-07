using FileSystem.Mcp.Server.Resolver;
using FileSystem.Mcp.Server.Resources;
using FileSystem.Mcp.Server.Services;
using FileSystem.Mcp.Server.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FileSystem.Mcp.Server.Extensions;

/// <summary>
/// Extension methods for configuring filesystem services.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileSystemService(
        this IServiceCollection services)
    {
        services.AddSingleton<RootPathResolver>();
        services.AddSingleton<RootProvider>();
        services.AddSingleton<IFileSystemService, FileSystemService>();

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

    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddSingleton<ExecutionOrderUtil>();
        return services;
    }

    public static IServiceCollection AddMcpResources(this IServiceCollection services)
    {
        services.AddSingleton<FileSystemResource>();
        return services;
    }
}
