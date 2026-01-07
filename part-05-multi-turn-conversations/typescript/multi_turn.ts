// Part 5: Multi-Turn with History
async function runConversation() {
    let history = [
        { role: "system", content: "You are a helpful assistant." }
    ];

    // Turn 1
    history.push({ role: "user", content: "Hi, I'm using TS" });
    let resp = await client.chat.completions.create({ messages: history, model: "gpt-4o" });
    let msg = resp.choices[0].message;
    history.push(msg);
    console.log(msg.content);

    // Turn 2
    history.push({ role: "user", content: "What did I just say?" });
    resp = await client.chat.completions.create({ messages: history, model: "gpt-4o" });
    console.log(resp.choices[0].message.content);
}
