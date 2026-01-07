"""
Part 4: Tool Definition Patterns - Python Examples
"""
from typing import Annotated, Literal, Optional, List
from pydantic import Field, BaseModel
from agent_framework import ai_function
import aiohttp

# Pattern 1: Simple function with typed parameters
@ai_function
def get_weather(
    city: Annotated[str, Field(description="City name")],
    units: Annotated[Literal["celsius", "fahrenheit"], Field(description="Temperature unit")] = "celsius"
) -> str:
    """Get current weather for a city."""
    weather_data = {
        "new york": {"temp": 22, "condition": "Sunny"},
        "london": {"temp": 15, "condition": "Cloudy"},
        "tokyo": {"temp": 28, "condition": "Humid"}
    }
    data = weather_data.get(city.lower(), {"temp": 20, "condition": "Unknown"})
    temp = data["temp"] if units == "celsius" else (data["temp"] * 9/5) + 32
    return f"Weather in {city}: {temp}°{'C' if units == 'celsius' else 'F'}, {data['condition']}"


# Pattern 2: Async function for I/O operations
@ai_function
async def fetch_api_data(
    endpoint: Annotated[str, Field(description="API endpoint URL")]
) -> str:
    """Fetch data from an external API."""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.get(endpoint, timeout=10) as response:
                if response.status == 200:
                    return await response.text()
                return f"API returned status {response.status}"
    except aiohttp.ClientError as e:
        return f"API error: {str(e)}"


# Pattern 3: Complex return types with Pydantic
class SearchResult(BaseModel):
    title: str
    url: str
    snippet: str

@ai_function
def search_documents(
    query: Annotated[str, Field(description="Search query")],
    limit: Annotated[int, Field(description="Max results", ge=1, le=20)] = 10
) -> List[dict]:
    """Search internal document repository."""
    results = [
        {"title": f"Document about {query}", "url": f"/docs/{i}", "snippet": f"Content related to {query}..."}
        for i in range(min(limit, 3))
    ]
    return results


# Pattern 4: Optional parameters with defaults
@ai_function
def send_notification(
    message: Annotated[str, Field(description="Message content")],
    recipient: Annotated[str, Field(description="Email or user ID")],
    priority: Annotated[Optional[str], Field(description="Priority: low, normal, high")] = "normal",
    schedule: Annotated[Optional[str], Field(description="ISO datetime to send")] = None
) -> str:
    """Send a notification to a user."""
    scheduled_info = f" scheduled for {schedule}" if schedule else ""
    return f"Notification ({priority}) queued for {recipient}{scheduled_info}"


if __name__ == "__main__":
    # Demo the tools
    print(get_weather("New York", "fahrenheit"))
    print(search_documents("AI agents", 5))
    print(send_notification("Hello!", "user@example.com", "high"))
