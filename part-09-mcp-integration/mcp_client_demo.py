"""
Part 9: MCP Client Demo
"""
import asyncio
import os
from dataclasses import dataclass
from typing import List

from agent_framework.mcp import MCPClient, MCPServerConfig
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential


@dataclass
class MCPTool:
    name: str
    description: str
    parameters: dict


async def connect_to_mcp_server():
    """Connect to an MCP server and discover available tools."""
    
    # Configure MCP server connection
    mcp_config = MCPServerConfig(
        url="http://localhost:8080",
        auth_token=os.getenv("MCP_TOKEN"),
        timeout=30,
        retry_attempts=3
    )
    
    # Create MCP client
    mcp_client = MCPClient(config=mcp_config)
    
    # Connect and discover capabilities
    await mcp_client.connect()
    
    # List available tools
    tools: List[MCPTool] = await mcp_client.list_tools()
    print(f"Discovered {len(tools)} tools from MCP server:")
    for tool in tools:
        print(f"  - {tool.name}: {tool.description}")
    
    # List available resources
    resources = await mcp_client.list_resources()
    print(f"\nDiscovered {len(resources)} resources:")
    for resource in resources:
        print(f"  - {resource.uri}: {resource.description}")
    
    # List available prompts
    prompts = await mcp_client.list_prompts()
    print(f"\nDiscovered {len(prompts)} prompt templates:")
    for prompt in prompts:
        print(f"  - {prompt.name}: {prompt.description}")
    
    return mcp_client


async def create_mcp_enabled_agent():
    """Create an agent with MCP tools automatically registered."""
    
    # Connect to MCP server
    mcp_client = await connect_to_mcp_server()
    
    # Create agent with MCP tools
    agent = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
    ).create_agent(
        name="MCPEnabledAgent",
        instructions="""
            You are a helpful assistant with access to external tools via MCP.
            Use the available tools to help users with their requests.
            Always explain what tool you're using and why.
        """,
        mcp_clients=[mcp_client]
    )
    
    return agent


async def main():
    agent = await create_mcp_enabled_agent()
    
    result = await agent.run(
        "Search for recent issues in the microsoft/agent-framework GitHub repo"
    )
    print(f"\nAgent response:\n{result.text}")


if __name__ == "__main__":
    asyncio.run(main())
