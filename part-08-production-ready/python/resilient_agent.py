"""
Part 8: Resilient Agent with Circuit Breaker
"""
import asyncio
from datetime import datetime, timedelta
from dataclasses import dataclass
from typing import Optional, Callable
import logging

logger = logging.getLogger(__name__)


@dataclass
class CircuitBreakerState:
    failures: int = 0
    last_failure: Optional[datetime] = None
    is_open: bool = False


class ResilientAgent:
    """
    Production-ready agent wrapper with:
    - Automatic retry with exponential backoff
    - Circuit breaker for failure protection
    - Timeout handling
    - Fallback responses
    """
    
    def __init__(
        self,
        agent,
        max_retries: int = 3,
        base_delay: float = 1.0,
        max_delay: float = 30.0,
        timeout: float = 60.0,
        circuit_threshold: int = 5,
        circuit_reset_time: int = 60,
        fallback_response: Optional[str] = None
    ):
        self.agent = agent
        self.max_retries = max_retries
        self.base_delay = base_delay
        self.max_delay = max_delay
        self.timeout = timeout
        self.circuit_threshold = circuit_threshold
        self.circuit_reset_time = circuit_reset_time
        self.fallback_response = fallback_response or "I'm experiencing difficulties. Please try again later."
        
        self.circuit = CircuitBreakerState()
    
    def _check_circuit(self) -> bool:
        """Check if circuit breaker allows requests."""
        if not self.circuit.is_open:
            return True
        
        if self.circuit.last_failure:
            elapsed = datetime.now() - self.circuit.last_failure
            if elapsed > timedelta(seconds=self.circuit_reset_time):
                logger.info("Circuit breaker reset - allowing requests")
                self.circuit.is_open = False
                self.circuit.failures = 0
                return True
        
        logger.warning("Circuit breaker is OPEN - rejecting request")
        return False
    
    def _record_failure(self):
        """Record a failure and potentially open the circuit."""
        self.circuit.failures += 1
        self.circuit.last_failure = datetime.now()
        
        if self.circuit.failures >= self.circuit_threshold:
            self.circuit.is_open = True
            logger.error(f"Circuit breaker OPENED after {self.circuit.failures} failures")
    
    def _record_success(self):
        """Record a success and reset failure count."""
        self.circuit.failures = 0
    
    async def run(
        self,
        message: str,
        thread=None,
        on_retry: Optional[Callable] = None
    ) -> str:
        """Run agent with resilience patterns."""
        
        # Check circuit breaker
        if not self._check_circuit():
            return self.fallback_response
        
        last_error = None
        
        for attempt in range(self.max_retries + 1):
            try:
                # Apply timeout
                result = await asyncio.wait_for(
                    self.agent.run(message, thread),
                    timeout=self.timeout
                )
                
                self._record_success()
                return result.text
                
            except asyncio.TimeoutError:
                last_error = "Request timed out"
                logger.warning(f"Attempt {attempt + 1}: Timeout")
                
            except Exception as e:
                last_error = str(e)
                logger.warning(f"Attempt {attempt + 1}: {last_error}")
            
            # Record failure
            self._record_failure()
            
            # Exponential backoff
            if attempt < self.max_retries:
                delay = min(self.base_delay * (2 ** attempt), self.max_delay)
                logger.info(f"Retrying in {delay:.1f}s...")
                
                if on_retry:
                    on_retry(attempt + 1, delay)
                
                await asyncio.sleep(delay)
        
        logger.error(f"All retries exhausted. Last error: {last_error}")
        return self.fallback_response


if __name__ == "__main__":
    print("ResilientAgent module loaded.")
    print("Wrap your agent: resilient_agent = ResilientAgent(base_agent)")
