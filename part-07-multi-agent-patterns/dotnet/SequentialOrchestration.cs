using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Orchestration;
using Azure.Identity;
using OpenAI;

namespace MAF.Part07.Orchestration;

/// <summary>
/// Part 7: Sequential Orchestration Pattern in .NET
/// </summary>
public class SequentialOrchestration
{
    public static async Task Main(string[] args)
    {
        var client = new AzureOpenAIClient(
            new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
            new DefaultAzureCredential());

        var responseClient = client.GetOpenAIResponseClient("gpt-4o");

        // Create specialized agents for content pipeline
        var researcher = responseClient.CreateAIAgent(
            name: "Researcher",
            instructions: @"
                You are a research specialist. Given a topic:
                1. Identify key subtopics to cover
                2. Find relevant facts and statistics
                3. Note important sources
                4. Compile a research brief for the writer");

        var writer = responseClient.CreateAIAgent(
            name: "Writer",
            instructions: @"
                You are a professional content writer. Given research:
                1. Create an engaging article structure
                2. Write clear, compelling prose
                3. Include relevant examples
                4. Maintain consistent tone and style");

        var editor = responseClient.CreateAIAgent(
            name: "Editor",
            instructions: @"
                You are an expert editor. Review the draft for:
                1. Grammar and spelling errors
                2. Clarity and readability
                3. Logical flow and structure
                4. Factual accuracy
                Provide the final polished version.");

        // Create sequential orchestrator
        var orchestrator = new SequentialOrchestrator(
            agents: new[] { researcher, writer, editor });

        Console.WriteLine("Starting content pipeline...");
        Console.WriteLine(new string('=', 50));

        // Execute pipeline: researcher -> writer -> editor
        var result = await orchestrator.RunAsync(
            "Create an article about the benefits of AI agents in enterprise software development");

        Console.WriteLine($"\nFinal Article:\n{result}");
    }
}
