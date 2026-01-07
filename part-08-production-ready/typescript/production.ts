
// Part 8: Production patterns using OpenTelemetry JS
import { NodeSDK } from '@opentelemetry/sdk-node';
import { ConsoleSpanExporter } from '@opentelemetry/sdk-trace-node';

const sdk = new NodeSDK({
  traceExporter: new ConsoleSpanExporter(),
  serviceName: 'my-ts-agent-service',
});

sdk.start();
