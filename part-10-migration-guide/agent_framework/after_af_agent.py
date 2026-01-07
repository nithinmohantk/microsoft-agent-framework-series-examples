"""
Part 10: Migration to Agent Framework - AFTER
"""
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential
import asyncio
import os

# Create client and agent
agent = AzureOpenAIResponsesClient(
    credential=AzureCliCredential(),  # Use Azure AD instead of API keys
    endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
).create_agent(
    name="MyAgent",
    instructions="You are a helpful assistant that summarizes text."
)


async def af_simple_example():
    """Simple AF invocation."""
    result = await agent.run("Summarize: Microsoft released Agent Framework...")
    print(result.text)


async def af_chat_example():
    """AF with thread (replaces ChatHistory)."""
    thread = agent.get_new_thread()
    
    result1 = await agent.run("Hello!", thread)
    print(result1.text)
    
    # Thread maintains context automatically
    result2 = await agent.run("What did I just say?", thread)
    print(result2.text)
    
    return thread


if __name__ == "__main__":
    asyncio.run(af_simple_example())
