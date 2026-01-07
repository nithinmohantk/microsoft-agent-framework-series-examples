using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Azure.Identity;
using OpenAI;

namespace MAF.Part06.Workflows;

/// <summary>
/// Part 6: Document Processing Workflow in .NET (Updated with Conditional Logic)
/// </summary>
public class DocumentWorkflow
{
    public static async Task Main(string[] args)
    {
        var client = new AzureOpenAIClient(
            new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
            new DefaultAzureCredential());

        var responseClient = client.GetOpenAIResponseClient("gpt-4o");

        // Create specialized agents
        var classifier = responseClient.CreateAIAgent("Classifier", "Classify: invoice, contract, report");
        var extractor = responseClient.CreateAIAgent("Extractor", "Extract fields");
        var validator = responseClient.CreateAIAgent("Validator", "Validate data");
        
        // Handling special cases
        var specialHandler = responseClient.CreateAIAgent("SpecialHandler", "Handle high priority docs");

        // Build workflow
        var builder = new WorkflowBuilder();

        builder.AddExecutor(classifier);
        builder.AddExecutor(extractor);
        builder.AddExecutor(validator);
        builder.AddExecutor(specialHandler);

        // Standard Flow
        builder.AddEdge(extractor, validator);

        // Conditional Flow from Classifier
        builder.AddConditionalEdge(
            source: classifier,
            destination: extractor,
            condition: result => 
                result.ToString().Contains("invoice") || 
                result.ToString().Contains("contract"));

        builder.AddConditionalEdge(
            source: classifier,
            destination: specialHandler,
            condition: result => result.ToString().Contains("urgent"));

        builder.SetStartExecutor(classifier);

        var workflow = builder.Build();

        // Run workflow
        Console.WriteLine("Processing document...");
        var result = await workflow.RunAsync("INVOICE #123 (URGENT)");
        Console.WriteLine($"Result: {result}");
    }
}
