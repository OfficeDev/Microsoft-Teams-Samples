# Bot Task Modules

This sample demonstrates how to use task modules (dialogs) in Microsoft Teams using a bot built with Teams SDK. The bot showcases a few approaches: opening task modules through Adaptive Cards with submit actions, multistep dialogs, and launching custom HTML/JavaScript webpages as task modules.

## Table of Contents

- [Interaction with Bot](#interaction-with-bot)
  - [1. Adaptive Card Task Module](#1-adaptive-card-task-module)
  - [2. Custom Form Task Module](#2-custom-form-task-module)
  - [3. Multistep Form Task Module](#3-multistep-form-task-module)
- [Sample Implementations](#sample-implementations)
- [How to run these samples](#how-to-run-these-samples)
  - [Run in the Teams Client](#run-in-the-teams-client)
    - [Configure DevTunnels](#configure-devtunnels)
  - [Provision with the Teams Developer CLI](#provision-with-the-teams-developer-cli)
- [Troubleshooting](#troubleshooting)
- [Further Reading](#further-reading)

## Interaction with Bot

![Bot Task Modules](bot-task-modules.gif)

The bot supports the following functionalities:

### 1. Adaptive Card Task Module

- Opens a task module with an Adaptive Card interface
- Demonstrates card-based dialogs with interactive elements
- Handles user input and submit actions within the card
- Shows best practices for Adaptive Card task module integration

### 2. Custom Form Task Module

- Launches a custom HTML webpage as a task module
- Demonstrates custom UI components including form fields
- Collects user information through an interactive form
- Handles form submission and data processing
- Shows Teams-themed styling for consistent user experience

### 3. Multistep Form Task Module

- Opens a two-step Adaptive Card dialog within a single task module session
- **Step 1** collects the user's name and advances to the next step on submit
- **Step 2** carries forward the name from step 1 and collects the user's email address
- On final submission, the bot sends a personalized confirmation message (`Hi {name}, thanks for submitting! Your email is {email}`)
- Demonstrates chaining task module responses using `task/continue` to transition between steps
- Shows how to pass data between steps using the Adaptive Card `Action.Submit` `data` payload

## Sample Implementations

| Language | Framework | Directory |
|----------|-----------|-----------|
| C# | .NET 10 / ASP.NET Core | [dotnet/bot-task-modules](dotnet/bot-task-modules/README.md) |
| Typescript | Node.js | [nodejs/bot-task-modules](nodejs/bot-task-modules/README.md) |
| Python | Python | [python/bot-task-modules](python/bot-task-modules/README.md) |

# How to run these samples

You can run these samples locally in the Teams Client after you have provisioned the Teams app, written its credentials into your project's environment file, and started the bot against a public DevTunnels URL.

## Run in the Teams Client

To run these samples in the Teams Client, you need to provision your app in an M365 tenant and configure the app to your DevTunnels URL.

1. Install the [DevTunnels CLI](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started)
2. Get access to an [M365 Developer Tenant](https://learn.microsoft.com/en-us/office/developer-program/microsoft-365-developer-program-get-started)
3. Install the [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/installation): `npm install -g @microsoft/teams.cli`

### Configure DevTunnels

Create a persistent tunnel for port 3978 with anonymous access:

```bash
devtunnel create -a my-tunnel
devtunnel port create -p 3978 my-tunnel
devtunnel host my-tunnel
```

Take note of the URL shown after *Connect via browser:*

## Provision with the Teams Developer CLI

The [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/) provisions your Microsoft Entra app, Teams-managed bot registration, Teams app manifest, and writes the credentials directly into your project's environment file in a single command.

Sign in with your M365 account:

```bash
teams login
```

From the language-specific sample directory you want to run, provision the app and credentials.

For Node.js and Python (`nodejs/bot-task-modules` or `python/bot-task-modules`):

```bash
teams app create --name "Bot Task Modules" --endpoint https://<your-devtunnel-domain>/api/messages --env .env
```

For .NET (`dotnet/bot-task-modules`):

```bash
teams app create --name "Bot Task Modules" --endpoint https://<your-devtunnel-domain>/api/messages --env appsettings.json
```

This single command creates a Microsoft Entra app registration, registers a Teams-managed bot pointing at your DevTunnels endpoint, generates and uploads the Teams app manifest, and writes `CLIENT_ID`, `CLIENT_SECRET`, and `TENANT_ID` into the environment file you specified (PascalCase keys under a `Teams` section for `appsettings.json`).

Once provisioning completes, start your bot - the sample will pick up the credentials automatically - and sideload the app from the prompt in Teams. See the [Teams Developer CLI documentation](https://microsoft.github.io/teams-sdk/cli/) for the full command reference.

> **Tip**: Using an AI coding assistant (GitHub Copilot CLI, Claude Code, Cursor, VS Code)? Install the [`teams-dev` agent skill](https://microsoft.github.io/teams-sdk/developer-tools/agent-skills) to drive these CLI steps from natural language - your assistant runs the right commands, manages credentials, and guides you through bot registration end-to-end.

# Teams SDK Troubleshooting

If you encounter errors you believe exists in Teams SDK, you can file an issue in GitHub for the langauge in which you encountered the issue
- [C#](https://github.com/microsoft/teams.net/issues/new/choose)
- [TypeScript](https://github.com/microsoft/teams.ts/issues/new/choose)
- [Python](https://github.com/microsoft/teams.py/issues/new/choose)

You can file general issues that exists across all SDK langauges [here](https://github.com/microsoft/teams-sdk/issues/new/choose)

## General Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable.
- Ensure your `.env` or `appsettings.json` file is set up correctly.
- Verify that the task module URL (for custom form) is properly configured and accessible.
- For custom form issues, ensure your bot endpoint is serving static files correctly.
- Use the Channels UI in Azure Bot Service in the Azure Portal to see detailed endpoint errors.

## Further Reading

- [Task Modules in Microsoft Teams](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/what-are-task-modules)
- [Invoke and dismiss task modules](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/task-modules/invoking-task-modules)
- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
- [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/)
- [`teams-dev` Agent Skill](https://microsoft.github.io/teams-sdk/developer-tools/agent-skills) - AI coding assistant skill that drives the Teams Developer CLI via natural language