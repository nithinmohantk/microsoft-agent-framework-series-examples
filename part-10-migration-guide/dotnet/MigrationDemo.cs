using Azure.Identity;
using OpenAI;
using Microsoft.Agents.AI;

namespace MAF.Part10.Migration;

/// <summary>
/// Part 10: Migration from Semantic Kernel - BEFORE (SK Pattern)
/// </summary>
public class BeforeSemanticKernel
{
    /*
    // Semantic Kernel - Agent creation pattern
    using Microsoft.SemanticKernel;
    
    var kernel = new KernelBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-4o",
            endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
            apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
        .Build();
    
    // Using chat history
    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage("Hello!");
    
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    var result = await chatService.GetChatMessageContentsAsync(chatHistory);
    chatHistory.AddAssistantMessage(result[0].Content);
    */
}

/// <summary>
/// Part 10: Migration to Agent Framework - AFTER (MAF Pattern)
/// </summary>
public class AfterAgentFramework
{
    public static async Task Main(string[] args)
    {
        // Agent Framework - Simplified pattern
        var agent = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
                new DefaultAzureCredential())  // Uses Azure AD instead of API keys
            .GetOpenAIResponseClient("gpt-4o")
            .CreateAIAgent(
                name: "MyAgent",
                instructions: "You are a helpful assistant.");

        // Thread replaces ChatHistory
        var thread = agent.GetNewThread();

        var result1 = await agent.RunAsync("Hello!", thread);
        Console.WriteLine(result1);

        // Thread maintains context automatically
        var result2 = await agent.RunAsync("What did I just say?", thread);
        Console.WriteLine(result2);
    }
}
