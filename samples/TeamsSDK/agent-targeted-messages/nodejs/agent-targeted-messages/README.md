# Agent Targeted Messages in Microsoft Teams - TypeScript

This sample demonstrates how to use **targeted messaging** in Microsoft Teams with an agent. Targeted messages are private messages that appear in a shared channel or group chat but are **only visible to a specific user**. The sample implements a reminder agent where all agent responses — confirmations, reminder deliveries, active reminder lists, and snooze confirmations — are sent as targeted messages. The agent includes commented-out stubs showing where agentic flows (e.g., LLM-based intent parsing, smart scheduling, and summarization) can be integrated.

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS version recommended)

## Run the sample

1. Navigate to this directory:
   ```
   cd agent-targeted-messages/nodejs/agent-targeted-messages
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Run the agent:
   ```
   npm start
   ```

The agent will start listening on `http://localhost:3978`.

Refer to the main [README.md](../../README.md) to interact with your agent in the agentsplayground or in Teams.
