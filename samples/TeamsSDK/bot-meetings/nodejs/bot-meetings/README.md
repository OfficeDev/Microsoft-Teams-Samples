# Bot Meetings - Node.js (TypeScript)

This sample demonstrates a bot for Microsoft Teams that handles real-time meeting events (start, end, participant join/leave) and retrieves meeting transcripts via the Microsoft Graph API.

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS version recommended)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd nodejs/bot-meetings
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the bot:
   ```bash
   npm start
   ```

The bot will start listening on `http://localhost:3978`.

Once the bot is running, follow the main [README.md](../../README.md) to provision your app and side-load it into Teams using the [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/).
