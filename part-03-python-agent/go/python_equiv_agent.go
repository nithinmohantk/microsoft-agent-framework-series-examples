package main

import (
	"context"
	"fmt"
	"log"
	"os"

	"github.com/Azure/azure-sdk-for-go/sdk/ai/azopenai"
	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
)

// Go implementation of Part 2/3: First Agent
func main() {
	endpoint := os.Getenv("AZURE_OPENAI_ENDPOINT")
	cred, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		log.Fatal(err)
	}

	client, err := azopenai.NewClient(endpoint, cred, nil)
	if err != nil {
		log.Fatal(err)
	}

	resp, err := client.GetChatCompletions(context.TODO(), azopenai.ChatCompletionsOptions{
		Messages: []azopenai.ChatRequestMessageClassification{
			&azopenai.ChatRequestSystemMessage{Content: azopenai.NewChatRequestMessageContent("You are a helpful assistant.")},
			&azopenai.ChatRequestUserMessage{Content: azopenai.NewChatRequestMessageContent("Hello from Go!")},
		},
		DeploymentName: toPtr("gpt-4o"),
	}, nil)

	if err != nil {
		log.Fatal(err)
	}

	fmt.Println(*resp.Choices[0].Message.Content)
}

func toPtr(s string) *string { return &s }
