# Bot Auth Quickstart

This sample demonstrates how to implement Single Sign-On (SSO) authentication for Microsoft Teams bots using Azure Active Directory. It showcases seamless user authentication, Microsoft Graph integration, proactive app installation, and proactive messaging capabilities.

## Table of Contents

- [Interaction with Bot](#interaction-with-bot)
- [Sample Implementations](#sample-implementations)
- [How to run these samples](#how-to-run-these-samples)
- [Troubleshooting](#troubleshooting)
- [Further Reading](#further-reading)

## Interaction with Bot

![Bot Auth Quickstart](bot-auth-quickstart.gif)

The bot responds to the following commands:

- **Login** - Sign in using your Microsoft 365 account with SSO authentication
- **Check and Install** or **Install** - Proactively installs the app for all team/chat members
- **Send message** or **Send** - Sends a message mentioning all members in the conversation
- **Logout** - Sign out from the bot and clear authentication session

## Sample Implementations

| Language | Platform | Location |
|----------|----------|----------|
| C# | .NET 10 / ASP.NET Core | dotnet/bot-auth-quickstart |
| TypeScript | Node.js | nodejs/bot-auth-quickstart |
| Python | Python | python/bot-auth-quickstart |

## How to run these samples

> **Note**: Authentication samples require Microsoft Teams and cannot be tested using the `agentsplayground` tool. The OAuth flow and SSO authentication features only work within the Teams client environment.

### Setup Instructions

For setup, configuration, and run instructions using Microsoft 365 Agents Toolkit or manual deployment, please refer to the language-specific README in your chosen implementation folder (dotnet/nodejs/python).

## Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable.
- Ensure your .env or appsettings file is setup correctly.
- Verify that "token.botframework.com" is included in `validDomains` in your manifest.json

## Further Reading

- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
- [Teams Bot Authentication](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots)
- [Microsoft Graph API](https://developer.microsoft.com/graph)
- [Proactive Installation using Graph API](https://learn.microsoft.com/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages)
