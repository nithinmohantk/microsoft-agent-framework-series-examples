using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Azure.Identity;
using OpenAI;

namespace MAF.Part06.Workflows;

/// <summary>
/// Part 6: Approval Workflow with Conditional Routing in .NET
/// </summary>
public class ApprovalWorkflow
{
    public static async Task Main(string[] args)
    {
        var client = new AzureOpenAIClient(
            new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
            new DefaultAzureCredential());

        var responseClient = client.GetOpenAIResponseClient("gpt-4o");

        // Create specialized agents
        var requestAnalyzer = responseClient.CreateAIAgent(
            name: "RequestAnalyzer",
            instructions: @"
                Analyze incoming requests and extract:
                - Request type (expense, leave, purchase, access)
                - Amount (if applicable)
                - Urgency level
                - Requester information
                Return structured analysis.");

        var riskAssessor = responseClient.CreateAIAgent(
            name: "RiskAssessor",
            instructions: @"
                Assess risk level based on:
                - Amount thresholds ($1000+ = high risk)
                - Policy compliance
                - Historical patterns
                Return: low, medium, or high risk with explanation.");

        var approvalRouter = responseClient.CreateAIAgent(
            name: "ApprovalRouter",
            instructions: @"
                Based on risk level, determine approval path:
                - Low risk: Auto-approve
                - Medium risk: Manager approval required
                - High risk: VP + Finance approval required
                Return approval decision or escalation path.");

        var notificationSender = responseClient.CreateAIAgent(
            name: "NotificationSender",
            instructions: @"
                Generate appropriate notification for:
                - Requester (status update)
                - Approvers (if escalation needed)
                Return formatted notification.");

        // Build workflow with conditional routing
        var builder = new WorkflowBuilder();

        builder.AddExecutor(requestAnalyzer);
        builder.AddExecutor(riskAssessor);
        builder.AddExecutor(approvalRouter);
        builder.AddExecutor(notificationSender);

        builder.AddEdge(requestAnalyzer, riskAssessor);
        builder.AddConditionalEdge(riskAssessor, approvalRouter,
            condition: result => result.Contains("high") || result.Contains("medium"));
        builder.AddEdge(approvalRouter, notificationSender);

        builder.SetStartExecutor(requestAnalyzer);

        var workflow = builder.Build();

        // Execute
        Console.WriteLine("Processing expense request...");
        var result = await workflow.RunAsync(
            "Process expense report for $5000 - Team dinner for 20 people");

        Console.WriteLine($"\nWorkflow Result:\n{result}");
    }
}
