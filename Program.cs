using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder  = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(config => config.LogToStandardErrorThreshold = LogLevel.Error);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();