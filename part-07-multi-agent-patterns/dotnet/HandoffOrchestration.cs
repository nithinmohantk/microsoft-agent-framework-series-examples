using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Orchestration;
using Azure.Identity;
using OpenAI;
using System.Text.RegularExpressions;

namespace MAF.Part07.Orchestration;

/// <summary>
/// Part 7: Handoff Orchestration Pattern in .NET
/// </summary>
public class HandoffOrchestration
{
    public static async Task Main(string[] args)
    {
        var client = new AzureOpenAIClient(
            new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
            new DefaultAzureCredential());

        var responseClient = client.GetOpenAIResponseClient("gpt-4o");

        // Tiered support agents
        var tier1 = responseClient.CreateAIAgent(
            name: "Tier1Support",
            instructions: @"
                You are a Tier 1 support agent. Handle basic inquiries:
                - Password resets
                - Account information
                - FAQ questions
                
                If the issue is technical, say 'HANDOFF:Tier2Support'
                If about billing, say 'HANDOFF:BillingAgent'
                Otherwise, resolve directly.");

        var tier2 = responseClient.CreateAIAgent(
            name: "Tier2Support",
            instructions: @"
                You are a Tier 2 technical support agent. Handle:
                - Complex technical issues
                - Bug reports
                - Integration problems
                
                If requires engineering, say 'HANDOFF:EngineeringEscalation'
                Otherwise, resolve the issue.");

        var billing = responseClient.CreateAIAgent(
            name: "BillingAgent",
            instructions: @"
                You are a billing specialist. Handle:
                - Invoice questions
                - Payment issues
                - Refund requests
                
                For refunds over $500, say 'HANDOFF:BillingManager'
                Otherwise, resolve the billing issue.");

        var engineering = responseClient.CreateAIAgent(
            name: "EngineeringEscalation",
            instructions: @"
                You are an engineering escalation agent. Handle:
                - Critical bugs
                - System outages
                - Security issues
                Create a detailed ticket for engineering.");

        // Create handoff orchestrator
        var orchestrator = new HandoffOrchestrator(
            initialAgent: tier1,
            agents: new Dictionary<string, object>
            {
                ["Tier2Support"] = tier2,
                ["BillingAgent"] = billing,
                ["EngineeringEscalation"] = engineering
            },
            handoffPattern: new Regex(@"HANDOFF:(\w+)"));

        var testCases = new[]
        {
            "I forgot my password",
            "I have a question about my invoice from last month",
            "The API is returning 500 errors consistently"
        };

        foreach (var query in testCases)
        {
            Console.WriteLine($"\nCustomer: {query}");
            Console.WriteLine(new string('-', 40));
            var result = await orchestrator.RunAsync(query);
            Console.WriteLine($"Resolution: {result}");
        }
    }
}
