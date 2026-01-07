
// Part 7: Orchestrator
async function sequential(task: string) {
    const res1 = await agent1.run(task);
    const res2 = await agent2.run(res1);
    return res2;
}

async function concurrent(task: string) {
    const [r1, r2] = await Promise.all([
        agent1.run(task),
        agent2.run(task)
    ]);
    return merge(r1, r2);
}
