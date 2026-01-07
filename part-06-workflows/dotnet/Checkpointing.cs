using Microsoft.Agents.AI.Workflows;

public class CheckpointPattern
{
    public static async Task RunCheckpointWorkflow()
    {
        // Initialize Checkpoint Store (File-based or DB)
        var checkpointStore = new FileCheckpointStore("workflow_checkpoints");

        var classifier = new Agent("Classifier");
        var extractor = new Agent("Extractor");

        // Build Workflow with Checkpointing
        var workflow = new WorkflowBuilder(checkpointStore)
            .AddExecutor(classifier)
            .AddExecutor(extractor)
            .AddEdge(classifier, extractor)
            .Build();

        // Run with a specific Thread/Session ID
        // If the workflow was interrupted, it resumes from the last checkpoint
        var inputDocument = "Some document text...";
        var result = await workflow.RunAsync(inputDocument, sessionId: "doc-process-123");
    }
}
