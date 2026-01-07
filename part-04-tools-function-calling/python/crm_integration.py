"""
Part 4: CRM Integration Tools
"""
import os
import aiohttp
from typing import Annotated
from pydantic import Field
from agent_framework import ai_function


class CRMTools:
    """Tools for CRM integration."""
    
    def __init__(self, api_key: str, base_url: str):
        self.api_key = api_key
        self.base_url = base_url
    
    @ai_function
    async def get_customer(
        self,
        customer_id: Annotated[str, Field(description="Customer ID")]
    ) -> str:
        """Retrieve customer information from CRM."""
        async with aiohttp.ClientSession() as session:
            headers = {"Authorization": f"Bearer {self.api_key}"}
            try:
                async with session.get(
                    f"{self.base_url}/customers/{customer_id}",
                    headers=headers,
                    timeout=10
                ) as response:
                    if response.status == 200:
                        data = await response.json()
                        return f"""Customer Found:
                        - ID: {data.get('id')}
                        - Name: {data.get('name')}
                        - Email: {data.get('email')}
                        - Status: {data.get('status')}"""
                    elif response.status == 404:
                        return f"Customer {customer_id} not found."
                    else:
                        return f"API error: status {response.status}"
            except aiohttp.ClientError as e:
                return f"Connection error: {str(e)}"
    
    @ai_function
    async def create_order(
        self,
        customer_id: Annotated[str, Field(description="Customer ID")],
        product_ids: Annotated[str, Field(description="Comma-separated product IDs")],
        quantity: Annotated[int, Field(description="Quantity")] = 1
    ) -> str:
        """Create a new order in the system."""
        async with aiohttp.ClientSession() as session:
            payload = {
                "customer_id": customer_id,
                "products": product_ids.split(","),
                "quantity": quantity
            }
            try:
                async with session.post(
                    f"{self.base_url}/orders",
                    json=payload,
                    headers={"Authorization": f"Bearer {self.api_key}"},
                    timeout=10
                ) as response:
                    if response.status == 201:
                        data = await response.json()
                        return f"Order created! Order ID: {data.get('order_id')}"
                    else:
                        error = await response.text()
                        return f"Failed to create order: {error}"
            except aiohttp.ClientError as e:
                return f"Connection error: {str(e)}"


# Usage example
if __name__ == "__main__":
    crm_tools = CRMTools(
        api_key=os.getenv("CRM_API_KEY", "demo-key"),
        base_url="https://api.example.com"
    )
    
    # Register with agent
    # agent = client.create_agent(
    #     name="OrderAgent",
    #     instructions="Help customers with orders.",
    #     tools=[crm_tools.get_customer, crm_tools.create_order]
    # )
