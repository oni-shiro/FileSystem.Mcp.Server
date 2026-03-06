using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FileSystem.Mcp.Server.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(config => config.LogToStandardErrorThreshold = LogLevel.Error);

// Add the filesystem service with root directory resolution priority:
// 1. Command-line argument (if provided)
// 2. MCP_ROOT_DIR environment variable (if set)
// 3. Current working directory (default)
string? rootDirectoryArg = args.Length > 0 ? args[0] : null;
builder.Services.AddFileSystemService(rootDirectoryArg);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();