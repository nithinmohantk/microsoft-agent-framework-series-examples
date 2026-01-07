
// Part 6: Workflow Pattern
async function documentWorkflow(doc: string) {
    // Agent 1: Classify
    const classification = await runAgent("Classifier", `Classify this: ${doc}`);
    
    // Agent 2: Extract
    if (classification.includes("invoice")) {
        const data = await runAgent("Extractor", `Extract fields from: ${doc}`);
        return data;
    }
}
