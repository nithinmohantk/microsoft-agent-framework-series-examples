"""
Part 9: Custom MCP Server
"""
import asyncio
import json
from typing import Optional

from mcp.server import Server
from mcp.types import Tool, TextContent, Resource

# Initialize MCP server
server = Server("company-internal-tools")


# ============================================================
# TOOL DEFINITIONS
# ============================================================

@server.tool("lookup_employee")
async def lookup_employee(employee_id: str) -> str:
    """
    Look up employee information by ID.
    
    Args:
        employee_id: The employee's unique identifier (e.g., EMP-12345)
    """
    employees = {
        "EMP-001": {"name": "Alice Johnson", "dept": "Engineering", "email": "alice@company.com"},
        "EMP-002": {"name": "Bob Smith", "dept": "Product", "email": "bob@company.com"},
        "EMP-003": {"name": "Carol Williams", "dept": "Sales", "email": "carol@company.com"},
    }
    
    employee = employees.get(employee_id)
    if employee:
        return json.dumps({
            "id": employee_id,
            "name": employee["name"],
            "department": employee["dept"],
            "email": employee["email"],
            "status": "active"
        }, indent=2)
    
    return f"Employee {employee_id} not found"


@server.tool("submit_ticket")
async def submit_ticket(
    title: str,
    description: str,
    priority: str = "medium",
    assignee: Optional[str] = None
) -> str:
    """Submit an internal support ticket."""
    import uuid
    from datetime import datetime
    
    ticket_id = f"TKT-{uuid.uuid4().hex[:8].upper()}"
    
    ticket = {
        "id": ticket_id,
        "title": title,
        "description": description,
        "priority": priority,
        "assignee": assignee,
        "status": "open",
        "created": datetime.now().isoformat()
    }
    
    return json.dumps({
        "success": True,
        "ticket": ticket,
        "message": f"Ticket {ticket_id} created successfully"
    }, indent=2)


@server.tool("query_metrics")
async def query_metrics(metric_name: str, time_range: str = "24h") -> str:
    """Query internal metrics and KPIs."""
    metrics = {
        "sales": {"value": 125000, "unit": "USD", "change": "+12%"},
        "uptime": {"value": 99.98, "unit": "%", "change": "+0.02%"},
        "errors": {"value": 23, "unit": "count", "change": "-15%"},
        "response_time": {"value": 145, "unit": "ms", "change": "-8%"},
    }
    
    metric = metrics.get(metric_name.lower())
    if metric:
        return json.dumps({
            "metric": metric_name,
            "time_range": time_range,
            "current_value": f"{metric['value']} {metric['unit']}",
            "trend": metric["change"],
            "status": "healthy"
        }, indent=2)
    
    return f"Metric '{metric_name}' not found. Available: {list(metrics.keys())}"


# ============================================================
# RESOURCE DEFINITIONS
# ============================================================

@server.resource("company://org-chart")
async def get_org_chart() -> str:
    """Provide the company org chart as a resource."""
    return json.dumps({
        "ceo": "Jane Doe",
        "departments": [
            {"name": "Engineering", "head": "Alice Johnson", "headcount": 45},
            {"name": "Product", "head": "Bob Smith", "headcount": 12},
            {"name": "Sales", "head": "Carol Williams", "headcount": 28},
        ]
    }, indent=2)


# ============================================================
# SERVER STARTUP
# ============================================================

if __name__ == "__main__":
    import sys
    
    print("Starting Company Internal Tools MCP Server...")
    print("Tools: lookup_employee, submit_ticket, query_metrics")
    print("Resources: company://org-chart")
    
    if "--tcp" in sys.argv:
        server.run(transport="tcp", port=8080)
    else:
        server.run(transport="stdio")
