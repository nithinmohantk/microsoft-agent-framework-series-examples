"""
Part 10: Migration from Semantic Kernel - BEFORE
"""
from semantic_kernel import Kernel
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.contents import ChatHistory
import os

# Create kernel
kernel = Kernel()

# Add AI service
kernel.add_service(AzureChatCompletion(
    deployment_name="gpt-4o",
    endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
    api_key=os.getenv("AZURE_OPENAI_API_KEY"),
    service_id="chat"
))

async def sk_simple_example():
    """Simple SK prompt invocation."""
    result = await kernel.invoke_prompt(
        "Summarize the following text: {{$input}}",
        input="Microsoft released Agent Framework..."
    )
    print(result)

async def sk_chat_example():
    """SK with chat history."""
    chat_history = ChatHistory()
    chat_history.add_user_message("Hello!")
    
    chat_function = kernel.get_function("chat")
    result = await kernel.invoke(chat_function, chat_history=chat_history)
    chat_history.add_assistant_message(str(result))
    
    return chat_history
