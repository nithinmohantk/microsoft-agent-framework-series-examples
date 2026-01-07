"""
Part 5: Multi-Turn Conversations Demo
"""
import asyncio
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential
import os


async def multi_turn_conversation():
    """Demonstrate multi-turn conversation with thread."""
    
    # Create agent
    agent = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
    ).create_agent(
        name="AssistantBot",
        instructions="You are a helpful assistant. Remember context from the conversation."
    )
    
    # Create a thread for this conversation
    thread = agent.get_new_thread()
    
    print("=== Multi-Turn Conversation Demo ===\n")
    
    # First turn
    print("User: My name is Alice and I'm working on a Python project.")
    result1 = await agent.run("My name is Alice and I'm working on a Python project.", thread)
    print(f"Assistant: {result1.text}\n")
    
    # Second turn - agent remembers the context
    print("User: What language am I using?")
    result2 = await agent.run("What language am I using?", thread)
    print(f"Assistant: {result2.text}\n")  # Will mention Python
    
    # Third turn - agent still has context
    print("User: What's my name again?")
    result3 = await agent.run("What's my name again?", thread)
    print(f"Assistant: {result3.text}\n")  # Will say Alice
    
    print("=== Demo Complete ===")


if __name__ == "__main__":
    asyncio.run(multi_turn_conversation())
