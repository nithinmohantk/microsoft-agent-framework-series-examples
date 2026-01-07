"""
Part 8: OpenTelemetry Configuration
"""
import os
import logging
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.resources import Resource
from opentelemetry.instrumentation.aiohttp_client import AioHttpClientInstrumentor

logger = logging.getLogger(__name__)


def configure_telemetry(service_name: str = "agent-service"):
    """Configure OpenTelemetry for the agent service."""
    
    # Create resource with service metadata
    resource = Resource.create({
        "service.name": service_name,
        "service.version": "1.0.0",
        "deployment.environment": os.getenv("ENVIRONMENT", "development")
    })
    
    # Create tracer provider
    provider = TracerProvider(resource=resource)
    
    # Configure OTLP exporter (sends to collector)
    otlp_exporter = OTLPSpanExporter(
        endpoint=os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317"),
        insecure=True
    )
    
    # Add batch processor for efficient export
    processor = BatchSpanProcessor(otlp_exporter)
    provider.add_span_processor(processor)
    
    # Set as global tracer provider
    trace.set_tracer_provider(provider)
    
    # Instrument HTTP client
    AioHttpClientInstrumentor().instrument()
    
    logger.info(f"Telemetry configured for {service_name}")
    return trace.get_tracer(service_name)


def traced(operation_name: str):
    """Decorator to trace agent operations."""
    def decorator(func):
        async def wrapper(*args, **kwargs):
            tracer = trace.get_tracer(__name__)
            with tracer.start_as_current_span(operation_name) as span:
                span.set_attribute("agent.operation", operation_name)
                try:
                    result = await func(*args, **kwargs)
                    span.set_attribute("agent.success", True)
                    return result
                except Exception as e:
                    span.set_attribute("agent.success", False)
                    span.set_attribute("agent.error", str(e))
                    span.record_exception(e)
                    raise
        return wrapper
    return decorator


# Usage example
if __name__ == "__main__":
    tracer = configure_telemetry("my-agent-service")
    print("Telemetry configured. Traces will be sent to OTLP collector.")
