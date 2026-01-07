using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Orchestration;
using Azure.Identity;
using OpenAI;

namespace MAF.Part07.Orchestration;

/// <summary>
/// Part 7: Concurrent Orchestration Pattern in .NET
/// </summary>
public class ConcurrentOrchestration
{
    public static async Task Main(string[] args)
    {
        var client = new AzureOpenAIClient(
            new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
            new DefaultAzureCredential());

        var responseClient = client.GetOpenAIResponseClient("gpt-4o");

        // Create parallel analysis agents
        var marketAnalyst = responseClient.CreateAIAgent(
            name: "MarketAnalyst",
            instructions: @"
                Analyze market conditions:
                - Market size and growth trends
                - Competitive landscape
                - Opportunities and threats
                Format as structured market analysis.");

        var techAnalyst = responseClient.CreateAIAgent(
            name: "TechAnalyst",
            instructions: @"
                Analyze technical aspects:
                - Technology stack
                - Innovation capabilities
                - Technical risks
                Format as structured technical analysis.");

        var financialAnalyst = responseClient.CreateAIAgent(
            name: "FinancialAnalyst",
            instructions: @"
                Analyze financial health:
                - Revenue and growth
                - Profitability
                - Financial risks
                Format as structured financial analysis.");

        // Aggregator function
        static string AggregateResults(string[] results)
        {
            var combined = "# Comprehensive Analysis Report\n\n";
            var sections = new[] { "## Market Analysis", "## Technical Analysis", "## Financial Analysis" };

            for (int i = 0; i < Math.Min(results.Length, sections.Length); i++)
            {
                combined += $"{sections[i]}\n{results[i]}\n\n";
            }

            return combined;
        }

        // Create concurrent orchestrator
        var orchestrator = new ConcurrentOrchestrator(
            agents: new[] { marketAnalyst, techAnalyst, financialAnalyst },
            aggregator: AggregateResults);

        Console.WriteLine("Starting parallel analysis (3 agents working simultaneously)...");
        Console.WriteLine(new string('=', 50));

        // All agents work in parallel
        var result = await orchestrator.RunAsync(
            "Analyze Microsoft for potential investment opportunity");

        Console.WriteLine($"\nCombined Report:\n{result}");
    }
}
