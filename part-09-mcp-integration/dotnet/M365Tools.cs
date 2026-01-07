using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Agents.AI.MCP;

namespace MAF.Part09.MCP;

public class Microsoft365MCPTool
{
    private readonly GraphServiceClient _graphClient;

    public Microsoft365MCPTool()
    {
        // Use DefaultAzureCredential (supports CLI, Env Vars, Managed Identity)
        var credential = new DefaultAzureCredential();
        // Connect to Microsoft Graph global endpoint
        _graphClient = new GraphServiceClient(credential, 
            new[] { "https://graph.microsoft.com/.default" });
    }

    [MCPTool("get_calendar_events", Description = "Get upcoming calendar events")]
    public async Task<string> GetCalendarEventsAsync(int count = 5)
    {
        try 
        {
            var result = await _graphClient.Me.Calendar.Events.GetAsync(config =>
            {
                config.QueryParameters.Top = count;
                config.QueryParameters.Select = new[] { "subject", "start", "end" };
                config.QueryParameters.Orderby = new[] { "start/dateTime" };
            });

            if (result?.Value == null || result.Value.Count == 0) 
                return "No upcoming events found.";

            var summary = result.Value.Select(e => 
                $"- {e.Subject}: {e.Start.DateTime} to {e.End.DateTime}");
                
            return string.Join("\n", summary);
        }
        catch (Exception ex)
        {
            return $"Error fetching events: {ex.Message}";
        }
    }
    
    [MCPTool("send_email", Description = "Send an email")]
    public async Task<string> SendEmailAsync(string recipient, string subject, string body)
    {
         var message = new Message
         {
             Subject = subject,
             Body = new ItemBody { Content = body, ContentType = BodyType.Text },
             ToRecipients = new List<Recipient> { new Recipient { EmailAddress = new EmailAddress { Address = recipient } } }
         };
         
         await _graphClient.Me.SendMail.PostAsync(new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody { Message = message });
         return "Email sent successfully.";
    }
}
