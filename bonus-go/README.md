# Bonus: Go Examples

Go implementations of the Microsoft Agent Framework patterns.

## Setup

```bash
go mod download
```

## Files

| File | Description |
|------|-------------|
| `research_assistant.go` | Research assistant with tools |
| `orchestration_patterns.go` | Sequential, Concurrent, Handoff patterns |

## Running

```bash
# Set environment variables
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o"

# Run examples
go run research_assistant.go
go run orchestration_patterns.go
```

## Note

These are conceptual implementations showing how to build similar patterns in Go. The official Microsoft Agent Framework SDK is available for Python and .NET.
