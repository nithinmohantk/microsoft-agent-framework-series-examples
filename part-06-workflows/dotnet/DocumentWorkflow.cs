using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Azure.Identity;
using OpenAI;

namespace MAF.Part06.Workflows;

/// <summary>
/// Part 6: Document Processing Workflow in .NET
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
        var classifier = responseClient.CreateAIAgent(
            name: "DocumentClassifier",
            instructions: @"
                Classify documents into categories:
                - invoice: Bills, payment requests
                - contract: Legal agreements
                - report: Analysis documents
                - correspondence: Letters, emails
                Return ONLY the category name.");

        var extractor = responseClient.CreateAIAgent(
            name: "DataExtractor",
            instructions: @"
                Extract key fields based on document type:
                - invoice: vendor, amount, date, invoice_number
                - contract: parties, effective_date, terms
                - report: title, date, key_findings
                Return data as structured JSON.");

        var validator = responseClient.CreateAIAgent(
            name: "DataValidator",
            instructions: @"
                Validate extracted data for:
                - Completeness
                - Format correctness
                - Logical consistency
                Return validation result.");

        // Build workflow
        var builder = new WorkflowBuilder();

        builder.AddExecutor(classifier);
        builder.AddExecutor(extractor);
        builder.AddExecutor(validator);

        builder.AddEdge(classifier, extractor);
        builder.AddEdge(extractor, validator);

        builder.SetStartExecutor(classifier);

        var workflow = builder.Build();

        // Run workflow
        Console.WriteLine("Processing document...");
        var result = await workflow.RunAsync(@"
            INVOICE #INV-2025-001

            From: TechCorp Solutions
            To: Acme Industries
            Date: January 15, 2025

            Services: Cloud Setup $5,000
            Total: $5,000
            Due: February 15, 2025
        ");

        Console.WriteLine($"\nResult:\n{result}");
    }
}
