"""
Research Assistant Agent
Built with Microsoft Agent Framework

Author: Nithin Mohan T K
Part 3 of the MAF Series
"""

import asyncio
import os
import logging
from typing import Annotated
from datetime import datetime

from dotenv import load_dotenv
from pydantic import Field
from azure.identity import AzureCliCredential

from agent_framework import ai_function
from agent_framework.azure import AzureOpenAIResponsesClient

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Load environment variables
load_dotenv()

# ============================================================
# TOOLS
# ============================================================

@ai_function
async def search_web(
    query: Annotated[str, Field(description="The search query")],
    num_results: Annotated[int, Field(description="Number of results")] = 5
) -> str:
    """Search the web for information on a topic."""
    logger.info(f"Searching web for: {query}")
    # Mock implementation - integrate with Bing/Google in production
    return f"""
    Top {num_results} results for '{query}':
    1. [Enterprise AI Guide] - Microsoft Research
    2. [Agentic AI Patterns] - Harvard Business Review
    3. [Building AI Agents] - Azure AI Blog
    4. [AI Agent Architecture] - MIT Technology Review
    5. [Production AI Systems] - Google AI Blog
    """

@ai_function
def summarize_text(
    content: Annotated[str, Field(description="Content to summarize")],
    max_words: Annotated[int, Field(description="Max words")] = 100
) -> str:
    """Create a concise summary of the provided content."""
    logger.info(f"Summarizing content ({len(content)} chars)")
    words = content.split()
    if len(words) <= max_words:
        return content
    return ' '.join(words[:max_words]) + '...'

@ai_function
def format_citation(
    title: Annotated[str, Field(description="Source title")],
    url: Annotated[str, Field(description="Source URL")],
    author: Annotated[str, Field(description="Author")] = "Unknown",
    year: Annotated[int, Field(description="Year")] = 2025
) -> str:
    """Format a citation in APA style."""
    return f"{author} ({year}). {title}. Retrieved from {url}"

@ai_function
def get_date() -> str:
    """Get the current date."""
    return datetime.now().strftime("%B %d, %Y")

# ============================================================
# MAIN APPLICATION
# ============================================================

async def main():
    try:
        # Initialize agent
        agent = AzureOpenAIResponsesClient(
            credential=AzureCliCredential(),
            endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        ).create_agent(
            name="ResearchAssistant",
            instructions="""
                You are an expert research assistant. Your capabilities:
                
                1. Search the web for current information
                2. Summarize complex content concisely
                3. Provide properly formatted citations
                4. Give accurate, well-sourced answers
                
                Always:
                - Cite your sources
                - Be objective and accurate
                - Acknowledge uncertainty when appropriate
                - Use tools to find real information
            """,
            tools=[search_web, summarize_text, format_citation, get_date]
        )
        
        # Create conversation thread
        thread = agent.get_new_thread()
        
        print("\n" + "=" * 60)
        print("  🔬 Research Assistant v1.0")
        print("  Powered by Microsoft Agent Framework")
        print("=" * 60)
        print("  Commands: 'exit' to quit, 'clear' for new session\n")
        
        while True:
            try:
                user_input = input("\n📝 You: ").strip()
                
                if not user_input:
                    continue
                
                if user_input.lower() == 'exit':
                    print("\n👋 Thank you for using Research Assistant!")
                    break
                
                if user_input.lower() == 'clear':
                    thread = agent.get_new_thread()
                    print("\n🔄 Session cleared. Starting fresh!")
                    continue
                
                # Stream the response
                print("\n🤖 Assistant: ", end="", flush=True)
                
                start_time = datetime.now()
                response_text = ""
                
                async for update in agent.run_stream(user_input, thread):
                    if update.text:
                        print(update.text, end="", flush=True)
                        response_text += update.text
                
                elapsed = (datetime.now() - start_time).total_seconds()
                print(f"\n\n⏱️  [Response time: {elapsed:.2f}s]")
                
            except KeyboardInterrupt:
                print("\n\n⚠️  Interrupted. Type 'exit' to quit.")
                
    except Exception as e:
        logger.error(f"Application error: {e}", exc_info=True)
        raise

if __name__ == "__main__":
    asyncio.run(main())
