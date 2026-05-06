# Targeted Messages in Microsoft Teams - TypeScript

This sample demonstrates how to use **targeted messaging** in Microsoft Teams. Targeted messages are private messages that appear in a shared channel or group chat but are **only visible to a specific user**. The sample implements a reminder bot where all bot responses — confirmations, reminder deliveries, active reminder lists, and snooze confirmations — are sent as targeted messages.

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS version recommended)

## Run the sample

1. Navigate to this directory:
   ```
   cd targeted-messages/bot-targeted-messages/nodejs
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Run the bot:
   ```
   npm start
   ```

The bot will start listening on `http://localhost:3978`.

Refer to the main [README.md](../../README.md) to interact with your bot in the agentsplayground or in Teams.
