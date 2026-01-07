# Part 2: Building Your First AI Agent (.NET)

Complete C# implementation of a Customer Support agent.

## Project Structure

```
CustomerSupportAgent/
├── Program.cs                 # Main application
├── Tools/
│   └── CustomerSupportTools.cs  # Agent tools
└── CustomerSupportAgent.csproj  # Project file
```

## Running the Example

```bash
# Set environment variable
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com"

# Login to Azure
az login

# Run the agent
cd CustomerSupportAgent
dotnet run
```

## Features

- ✅ Multi-turn conversations with thread
- ✅ 4 custom tools (LookupOrder, LookupCustomer, CreateSupportTicket, GetFAQ)
- ✅ Streaming responses
- ✅ Azure DefaultAzureCredential authentication
- ✅ Dependency injection and logging

## Article Link

📖 [Read the full article →](https://www.dataa.dev/2025/10/08/building-your-first-ai-agent-with-microsoft-agent-framework-net-part-2/)
