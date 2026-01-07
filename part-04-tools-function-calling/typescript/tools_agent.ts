import { AzureOpenAI } from "openai";
// ... imports

async function main() {
    // ... setup client
    
    const tools = [
        {
            type: "function",
            function: {
                name: "get_weather",
                description: "Get weather",
                parameters: {
                    type: "object",
                    properties: {
                        location: { type: "string" }
                    },
                    required: ["location"]
                }
            }
        }
    ];

    // Call with tools
    // ...
    console.log("TypeScript supports function calling via standard OpenAI API schema");
}
