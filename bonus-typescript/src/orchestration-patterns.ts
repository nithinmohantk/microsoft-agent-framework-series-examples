/**
 * Multi-Agent Orchestration Patterns - TypeScript
 * Microsoft Agent Framework Series - Part 7 Equivalent
 */

import OpenAI from 'openai';

type Agent = {
    name: string;
    instructions: string;
    run: (input: string) => Promise<string>;
};

// Create an agent
function createAgent(client: OpenAI, name: string, instructions: string): Agent {
    return {
        name,
        instructions,
        run: async (input: string) => {
            const response = await client.chat.completions.create({
                model: 'gpt-4o',
                messages: [
                    { role: 'system', content: instructions },
                    { role: 'user', content: input }
                ]
            });
            return response.choices[0].message.content || '';
        }
    };
}

// Sequential Orchestrator
class SequentialOrchestrator {
    private agents: Agent[];

    constructor(agents: Agent[]) {
        this.agents = agents;
    }

    async run(input: string): Promise<string> {
        let result = input;

        for (const agent of this.agents) {
            console.log(`  → Running ${agent.name}...`);
            result = await agent.run(result);
        }

        return result;
    }
}

// Concurrent Orchestrator
class ConcurrentOrchestrator {
    private agents: Agent[];
    private aggregator: (results: string[]) => string;

    constructor(agents: Agent[], aggregator: (results: string[]) => string) {
        this.agents = agents;
        this.aggregator = aggregator;
    }

    async run(input: string): Promise<string> {
        console.log(`  → Running ${this.agents.length} agents in parallel...`);

        const promises = this.agents.map(agent => agent.run(input));
        const results = await Promise.all(promises);

        return this.aggregator(results);
    }
}

// Handoff Orchestrator
class HandoffOrchestrator {
    private initialAgent: Agent;
    private agents: Map<string, Agent>;
    private handoffPattern: RegExp;

    constructor(
        initialAgent: Agent,
        agents: Record<string, Agent>,
        handoffPattern: RegExp = /HANDOFF:(\w+)/
    ) {
        this.initialAgent = initialAgent;
        this.agents = new Map(Object.entries(agents));
        this.handoffPattern = handoffPattern;
    }

    async run(input: string): Promise<string> {
        let currentAgent = this.initialAgent;
        let result = await currentAgent.run(input);

        while (true) {
            const match = result.match(this.handoffPattern);
            if (!match) break;

            const nextAgentName = match[1];
            const nextAgent = this.agents.get(nextAgentName);

            if (!nextAgent) {
                console.log(`  ⚠️ Unknown agent: ${nextAgentName}`);
                break;
            }

            console.log(`  → Handoff to ${nextAgentName}`);
            currentAgent = nextAgent;
            result = await currentAgent.run(input);
        }

        return result;
    }
}

// Demo
async function main() {
    const client = new OpenAI({
        apiKey: process.env.AZURE_OPENAI_API_KEY,
        baseURL: `${process.env.AZURE_OPENAI_ENDPOINT}/openai/deployments/gpt-4o`,
        defaultQuery: { 'api-version': '2024-08-01-preview' }
    });

    // Sequential Demo
    console.log('\n=== Sequential Orchestration ===');
    const researcher = createAgent(client, 'Researcher', 'Research the topic and provide key facts.');
    const writer = createAgent(client, 'Writer', 'Write an article from the research.');
    const editor = createAgent(client, 'Editor', 'Edit and polish the article.');

    const sequential = new SequentialOrchestrator([researcher, writer, editor]);
    const seqResult = await sequential.run('AI agents in enterprise software');
    console.log(`Result: ${seqResult.substring(0, 200)}...`);

    // Concurrent Demo
    console.log('\n=== Concurrent Orchestration ===');
    const market = createAgent(client, 'MarketAnalyst', 'Analyze market trends.');
    const tech = createAgent(client, 'TechAnalyst', 'Analyze technology aspects.');
    const finance = createAgent(client, 'FinanceAnalyst', 'Analyze financial health.');

    const concurrent = new ConcurrentOrchestrator(
        [market, tech, finance],
        (results) => results.map((r, i) => `## Section ${i + 1}\n${r}`).join('\n\n')
    );
    const concResult = await concurrent.run('Analyze Microsoft');
    console.log(`Result: ${concResult.substring(0, 200)}...`);

    // Handoff Demo
    console.log('\n=== Handoff Orchestration ===');
    const tier1 = createAgent(client, 'Tier1',
        'Handle basic queries. Say HANDOFF:Tier2 for technical issues.');
    const tier2 = createAgent(client, 'Tier2', 'Handle technical issues.');

    const handoff = new HandoffOrchestrator(tier1, { Tier2: tier2 });
    const handoffResult = await handoff.run('The API returns 500 errors');
    console.log(`Result: ${handoffResult}`);
}

main().catch(console.error);
