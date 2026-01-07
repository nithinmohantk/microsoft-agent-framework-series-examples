// Multi-Agent Orchestration Patterns - Go Implementation
// Microsoft Agent Framework Series - Part 7 Equivalent

package main

import (
	"context"
	"fmt"
	"os"
	"regexp"
	"strings"
	"sync"

	"github.com/joho/godotenv"
	openai "github.com/sashabaranov/go-openai"
)

// Agent represents an AI agent
type Agent struct {
	Name         string
	Instructions string
	client       *openai.Client
}

func NewAgent(client *openai.Client, name, instructions string) *Agent {
	return &Agent{
		Name:         name,
		Instructions: instructions,
		client:       client,
	}
}

func (a *Agent) Run(ctx context.Context, input string) (string, error) {
	resp, err := a.client.CreateChatCompletion(ctx, openai.ChatCompletionRequest{
		Model: os.Getenv("AZURE_OPENAI_DEPLOYMENT_NAME"),
		Messages: []openai.ChatCompletionMessage{
			{Role: openai.ChatMessageRoleSystem, Content: a.Instructions},
			{Role: openai.ChatMessageRoleUser, Content: input},
		},
	})
	if err != nil {
		return "", err
	}
	return resp.Choices[0].Message.Content, nil
}

// SequentialOrchestrator runs agents in sequence
type SequentialOrchestrator struct {
	Agents []*Agent
}

func (o *SequentialOrchestrator) Run(ctx context.Context, input string) (string, error) {
	result := input
	var err error

	for _, agent := range o.Agents {
		fmt.Printf("  → Running %s...\n", agent.Name)
		result, err = agent.Run(ctx, result)
		if err != nil {
			return "", err
		}
	}

	return result, nil
}

// ConcurrentOrchestrator runs agents in parallel
type ConcurrentOrchestrator struct {
	Agents     []*Agent
	Aggregator func([]string) string
}

func (o *ConcurrentOrchestrator) Run(ctx context.Context, input string) (string, error) {
	fmt.Printf("  → Running %d agents in parallel...\n", len(o.Agents))

	results := make([]string, len(o.Agents))
	var wg sync.WaitGroup
	var mu sync.Mutex
	var firstErr error

	for i, agent := range o.Agents {
		wg.Add(1)
		go func(idx int, a *Agent) {
			defer wg.Done()
			result, err := a.Run(ctx, input)
			mu.Lock()
			if err != nil && firstErr == nil {
				firstErr = err
			}
			results[idx] = result
			mu.Unlock()
		}(i, agent)
	}

	wg.Wait()

	if firstErr != nil {
		return "", firstErr
	}

	return o.Aggregator(results), nil
}

// HandoffOrchestrator routes to different agents based on responses
type HandoffOrchestrator struct {
	InitialAgent   *Agent
	Agents         map[string]*Agent
	HandoffPattern *regexp.Regexp
}

func (o *HandoffOrchestrator) Run(ctx context.Context, input string) (string, error) {
	currentAgent := o.InitialAgent
	result, err := currentAgent.Run(ctx, input)
	if err != nil {
		return "", err
	}

	for {
		match := o.HandoffPattern.FindStringSubmatch(result)
		if match == nil {
			break
		}

		nextAgentName := match[1]
		nextAgent, exists := o.Agents[nextAgentName]
		if !exists {
			fmt.Printf("  ⚠️ Unknown agent: %s\n", nextAgentName)
			break
		}

		fmt.Printf("  → Handoff to %s\n", nextAgentName)
		currentAgent = nextAgent
		result, err = currentAgent.Run(ctx, input)
		if err != nil {
			return "", err
		}
	}

	return result, nil
}

func main() {
	godotenv.Load()

	config := openai.DefaultAzureConfig(
		os.Getenv("AZURE_OPENAI_API_KEY"),
		os.Getenv("AZURE_OPENAI_ENDPOINT"),
	)
	config.APIVersion = "2024-08-01-preview"
	client := openai.NewClientWithConfig(config)
	ctx := context.Background()

	// Sequential Demo
	fmt.Println("\n=== Sequential Orchestration ===")
	sequential := &SequentialOrchestrator{
		Agents: []*Agent{
			NewAgent(client, "Researcher", "Research the topic and provide key facts."),
			NewAgent(client, "Writer", "Write an article from the research."),
			NewAgent(client, "Editor", "Edit and polish the article."),
		},
	}
	seqResult, _ := sequential.Run(ctx, "AI agents in enterprise software")
	fmt.Printf("Result: %s...\n", truncate(seqResult, 200))

	// Concurrent Demo
	fmt.Println("\n=== Concurrent Orchestration ===")
	concurrent := &ConcurrentOrchestrator{
		Agents: []*Agent{
			NewAgent(client, "MarketAnalyst", "Analyze market trends."),
			NewAgent(client, "TechAnalyst", "Analyze technology aspects."),
			NewAgent(client, "FinanceAnalyst", "Analyze financial health."),
		},
		Aggregator: func(results []string) string {
			var combined strings.Builder
			sections := []string{"## Market", "## Technical", "## Financial"}
			for i, r := range results {
				combined.WriteString(fmt.Sprintf("%s\n%s\n\n", sections[i], r))
			}
			return combined.String()
		},
	}
	concResult, _ := concurrent.Run(ctx, "Analyze Microsoft")
	fmt.Printf("Result: %s...\n", truncate(concResult, 200))

	// Handoff Demo
	fmt.Println("\n=== Handoff Orchestration ===")
	handoff := &HandoffOrchestrator{
		InitialAgent: NewAgent(client, "Tier1",
			"Handle basic queries. Say HANDOFF:Tier2 for technical issues."),
		Agents: map[string]*Agent{
			"Tier2": NewAgent(client, "Tier2", "Handle technical issues in detail."),
		},
		HandoffPattern: regexp.MustCompile(`HANDOFF:(\w+)`),
	}
	handoffResult, _ := handoff.Run(ctx, "The API returns 500 errors")
	fmt.Printf("Result: %s\n", handoffResult)
}

func truncate(s string, maxLen int) string {
	if len(s) <= maxLen {
		return s
	}
	return s[:maxLen] + "..."
}
