---
page_type: sample
description: This sample bot demonstrates implementing SSO in Microsoft Teams using Azure AD with proactive app installation functionality using Microsoft Graph APIs.
products:
- office-teams
- office
- office-365
languages:
- typescript
- nodejs
extensions:
 contentType: samples
 createdDate: "03/02/2026 13:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-bot-auth-quickstart-nodejs
---

# Teams Bot Auth Quickstart - Node.js

This is the **Node.js/TypeScript** implementation of the Teams Bot Auth Quickstart sample.

For complete documentation, setup instructions, and sample overview, see the **[main README](../../README.md)** at the sample root.

## Quick Start

### Prerequisites

- [Node.js](https://nodejs.org/en/download/) version 16.14.2 or higher
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) (recommended)

### Run with Microsoft 365 Agents Toolkit

1. Open this folder in VS Code
2. Install the Microsoft 365 Agents Toolkit extension
3. Sign in with your M365 account
4. Press **F5** to start debugging

### Manual Setup

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Configure environment:**
   
   Update `.localConfigs` or create a `.env` file:
   ```env
   CLIENT_ID=<your-app-id>
   CLIENT_PASSWORD=<your-app-secret>
   BOT_TYPE=SingleTenant
   TENANT_ID=<your-tenant-id>
   CONNECTION_NAME=<oauth-connection-name>
   TEAMS_APP_ID=<teams-app-id>
   ```

3. **Start the bot:**
   ```bash
   npm run dev:teamsfx  # For local development with Teams Toolkit
   # or
   npm start            # For production
   ```

4. **Setup tunnel** (ngrok or dev tunnels) pointing to `http://localhost:3978`

5. **Deploy manifest** - See [main README](../../README.md#setup-teams-app-manifest) for manifest setup

## Project Structure

```
.
├── app.ts              # Main bot application logic
├── config.ts           # Configuration management
├── index.ts            # Entry point
├── appPackage/         # Teams app manifest and icons
├── infra/              # Azure infrastructure (Bicep)
└── m365agents.yml      # Microsoft 365 Agents Toolkit config
```

## Key Features

- **SSO Authentication** - Seamless sign-in using Azure AD
- **Microsoft Graph Integration** - Access user data via Graph API
- **Proactive Installation** - Auto-install app for team/chat members using Graph APIs
- **Dynamic Catalog Discovery** - Automatically discovers app catalog ID
- **Proactive Messaging** - Send messages to members

> **Note**: The `APP_CATALOG_TEAM_APP_ID` is discovered dynamically by the bot, so no manual configuration is needed.

## Bot Commands

- `Login` - Sign in with Microsoft 365
- `Check and Install` or `Install` - Install app for all members
- `Send message` or `Send` - Send a message mentioning all members
- `Logout` - Sign out

## Learn More

- **[Main Sample README](../../README.md)** - Complete setup and documentation
- [Teams Bot Authentication](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots)
- [Microsoft Graph API](https://developer.microsoft.com/graph)
- [Proactive Installation via Graph](https://learn.microsoft.com/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages)