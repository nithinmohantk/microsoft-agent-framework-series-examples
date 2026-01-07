"""
Part 5: Redis Thread Persistence
"""
import json
import redis
from datetime import datetime
from typing import Optional


class RedisThreadStore:
    """Persist agent threads to Redis for recovery and session management."""
    
    def __init__(self, redis_url: str = "redis://localhost:6379"):
        self.redis = redis.from_url(redis_url)
        self.ttl = 86400 * 7  # 7 days expiration
    
    async def save_thread(self, session_id: str, thread) -> None:
        """Save thread state to Redis."""
        data = {
            "messages": thread.messages,
            "metadata": {
                "created": datetime.now().isoformat(),
                "message_count": len(thread.messages),
                "last_updated": datetime.now().isoformat()
            }
        }
        self.redis.setex(
            f"agent:thread:{session_id}", 
            self.ttl, 
            json.dumps(data)
        )
        print(f"Thread saved: {session_id} ({len(thread.messages)} messages)")
    
    async def load_thread(self, session_id: str, agent) -> Optional[object]:
        """Load thread from Redis, or create new if not found."""
        data = self.redis.get(f"agent:thread:{session_id}")
        
        if data:
            parsed = json.loads(data)
            thread = agent.get_new_thread()
            thread.messages = parsed["messages"]
            print(f"Thread loaded: {session_id} ({len(thread.messages)} messages)")
            return thread
        
        print(f"No existing thread found for {session_id}, creating new")
        return agent.get_new_thread()
    
    async def delete_thread(self, session_id: str) -> bool:
        """Delete a thread from Redis."""
        result = self.redis.delete(f"agent:thread:{session_id}")
        return result > 0
    
    async def list_sessions(self, pattern: str = "agent:thread:*") -> list:
        """List all active session IDs."""
        keys = self.redis.keys(pattern)
        return [k.decode().split(":")[-1] for k in keys]


# Usage example
async def persistent_conversation():
    from agent_framework.azure import AzureOpenAIResponsesClient
    from azure.identity import AzureCliCredential
    
    store = RedisThreadStore()
    
    agent = AzureOpenAIResponsesClient(
        credential=AzureCliCredential()
    ).create_agent(
        name="PersistentBot",
        instructions="You are a helpful assistant."
    )
    
    session_id = "user-12345"
    
    # Load existing thread or create new
    thread = await store.load_thread(session_id, agent)
    
    # Run conversation
    result = await agent.run("Continue where we left off", thread)
    print(f"Assistant: {result.text}")
    
    # Save after each interaction
    await store.save_thread(session_id, thread)


if __name__ == "__main__":
    import asyncio
    asyncio.run(persistent_conversation())
