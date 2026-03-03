# Bot Auth Quickstart - Node.js

A Microsoft Teams bot with SSO authentication and Microsoft Graph integration.

## Features

- **SSO Authentication** - Single Sign-On with Microsoft Entra ID
- **Graph Integration** - Fetch user profile, profile photo, and list group chats via Microsoft Graph

## Prerequisites

- [Node.js 18+](https://nodejs.org/en/download/)
- [Visual Studio Code](https://code.visualstudio.com/) with [Microsoft 365 Agents Toolkit](https://aka.ms/teams-toolkit) extension

## Run the sample

### Using Microsoft 365 Agents Toolkit (Recommended)

1. Open the folder in VS Code
2. Select the Microsoft 365 Agents Toolkit icon on the left toolbar
3. Sign in with your Microsoft 365 account
4. Press `F5` to start debugging

### Using Command Line

1. Navigate to this directory:

   ```sh
   cd nodejs/bot-auth-quickstart
   ```

2. Install dependencies and run:

   ```sh
   npm install
   npm start
   ```

   The bot will start listening on `http://localhost:3978`.

## Further Reading

See the [root README](../../README.md) for detailed setup and configuration instructions.