"""
Part 7: Handoff Orchestration Pattern
"""
import asyncio
from agent_framework.orchestration import HandoffOrchestrator
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential
import os


async def create_support_system():
    """Create a tiered support system with handoffs."""
    
    client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
    )
    
    # Tiered support agents
    tier1 = client.create_agent(
        name="Tier1Support",
        instructions="""
            You are a Tier 1 support agent. Handle basic inquiries:
            - Password resets
            - Account information
            - FAQ questions
            - Basic troubleshooting
            
            If the issue is technical or complex, say "HANDOFF:Tier2Support"
            If the issue is about billing, say "HANDOFF:BillingAgent"
            Otherwise, resolve the issue directly.
        """
    )
    
    tier2 = client.create_agent(
        name="Tier2Support",
        instructions="""
            You are a Tier 2 technical support agent. Handle:
            - Complex technical issues
            - Bug reports
            - Integration problems
            - Performance issues
            
            If the issue requires engineering, say "HANDOFF:EngineeringEscalation"
            Otherwise, resolve the technical issue.
        """
    )
    
    billing = client.create_agent(
        name="BillingAgent",
        instructions="""
            You are a billing specialist. Handle:
            - Invoice questions
            - Payment issues
            - Refund requests
            - Subscription changes
            
            For refunds over $500, say "HANDOFF:BillingManager"
            Otherwise, resolve the billing issue.
        """
    )
    
    engineering = client.create_agent(
        name="EngineeringEscalation",
        instructions="""
            You are an engineering escalation agent. Handle:
            - Critical bugs
            - System outages
            - Security issues
            Create a detailed ticket for the engineering team.
        """
    )
    
    # Create handoff orchestrator
    orchestrator = HandoffOrchestrator(
        initial_agent=tier1,
        agents={
            "Tier2Support": tier2,
            "BillingAgent": billing,
            "EngineeringEscalation": engineering
        },
        handoff_pattern=r"HANDOFF:(\w+)"  # Regex to detect handoff
    )
    
    return orchestrator


async def main():
    orchestrator = await create_support_system()
    
    test_cases = [
        "I forgot my password",
        "I have a question about my invoice from last month",
        "The API is returning 500 errors consistently"
    ]
    
    for query in test_cases:
        print(f"\nCustomer: {query}")
        print("-" * 40)
        result = await orchestrator.run(query)
        print(f"Resolution: {result}")


if __name__ == "__main__":
    asyncio.run(main())
