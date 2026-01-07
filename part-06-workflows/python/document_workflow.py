"""
Part 6: Document Processing Workflow
"""
import asyncio
from agent_framework import WorkflowBuilder
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential
import os


async def create_document_processing_workflow():
    """Create a workflow for processing documents through multiple agents."""
    
    client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
    )
    
    # Create specialized agents
    classifier_agent = client.create_agent(
        name="DocumentClassifier",
        instructions="""
            You are a document classification expert.
            Analyze documents and classify them into categories:
            - invoice: Bills, payment requests, receipts
            - contract: Legal agreements, terms of service
            - report: Analysis documents, summaries, metrics
            - correspondence: Letters, emails, memos
            - other: Anything that doesn't fit above categories
            
            Return ONLY the category name in lowercase.
        """
    )
    
    extractor_agent = client.create_agent(
        name="DataExtractor",
        instructions="""
            You are a data extraction specialist.
            Based on the document type, extract key fields:
            
            For invoices: vendor, amount, date, invoice_number
            For contracts: parties, effective_date, terms, signatures
            For reports: title, date, key_findings, recommendations
            For correspondence: sender, recipient, date, subject, action_items
            
            Return data as structured JSON.
        """
    )
    
    validator_agent = client.create_agent(
        name="DataValidator",
        instructions="""
            You are a data validation expert.
            Check extracted data for:
            - Completeness: All required fields present
            - Format: Dates, numbers, emails properly formatted
            - Consistency: Values make logical sense
            
            Return validation result with any issues found.
        """
    )
    
    # Build the workflow
    builder = WorkflowBuilder()
    
    # Add executors (agents)
    builder.add_executor(classifier_agent)
    builder.add_executor(extractor_agent)
    builder.add_executor(validator_agent)
    
    # Define edges (execution flow)
    builder.add_edge(classifier_agent, extractor_agent)
    builder.add_edge(extractor_agent, validator_agent)
    
    # Set entry point
    builder.set_start_executor(classifier_agent)
    
    # Build the workflow
    workflow = builder.build()
    
    return workflow


async def process_document(document_text: str):
    """Process a document through the workflow."""
    workflow = await create_document_processing_workflow()
    
    print("Processing document through workflow...")
    print("=" * 50)
    
    # Run the workflow
    result = await workflow.run(document_text)
    
    print(f"\nFinal Result:\n{result}")
    return result


# Example usage
if __name__ == "__main__":
    sample_document = """
    INVOICE #INV-2025-001
    
    From: TechCorp Solutions
    To: Acme Industries
    Date: January 15, 2025
    
    Services Rendered:
    - Cloud Infrastructure Setup: $5,000
    - Security Audit: $2,500
    - Training (3 days): $1,500
    
    Total: $9,000
    Due Date: February 15, 2025
    """
    
    asyncio.run(process_document(sample_document))
