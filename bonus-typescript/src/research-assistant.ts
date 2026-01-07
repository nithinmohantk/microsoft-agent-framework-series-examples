/**
 * Research Assistant Agent - TypeScript Implementation
 * Microsoft Agent Framework Series - Part 3 Equivalent
 */

import OpenAI from 'openai';
import { DefaultAzureCredential, getBearerTokenProvider } from '@azure/identity';
import * as dotenv from 'dotenv';

dotenv.config();

// Tool definitions
const tools: OpenAI.ChatCompletionTool[] = [
    {
        type: 'function',
        function: {
            name: 'search_web',
            description: 'Search the web for information on a topic',
            parameters: {
                type: 'object',
                properties: {
                    query: { type: 'string', description: 'The search query' },
                    num_results: { type: 'number', description: 'Number of results', default: 5 }
                },
                required: ['query']
            }
        }
    },
    {
        type: 'function',
        function: {
            name: 'summarize_text',
            description: 'Create a concise summary of the provided content',
            parameters: {
                type: 'object',
                properties: {
                    content: { type: 'string', description: 'Content to summarize' },
                    max_words: { type: 'number', description: 'Max words', default: 100 }
                },
                required: ['content']
            }
        }
    },
    {
        type: 'function',
        function: {
            name: 'get_date',
            description: 'Get the current date',
            parameters: { type: 'object', properties: {} }
        }
    }
];

// Tool implementations
function searchWeb(query: string, numResults: number = 5): string {
    console.log(`Searching web for: ${query}`);
    return `Top ${numResults} results for '${query}':
    1. [Enterprise AI Guide] - Microsoft Research
    2. [Agentic AI Patterns] - Harvard Business Review
    3. [Building AI Agents] - Azure AI Blog`;
}

function summarizeText(content: string, maxWords: number = 100): string {
    const words = content.split(' ');
    if (words.length <= maxWords) return content;
    return words.slice(0, maxWords).join(' ') + '...';
}

function getDate(): string {
    return new Date().toLocaleDateString('en-US', {
        year: 'numeric', month: 'long', day: 'numeric'
    });
}

// Execute tool calls
function executeTool(name: string, args: Record<string, unknown>): string {
    switch (name) {
        case 'search_web':
            return searchWeb(args.query as string, args.num_results as number);
        case 'summarize_text':
            return summarizeText(args.content as string, args.max_words as number);
        case 'get_date':
            return getDate();
        default:
            return `Unknown tool: ${name}`;
    }
}

// Agent class
class ResearchAssistant {
    private client: OpenAI;
    private messages: OpenAI.ChatCompletionMessageParam[] = [];
    private systemPrompt: string;

    constructor() {
        const credential = new DefaultAzureCredential();
        const scope = 'https://cognitiveservices.azure.com/.default';
        const azureADTokenProvider = getBearerTokenProvider(credential, scope);

        this.client = new OpenAI({
            apiKey: '', // Not needed with Azure AD
            baseURL: `${process.env.AZURE_OPENAI_ENDPOINT}/openai/deployments/${process.env.AZURE_OPENAI_DEPLOYMENT_NAME}`,
            defaultQuery: { 'api-version': '2024-08-01-preview' },
            defaultHeaders: { 'api-key': '' },
        });

        this.systemPrompt = `You are an expert research assistant. Your capabilities:
      1. Search the web for current information
      2. Summarize complex content concisely
      3. Give accurate, well-sourced answers
      Always cite your sources and use tools to find information.`;

        this.messages.push({ role: 'system', content: this.systemPrompt });
    }

    async run(userMessage: string): Promise<string> {
        this.messages.push({ role: 'user', content: userMessage });

        let response = await this.client.chat.completions.create({
            model: process.env.AZURE_OPENAI_DEPLOYMENT_NAME!,
            messages: this.messages,
            tools: tools,
            tool_choice: 'auto'
        });

        let assistantMessage = response.choices[0].message;

        // Handle tool calls
        while (assistantMessage.tool_calls && assistantMessage.tool_calls.length > 0) {
            this.messages.push(assistantMessage);

            for (const toolCall of assistantMessage.tool_calls) {
                const args = JSON.parse(toolCall.function.arguments);
                const result = executeTool(toolCall.function.name, args);

                this.messages.push({
                    role: 'tool',
                    tool_call_id: toolCall.id,
                    content: result
                });
            }

            response = await this.client.chat.completions.create({
                model: process.env.AZURE_OPENAI_DEPLOYMENT_NAME!,
                messages: this.messages,
                tools: tools,
                tool_choice: 'auto'
            });

            assistantMessage = response.choices[0].message;
        }

        const finalContent = assistantMessage.content || '';
        this.messages.push({ role: 'assistant', content: finalContent });

        return finalContent;
    }

    clearHistory(): void {
        this.messages = [{ role: 'system', content: this.systemPrompt }];
    }
}

// Main
async function main() {
    console.log('='.repeat(60));
    console.log('  🔬 Research Assistant v1.0 (TypeScript)');
    console.log('='.repeat(60));

    const assistant = new ResearchAssistant();

    // Example conversation
    const questions = [
        "What are the key trends in AI agents for 2025?",
        "Summarize the main benefits of agentic AI"
    ];

    for (const question of questions) {
        console.log(`\n📝 You: ${question}`);
        const response = await assistant.run(question);
        console.log(`\n🤖 Assistant: ${response}`);
    }
}

main().catch(console.error);
