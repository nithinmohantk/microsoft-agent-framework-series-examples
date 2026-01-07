"""
Part 7: Sequential Orchestration Pattern
"""
import asyncio
from agent_framework.orchestration import SequentialOrchestrator
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential
import os


async def create_content_pipeline():
    """Create a sequential content creation pipeline."""
    
    client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
    )
    
    # Create specialized agents
    researcher = client.create_agent(
        name="Researcher",
        instructions="""
            You are a research specialist. Given a topic:
            1. Identify key subtopics to cover
            2. Find relevant facts and statistics
            3. Note important sources and references
            4. Compile a research brief for the writer
        """
    )
    
    writer = client.create_agent(
        name="Writer",
        instructions="""
            You are a professional content writer. Given research:
            1. Create an engaging article structure
            2. Write clear, compelling prose
            3. Include relevant examples
            4. Maintain consistent tone and style
        """
    )
    
    editor = client.create_agent(
        name="Editor",
        instructions="""
            You are an expert editor. Review the draft for:
            1. Grammar and spelling errors
            2. Clarity and readability
            3. Logical flow and structure
            4. Factual accuracy
            Provide the final polished version.
        """
    )
    
    # Create sequential orchestrator
    orchestrator = SequentialOrchestrator(
        agents=[researcher, writer, editor]
    )
    
    return orchestrator


async def main():
    orchestrator = await create_content_pipeline()
    
    print("Starting content pipeline...")
    print("=" * 50)
    
    # Execute pipeline: researcher -> writer -> editor
    result = await orchestrator.run(
        "Create an article about the benefits of AI agents in enterprise software development"
    )
    
    print(f"\nFinal Article:\n{result}")


if __name__ == "__main__":
    asyncio.run(main())
