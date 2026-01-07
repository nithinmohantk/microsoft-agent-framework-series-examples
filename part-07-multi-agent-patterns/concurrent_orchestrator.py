"""
Part 7: Concurrent Orchestration Pattern
"""
import asyncio
from agent_framework.orchestration import ConcurrentOrchestrator
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential
import os


async def create_analysis_team():
    """Create a team of parallel analysis agents."""
    
    client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
    )
    
    # Create parallel analysis agents
    market_analyst = client.create_agent(
        name="MarketAnalyst",
        instructions="""
            Analyze market conditions for the given company/topic:
            - Market size and growth trends
            - Competitive landscape
            - Market opportunities and threats
            Format as a structured market analysis section.
        """
    )
    
    tech_analyst = client.create_agent(
        name="TechAnalyst",
        instructions="""
            Analyze technical aspects:
            - Technology stack and architecture
            - Innovation and R&D capabilities  
            - Technical risks and opportunities
            Format as a structured technical analysis section.
        """
    )
    
    financial_analyst = client.create_agent(
        name="FinancialAnalyst",
        instructions="""
            Analyze financial health:
            - Revenue and growth metrics
            - Profitability and margins
            - Financial risks and opportunities
            Format as a structured financial analysis section.
        """
    )
    
    # Aggregator function to combine results
    def aggregate_analysis(results: list) -> str:
        combined = "# Comprehensive Analysis Report\n\n"
        sections = ["## Market Analysis", "## Technical Analysis", "## Financial Analysis"]
        
        for i, (section, result) in enumerate(zip(sections, results)):
            combined += f"{section}\n{result}\n\n"
        
        combined += "## Summary\nAnalysis compiled from market, technical, and financial perspectives."
        return combined
    
    orchestrator = ConcurrentOrchestrator(
        agents=[market_analyst, tech_analyst, financial_analyst],
        aggregator=aggregate_analysis
    )
    
    return orchestrator


async def main():
    orchestrator = await create_analysis_team()
    
    print("Starting parallel analysis (3 agents working simultaneously)...")
    print("=" * 50)
    
    # All agents work in parallel
    result = await orchestrator.run(
        "Analyze Microsoft for potential investment opportunity"
    )
    
    print(f"\nCombined Report:\n{result}")


if __name__ == "__main__":
    asyncio.run(main())
