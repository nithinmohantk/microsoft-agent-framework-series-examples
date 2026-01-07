# Part 1: Introduction to Microsoft Agent Framework

This folder contains conceptual documentation and diagrams for understanding the Microsoft Agent Framework architecture.

## 🎯 What is Microsoft Agent Framework?

Microsoft Agent Framework (MAF) is an **open-source, model-agnostic framework** for building AI agents that can:
- **Reason** about user requests using LLMs
- **Use tools** to take real-world actions
- **Maintain context** across multi-turn conversations
- **Orchestrate** multiple agents for complex workflows

## 🏗️ Architecture Overview

### System Context (C4 Level 1)

```mermaid
C4Context
    title Microsoft Agent Framework - System Context

    Person(developer, "Developer", "Builds AI-powered applications")
    Person(enduser, "End User", "Interacts with AI agents")
    
    System_Boundary(maf, "Microsoft Agent Framework") {
        System(core, "Agent Framework", "Core runtime for AI agents")
    }
    
    System_Ext(azure_openai, "Azure OpenAI", "GPT-4o, GPT-4 Turbo")
    System_Ext(openai, "OpenAI", "GPT-4o, o1")
    System_Ext(mcp, "MCP Servers", "GitHub, Slack, DB tools")
    System_Ext(enterprise, "Enterprise Systems", "CRM, ERP, M365")
    
    Rel(developer, core, "Builds agents using")
    Rel(enduser, core, "Interacts with")
    Rel(core, azure_openai, "LLM reasoning", "REST API")
    Rel(core, openai, "LLM reasoning", "REST API")
    Rel(core, mcp, "Tool discovery", "MCP Protocol")
    Rel(core, enterprise, "Integrates with", "REST/GraphQL")
```

### Container Diagram (C4 Level 2)

```mermaid
C4Container
    title Microsoft Agent Framework - Container View

    Person(user, "User")
    
    Container_Boundary(maf, "Microsoft Agent Framework") {
        Container(agent_client, "Agent Client", "Python/C#", "Creates and manages agents")
        Container(tool_registry, "Tool Registry", "Python/C#", "Registers @ai_function tools")
        Container(thread_manager, "Thread Manager", "", "Manages conversation context")
        Container(workflow_engine, "Workflow Engine", "", "Graph-based orchestration")
        Container(mcp_client, "MCP Client", "", "Connects to external MCP servers")
    }
    
    System_Ext(llm, "LLM Provider", "Azure OpenAI / OpenAI")
    System_Ext(mcp_server, "MCP Servers", "External tool providers")
    System_Ext(storage, "Storage", "Redis, PostgreSQL")
    
    Rel(user, agent_client, "Sends requests")
    Rel(agent_client, llm, "Reasoning & generation")
    Rel(agent_client, tool_registry, "Executes tools")
    Rel(agent_client, thread_manager, "Manages context")
    Rel(agent_client, workflow_engine, "Orchestrates agents")
    Rel(mcp_client, mcp_server, "Discovers tools")
    Rel(thread_manager, storage, "Persists state")
```

### Component Architecture

```mermaid
flowchart TB
    subgraph Clients["📱 Client Applications"]
        Web["Web App"]
        Mobile["Mobile App"]
        API["REST API"]
        CLI["CLI Tool"]
    end
    
    subgraph Core["🧠 Agent Framework Core"]
        direction TB
        Client["AzureOpenAIResponsesClient"]
        Agent["AI Agent"]
        Tools["Tool Registry"]
        Thread["Thread Manager"]
        Workflow["Workflow Engine"]
    end
    
    subgraph Providers["🤖 LLM Providers"]
        AzureOAI["Azure OpenAI<br/>gpt-4o, gpt-4-turbo"]
        OpenAI["OpenAI<br/>gpt-4o, o1"]
        Other["Other Providers<br/>Claude, Gemini"]
    end
    
    subgraph ToolEcosystem["🔧 Tool Ecosystem"]
        BuiltIn["Built-in Tools<br/>• Code Interpreter<br/>• File Search"]
        Custom["Custom Tools<br/>@ai_function decorated"]
        MCP["MCP Integration<br/>External tool servers"]
    end
    
    subgraph External["🌐 External Services"]
        GitHub["GitHub MCP"]
        Slack["Slack MCP"]
        DB["Database MCP"]
        M365["Microsoft 365"]
        CRM["CRM Systems"]
    end
    
    Clients --> Core
    Core --> Providers
    Core --> ToolEcosystem
    ToolEcosystem --> External
    
    style Core fill:#0066cc,color:#fff
    style Providers fill:#10a37f,color:#fff
    style ToolEcosystem fill:#7c3aed,color:#fff
    style External fill:#6b7280,color:#fff
```

### Agent Execution Flow

```mermaid
sequenceDiagram
    participant User
    participant Agent
    participant LLM as Azure OpenAI
    participant Tools
    participant External as External API
    
    User->>Agent: "Check order status for ORD-123"
    Agent->>LLM: Process request with instructions
    
    Note over LLM: Decides to call tool
    LLM->>Agent: Tool call: lookup_order(ORD-123)
    
    Agent->>Tools: Execute lookup_order
    Tools->>External: GET /orders/ORD-123
    External->>Tools: {status: "shipped", ...}
    Tools->>Agent: Order found: shipped
    
    Agent->>LLM: Tool result + continue
    LLM->>Agent: "Your order ORD-123 has been shipped..."
    
    Agent->>User: Final response with order details
```

## 🔑 Core Concepts

| Concept | Description | Example |
|---------|-------------|---------|
| **Agent** | AI entity that processes requests | `client.create_agent(name, instructions, tools)` |
| **Tools** | Functions the agent can call | `@ai_function` decorated functions |
| **Thread** | Conversation context | `agent.get_new_thread()` |
| **Workflow** | Multi-agent orchestration | `WorkflowBuilder` with edges |
| **MCP** | External tool protocol | Connect to GitHub, Slack, DB servers |

## 🆚 Comparison with Other Frameworks

| Feature | MAF | Semantic Kernel | AutoGen | LangChain |
|---------|-----|-----------------|---------|-----------|
| **Model Agnostic** | ✅ | ✅ | ✅ | ✅ |
| **Native .NET** | ✅ | ✅ | ❌ | ❌ |
| **Native Python** | ✅ | ✅ | ✅ | ✅ |
| **MCP Support** | ✅ | ❌ | ❌ | ❌ |
| **Azure Integration** | ✅ | ✅ | ⚠️ | ⚠️ |
| **Multi-Agent** | ✅ | ⚠️ | ✅ | ⚠️ |
| **Azure AI Foundry** | ✅ | ❌ | ❌ | ❌ |

## 📖 Article Link

📖 [Read the full article →](https://www.dataa.dev/2025/10/01/introduction-to-microsoft-agent-framework-the-open-source-engine-for-agentic-ai-apps-part-1/)

## ➡️ Next Steps

Continue to [Part 2: Building Your First Agent (.NET)](../part-02-dotnet-agent/)
