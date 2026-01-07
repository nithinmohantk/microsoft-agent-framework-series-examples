using Microsoft.Agents.AI;

public static class SummarizationPattern
{
    public static async Task ManageLongConversation(IAgent agent, IThread thread, int maxMessages = 20)
    {
        // Retrieve messages (hypothetical accessor)
        var messages = await thread.GetMessagesAsync(); 
        
        if (messages.Count > maxMessages)
        {
            Console.WriteLine($"Summarizing history... ({messages.Count} messages)");
            
            // Keep last 10 messages intact
            var recentMessages = messages.TakeLast(10).ToList();
            var olderMessages = messages.Take(messages.Count - 10).ToList();
            
            // Create summarization prompt
            var oldContent = string.Join("\n", olderMessages.Select(m => $"{m.Role}: {m.Content}"));
            var summaryPrompt = $"Summarize this conversation context concisely:\n{oldContent}";
            
            // Generate summary using the agent
            var summaryResult = await agent.RunAsync(summaryPrompt);
            
            // Update Thread: Clear and Replace
            await thread.ClearAsync();
            await thread.AddMessageAsync(new Message(Role.System, $"Previous context summary: {summaryResult}"));
            
            // Re-add recent messages
            foreach(var msg in recentMessages)
            {
                await thread.AddMessageAsync(msg);
            }
        }
    }
}
