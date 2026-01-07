using System;
using System.Diagnostics;
using Azure.Identity;
using OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Agents.AI;
using MAF.CustomerSupport.Tools;

namespace MAF.CustomerSupport;

/// <summary>
/// Customer Support Agent - Part 2 of the MAF Series
/// Author: Nithin Mohan T K
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            // Configure Azure OpenAI
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not set");
            
            var client = new AzureOpenAIClient(
                new Uri(endpoint),
                new DefaultAzureCredential());
            
            // Create the agent with tools
            var agent = client
                .GetOpenAIResponseClient("gpt-4o")
                .CreateAIAgent(
                    name: "CustomerSupportBot",
                    instructions: """
                        You are a helpful customer support agent for TechCorp.
                        
                        Guidelines:
                        - Always be polite and professional
                        - Use tools to look up real information
                        - For complex issues, create support tickets
                        - Provide clear, actionable responses
                        
                        Available tools:
                        - LookupOrder: Check order status by order number
                        - LookupCustomer: Find customer by email
                        - CreateSupportTicket: Escalate issues requiring human attention
                        - GetFAQ: Answer common questions about shipping, returns, payments, account
                        """,
                    tools: new object[]
                    {
                        CustomerSupportTools.LookupOrder,
                        CustomerSupportTools.LookupCustomer,
                        CustomerSupportTools.CreateSupportTicket,
                        CustomerSupportTools.GetFAQ
                    });
            
            // Create thread for multi-turn conversation
            var thread = agent.GetNewThread();
            
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("  🤖 TechCorp Customer Support Agent");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("  Type your question or 'exit' to quit.\n");
            
            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                    
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;
                
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    
                    Console.Write("\nAgent: ");
                    await foreach (var update in agent.RunStreamAsync(input, thread))
                    {
                        if (update.Text != null)
                        {
                            Console.Write(update.Text);
                        }
                    }
                    
                    stopwatch.Stop();
                    Console.WriteLine($"\n\n[Response time: {stopwatch.ElapsedMilliseconds}ms]\n");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing request");
                    Console.WriteLine($"\n❌ An error occurred: {ex.Message}\n");
                }
            }
            
            Console.WriteLine("\nThank you for using TechCorp Support. Goodbye!");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application startup failed");
            throw;
        }
    }
}
