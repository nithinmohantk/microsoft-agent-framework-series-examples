// Research Assistant Agent - Go Implementation
// Microsoft Agent Framework Series - Part 3 Equivalent

package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"strings"
	"time"

	"github.com/joho/godotenv"
	openai "github.com/sashabaranov/go-openai"
)

// Tool definitions
type Tool struct {
	Name        string
	Description string
	Execute     func(args map[string]interface{}) string
}

var tools = map[string]Tool{
	"search_web": {
		Name:        "search_web",
		Description: "Search the web for information",
		Execute: func(args map[string]interface{}) string {
			query := args["query"].(string)
			numResults := 5
			if n, ok := args["num_results"].(float64); ok {
				numResults = int(n)
			}
			log.Printf("Searching web for: %s", query)
			return fmt.Sprintf(`Top %d results for '%s':
1. [Enterprise AI Guide] - Microsoft Research
2. [Agentic AI Patterns] - Harvard Business Review
3. [Building AI Agents] - Azure AI Blog`, numResults, query)
		},
	},
	"summarize_text": {
		Name:        "summarize_text",
		Description: "Create a concise summary",
		Execute: func(args map[string]interface{}) string {
			content := args["content"].(string)
			maxWords := 100
			if n, ok := args["max_words"].(float64); ok {
				maxWords = int(n)
			}
			words := strings.Fields(content)
			if len(words) <= maxWords {
				return content
			}
			return strings.Join(words[:maxWords], " ") + "..."
		},
	},
	"get_date": {
		Name:        "get_date",
		Description: "Get the current date",
		Execute: func(args map[string]interface{}) string {
			return time.Now().Format("January 2, 2006")
		},
	},
}

// OpenAI tool definitions
func getOpenAITools() []openai.Tool {
	return []openai.Tool{
		{
			Type: openai.ToolTypeFunction,
			Function: &openai.FunctionDefinition{
				Name:        "search_web",
				Description: "Search the web for information on a topic",
				Parameters: map[string]interface{}{
					"type": "object",
					"properties": map[string]interface{}{
						"query":       map[string]string{"type": "string", "description": "Search query"},
						"num_results": map[string]interface{}{"type": "number", "description": "Number of results"},
					},
					"required": []string{"query"},
				},
			},
		},
		{
			Type: openai.ToolTypeFunction,
			Function: &openai.FunctionDefinition{
				Name:        "summarize_text",
				Description: "Create a concise summary of content",
				Parameters: map[string]interface{}{
					"type": "object",
					"properties": map[string]interface{}{
						"content":   map[string]string{"type": "string", "description": "Content to summarize"},
						"max_words": map[string]interface{}{"type": "number", "description": "Max words"},
					},
					"required": []string{"content"},
				},
			},
		},
		{
			Type: openai.ToolTypeFunction,
			Function: &openai.FunctionDefinition{
				Name:        "get_date",
				Description: "Get the current date",
				Parameters: map[string]interface{}{
					"type":       "object",
					"properties": map[string]interface{}{},
				},
			},
		},
	}
}

// ResearchAssistant agent
type ResearchAssistant struct {
	client   *openai.Client
	messages []openai.ChatCompletionMessage
}

func NewResearchAssistant() *ResearchAssistant {
	config := openai.DefaultAzureConfig(
		os.Getenv("AZURE_OPENAI_API_KEY"),
		os.Getenv("AZURE_OPENAI_ENDPOINT"),
	)
	config.APIVersion = "2024-08-01-preview"

	return &ResearchAssistant{
		client: openai.NewClientWithConfig(config),
		messages: []openai.ChatCompletionMessage{
			{
				Role: openai.ChatMessageRoleSystem,
				Content: `You are an expert research assistant. Your capabilities:
1. Search the web for current information
2. Summarize complex content concisely
3. Give accurate, well-sourced answers
Always cite your sources and use tools to find information.`,
			},
		},
	}
}

func (ra *ResearchAssistant) Run(ctx context.Context, userMessage string) (string, error) {
	ra.messages = append(ra.messages, openai.ChatCompletionMessage{
		Role:    openai.ChatMessageRoleUser,
		Content: userMessage,
	})

	for {
		resp, err := ra.client.CreateChatCompletion(ctx, openai.ChatCompletionRequest{
			Model:    os.Getenv("AZURE_OPENAI_DEPLOYMENT_NAME"),
			Messages: ra.messages,
			Tools:    getOpenAITools(),
		})
		if err != nil {
			return "", err
		}

		assistantMsg := resp.Choices[0].Message

		// Check for tool calls
		if len(assistantMsg.ToolCalls) == 0 {
			ra.messages = append(ra.messages, assistantMsg)
			return assistantMsg.Content, nil
		}

		// Execute tool calls
		ra.messages = append(ra.messages, assistantMsg)

		for _, toolCall := range assistantMsg.ToolCalls {
			var args map[string]interface{}
			json.Unmarshal([]byte(toolCall.Function.Arguments), &args)

			tool := tools[toolCall.Function.Name]
			result := tool.Execute(args)

			ra.messages = append(ra.messages, openai.ChatCompletionMessage{
				Role:       openai.ChatMessageRoleTool,
				Content:    result,
				ToolCallID: toolCall.ID,
			})
		}
	}
}

func main() {
	godotenv.Load()

	fmt.Println(strings.Repeat("=", 60))
	fmt.Println("  🔬 Research Assistant v1.0 (Go)")
	fmt.Println(strings.Repeat("=", 60))

	assistant := NewResearchAssistant()
	ctx := context.Background()

	questions := []string{
		"What are the key trends in AI agents for 2025?",
		"Summarize the main benefits of agentic AI",
	}

	for _, q := range questions {
		fmt.Printf("\n📝 You: %s\n", q)
		response, err := assistant.Run(ctx, q)
		if err != nil {
			log.Printf("Error: %v", err)
			continue
		}
		fmt.Printf("\n🤖 Assistant: %s\n", response)
	}
}
