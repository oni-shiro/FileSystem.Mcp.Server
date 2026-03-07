using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FileSystem.Mcp.Server.Extensions;
using Microsoft.Extensions.Configuration;
using FileSystem.Mcp.Server.Configuration;
using FileSystem.Mcp.Server.Tools;
using FileSystem.Mcp.Server.Services;
using FileSystem.Mcp.Server.Resources;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(config => config.LogToStandardErrorThreshold = LogLevel.Information);

builder
.Configuration
.SetBasePath(AppContext.BaseDirectory)
.AddJsonFile("config.json", optional: true, reloadOnChange: true);

builder.Services.Configure<AppConfig>(builder.Configuration);

builder.Services.AddFileSystemService();
builder.Services.AddBatchFileOperationService();
builder.Services.AddUtilities();
builder.Services.AddMcpResources();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResources<FileSystemResource>();

var app = builder.Build();

await app.RunAsync();