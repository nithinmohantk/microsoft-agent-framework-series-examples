/**
 * Multi-Turn Conversations with Redis Persistence - TypeScript
 * Microsoft Agent Framework Series - Part 5 Equivalent
 */

import { createClient, RedisClientType } from 'redis';
import OpenAI from 'openai';

interface ThreadState {
    messages: OpenAI.ChatCompletionMessageParam[];
    metadata: {
        created: string;
        lastUpdated: string;
        messageCount: number;
    };
}

class RedisThreadStore {
    private client: RedisClientType;
    private ttlSeconds: number = 60 * 60 * 24 * 7; // 7 days

    constructor(redisUrl: string = 'redis://localhost:6379') {
        this.client = createClient({ url: redisUrl });
    }

    async connect(): Promise<void> {
        await this.client.connect();
        console.log('Connected to Redis');
    }

    async saveThread(sessionId: string, messages: OpenAI.ChatCompletionMessageParam[]): Promise<void> {
        const state: ThreadState = {
            messages,
            metadata: {
                created: new Date().toISOString(),
                lastUpdated: new Date().toISOString(),
                messageCount: messages.length
            }
        };

        await this.client.setEx(
            `agent:thread:${sessionId}`,
            this.ttlSeconds,
            JSON.stringify(state)
        );

        console.log(`Thread saved: ${sessionId} (${messages.length} messages)`);
    }

    async loadThread(sessionId: string): Promise<OpenAI.ChatCompletionMessageParam[] | null> {
        const data = await this.client.get(`agent:thread:${sessionId}`);

        if (data) {
            const state: ThreadState = JSON.parse(data);
            console.log(`Thread loaded: ${sessionId} (${state.messages.length} messages)`);
            return state.messages;
        }

        console.log(`No thread found for ${sessionId}`);
        return null;
    }

    async deleteThread(sessionId: string): Promise<boolean> {
        const result = await this.client.del(`agent:thread:${sessionId}`);
        return result > 0;
    }

    async listSessions(): Promise<string[]> {
        const keys = await this.client.keys('agent:thread:*');
        return keys.map(k => k.split(':').pop()!);
    }

    async disconnect(): Promise<void> {
        await this.client.disconnect();
    }
}

// Persistent Agent
class PersistentAgent {
    private client: OpenAI;
    private store: RedisThreadStore;
    private sessionId: string;
    private messages: OpenAI.ChatCompletionMessageParam[] = [];

    constructor(sessionId: string, store: RedisThreadStore) {
        this.client = new OpenAI({
            apiKey: process.env.AZURE_OPENAI_API_KEY,
            baseURL: `${process.env.AZURE_OPENAI_ENDPOINT}/openai/deployments/gpt-4o`,
            defaultQuery: { 'api-version': '2024-08-01-preview' }
        });
        this.store = store;
        this.sessionId = sessionId;
    }

    async initialize(): Promise<void> {
        const savedMessages = await this.store.loadThread(this.sessionId);

        if (savedMessages) {
            this.messages = savedMessages;
        } else {
            this.messages = [{
                role: 'system',
                content: 'You are a helpful assistant. Remember context from the conversation.'
            }];
        }
    }

    async run(userMessage: string): Promise<string> {
        this.messages.push({ role: 'user', content: userMessage });

        const response = await this.client.chat.completions.create({
            model: 'gpt-4o',
            messages: this.messages
        });

        const assistantMessage = response.choices[0].message.content || '';
        this.messages.push({ role: 'assistant', content: assistantMessage });

        // Auto-save after each interaction
        await this.store.saveThread(this.sessionId, this.messages);

        return assistantMessage;
    }
}

// Demo
async function main() {
    const store = new RedisThreadStore();
    await store.connect();

    const agent = new PersistentAgent('user-12345', store);
    await agent.initialize();

    console.log('=== Persistent Conversation Demo ===\n');

    const response1 = await agent.run("My name is Alice and I'm learning TypeScript.");
    console.log(`Assistant: ${response1}\n`);

    const response2 = await agent.run("What language am I learning?");
    console.log(`Assistant: ${response2}\n`);

    await store.disconnect();
}

main().catch(console.error);
