using Azure.Identity;
using OpenAI;
using Microsoft.Agents.AI;

namespace MAF.Part05.MultiTurn;

/// <summary>
/// Part 5: Multi-Turn Conversations in .NET
/// </summary>
public class MultiTurnDemo
{
    public static async Task Main(string[] args)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not set");

        var agent = new AzureOpenAIClient(
                new Uri(endpoint),
                new DefaultAzureCredential())
            .GetOpenAIResponseClient("gpt-4o")
            .CreateAIAgent(
                name: "AssistantBot",
                instructions: "You are a helpful assistant. Remember context from the conversation.");

        // Create a thread for multi-turn conversation
        var thread = agent.GetNewThread();

        Console.WriteLine("=== Multi-Turn Conversation Demo ===\n");

        // First turn
        Console.WriteLine("User: I'm Bob, and I need help with Azure.");
        var result1 = await agent.RunAsync("I'm Bob, and I need help with Azure.", thread);
        Console.WriteLine($"Assistant: {result1}\n");

        // Second turn - agent remembers the context
        Console.WriteLine("User: What cloud platform am I asking about?");
        var result2 = await agent.RunAsync("What cloud platform am I asking about?", thread);
        Console.WriteLine($"Assistant: {result2}\n");  // Will mention Azure

        // Third turn - agent still has context
        Console.WriteLine("User: Remind me of my name?");
        var result3 = await agent.RunAsync("Remind me of my name?", thread);
        Console.WriteLine($"Assistant: {result3}\n");  // Will say Bob

        Console.WriteLine("=== Demo Complete ===");
    }
}
