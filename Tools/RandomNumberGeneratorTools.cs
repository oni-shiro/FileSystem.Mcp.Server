using System.ComponentModel;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Tools;

[McpServerToolType]
public class RandomNumberGeneratorTools
{
    [McpServerTool]
    [Description("Generates a random number between the specified minimum and maximum values.")]
    public int GetRandomNumberMyImpl([Description("The minimum value")] int min, [Description("The maximum value")] int max)
    {
        return new Random().Next(min, max);
    }
}