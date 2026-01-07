"""
Part 6: Conditional Routing in Workflows
"""
from agent_framework import WorkflowBuilder


def create_conditional_workflow(client):
    """Create a workflow with conditional routing based on classification."""
    
    # Create type-specific processors
    invoice_processor = client.create_agent(
        name="InvoiceProcessor",
        instructions="Process invoices: validate amounts, check vendor, verify terms."
    )
    
    contract_processor = client.create_agent(
        name="ContractProcessor", 
        instructions="Process contracts: extract terms, identify obligations, flag risks."
    )
    
    general_processor = client.create_agent(
        name="GeneralProcessor",
        instructions="Process general documents: summarize content, extract key points."
    )
    
    classifier = client.create_agent(
        name="Classifier",
        instructions="Classify document as: invoice, contract, or other. Return only the type."
    )
    
    # Router function based on classifier output
    def route_by_type(classification_result: str):
        result_lower = classification_result.lower().strip()
        
        if "invoice" in result_lower:
            return invoice_processor
        elif "contract" in result_lower:
            return contract_processor
        else:
            return general_processor
    
    # Build workflow with conditional routing
    builder = WorkflowBuilder()
    
    builder.add_executor(classifier)
    builder.add_executor(invoice_processor)
    builder.add_executor(contract_processor)
    builder.add_executor(general_processor)
    
    # Add conditional edge - routes based on classification
    builder.add_conditional_edge(
        source=classifier,
        router=route_by_type
    )
    
    builder.set_start_executor(classifier)
    
    return builder.build()


# Usage
async def process_with_routing(document: str, client):
    """Process document with conditional routing."""
    workflow = create_conditional_workflow(client)
    result = await workflow.run(document)
    return result
