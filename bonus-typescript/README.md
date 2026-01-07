# Bonus: TypeScript Examples

TypeScript/Node.js implementations of the Microsoft Agent Framework patterns.

## Setup

```bash
npm install
npm run build
```

## Files

| File | Description |
|------|-------------|
| `src/research-assistant.ts` | Research assistant with tools |
| `src/multi-turn-redis.ts` | Multi-turn with Redis persistence |
| `src/orchestration-patterns.ts` | Sequential, Concurrent, Handoff patterns |

## Running

```bash
# Set environment variables
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o"

# Run examples
npx ts-node src/research-assistant.ts
npx ts-node src/multi-turn-redis.ts
npx ts-node src/orchestration-patterns.ts
```

## Note

These are conceptual implementations showing how to build similar patterns in TypeScript. The official Microsoft Agent Framework SDK is available for Python and .NET.
