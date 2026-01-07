using Microsoft.Agents.AI.MCP;
using Azure.Identity;
using OpenAI;

namespace MAF.Part09.MCP;

/// <summary>
/// Part 9: Multi-MCP Server Integration in .NET
/// </summary>
public class MultiMCPServers
{
    public static async Task Main(string[] args)
    {
        // GitHub MCP - for code and issue management
        var githubMcp = new MCPClient(new MCPServerConfig
        {
            Url = "http://localhost:8081",
            AuthToken = Environment.GetEnvironmentVariable("GITHUB_MCP_TOKEN"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        await githubMcp.ConnectAsync();
        var githubTools = await githubMcp.ListToolsAsync();
        Console.WriteLine($"GitHub MCP: {githubTools.Count} tools");

        // Slack MCP - for team communication
        var slackMcp = new MCPClient(new MCPServerConfig
        {
            Url = "http://localhost:8082",
            AuthToken = Environment.GetEnvironmentVariable("SLACK_MCP_TOKEN"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        await slackMcp.ConnectAsync();
        var slackTools = await slackMcp.ListToolsAsync();
        Console.WriteLine($"Slack MCP: {slackTools.Count} tools");

        // Database MCP - for data queries
        var dbMcp = new MCPClient(new MCPServerConfig
        {
            Url = "http://localhost:8083",
            AuthToken = Environment.GetEnvironmentVariable("DB_MCP_TOKEN"),
            Timeout = TimeSpan.FromSeconds(60)
        });
        await dbMcp.ConnectAsync();
        var dbTools = await dbMcp.ListToolsAsync();
        Console.WriteLine($"Database MCP: {dbTools.Count} tools");

        // Create agent with all MCP connections
        var agent = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
                new DefaultAzureCredential())
            .GetOpenAIResponseClient("gpt-4o")
            .CreateAIAgent(
                name: "EnterpriseAgent",
                instructions: @"
                    You are an enterprise assistant with access to multiple systems:
                    - GitHub: Search code, list issues, create PRs
                    - Slack: Send messages, search channels
                    - Database: Query customer data, generate reports
                    
                    Choose the appropriate tool based on the user's request.",
                mcpClients: new[] { githubMcp, slackMcp, dbMcp });

        Console.WriteLine("\nAgent ready with multi-MCP integration!");

        // Complex task using multiple MCP servers
        var result = await agent.RunAsync(@"
            1. Find the latest critical bug reports in GitHub
            2. Query the database to see how many customers are affected
            3. Send a summary to the #engineering Slack channel
        ");

        Console.WriteLine($"\nResult:\n{result}");
    }
}
