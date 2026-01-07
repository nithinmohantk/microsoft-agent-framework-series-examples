import { AzureOpenAI } from "openai";
import { DefaultAzureCredential } from "@azure/identity";
import { getBearerTokenProvider } from "@azure/identity";

// Typescript implementation of Part 2/3: First Agent
async function main() {
    const scope = "https://cognitiveservices.azure.com/.default";
    const azureADProvider = getBearerTokenProvider(new DefaultAzureCredential(), scope);

    const client = new AzureOpenAI({
        azureADTokenProvider: azureADProvider,
        deployment: "gpt-4o",
        apiVersion: "2024-05-01-preview"
    });

    const messages = [
        { role: "system", content: "You are a helpful assistant." },
        { role: "user", content: "Hello from TypeScript!" }
    ];

    const result = await client.chat.completions.create({
        messages: messages as any,
        model: "gpt-4o",
    });

    console.log(result.choices[0].message.content);
}

main().catch(console.error);
