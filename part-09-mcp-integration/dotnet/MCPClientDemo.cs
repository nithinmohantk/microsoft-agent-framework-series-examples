using Microsoft.Agents.AI.MCP;
using Azure.Identity;
using OpenAI;

namespace MAF.Part09.MCP;

/// <summary>
/// Part 9: MCP Client Demo in .NET
/// </summary>
public class MCPClientDemo
{
    public static async Task Main(string[] args)
    {
        // Configure MCP server connection
        var mcpConfig = new MCPServerConfig
        {
            Url = "http://localhost:8080",
            AuthToken = Environment.GetEnvironmentVariable("MCP_TOKEN"),
            Timeout = TimeSpan.FromSeconds(30),
            RetryAttempts = 3
        };

        // Create MCP client
        var mcpClient = new MCPClient(mcpConfig);

        // Connect and discover capabilities
        await mcpClient.ConnectAsync();

        // List available tools
        var tools = await mcpClient.ListToolsAsync();
        Console.WriteLine($"Discovered {tools.Count} tools from MCP server:");
        foreach (var tool in tools)
        {
            Console.WriteLine($"  - {tool.Name}: {tool.Description}");
        }

        // List available resources
        var resources = await mcpClient.ListResourcesAsync();
        Console.WriteLine($"\nDiscovered {resources.Count} resources:");
        foreach (var resource in resources)
        {
            Console.WriteLine($"  - {resource.Uri}: {resource.Description}");
        }

        // Create agent with MCP tools
        var agent = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
                new DefaultAzureCredential())
            .GetOpenAIResponseClient("gpt-4o")
            .CreateAIAgent(
                name: "MCPEnabledAgent",
                instructions: @"
                    You are a helpful assistant with access to external tools via MCP.
                    Use the available tools to help users with their requests.",
                mcpClients: new[] { mcpClient });

        // The agent can now use any tool from the MCP server
        var result = await agent.RunAsync(
            "Search for recent issues in the microsoft/agent-framework GitHub repo");

        Console.WriteLine($"\nAgent response:\n{result}");
    }
}
