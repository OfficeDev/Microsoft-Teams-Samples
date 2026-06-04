# Bot Message Extensions

This sample demonstrates a search-based messaging extension in Microsoft Teams that allows users to search for Wikipedia articles. The extension supports search commands, item selection, and link unfurling.

## Table of Contents

- [Interaction with Bot](#interaction-with-bot)
- [Sample Implementations](#sample-implementations)
- [How to run these samples](#how-to-run-these-samples)
  - [Run in the Teams Client](#run-in-the-teams-client)
    - [Configure DevTunnels](#configure-devtunnels)
  - [Provision with the Teams Developer CLI](#provision-with-the-teams-developer-cli)
  - [Configure the App Manifest](#configure-the-app-manifest)
- [Troubleshooting](#troubleshooting)
- [Further Reading](#further-reading)

## Interaction with Bot

![Bot Message Extensions](bot-message-extensions.gif)

The extension supports the following capabilities:

* **Wikipedia Search** - Search for Wikipedia articles directly from the compose area
* **Link Unfurling** - Generates a rich preview card when a URL is shared in the compose area

## Sample Implementations

| Language | Framework | Directory |
|----------|-----------|-----------|
| C# | .NET 10 / ASP.NET Core | [dotnet/bot-message-extensions](dotnet/bot-message-extensions/README.md) |
| TypeScript | Node.js | [nodejs/bot-message-extensions](nodejs/bot-message-extensions/README.md) |
| Python | Python | [python/bot-message-extensions](python/bot-message-extensions/README.md) |

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

For Node.js and Python (`nodejs/bot-message-extensions` or `python/bot-message-extensions`):

```bash
teams app create --name "Bot Message Extensions" --endpoint https://<your-devtunnel-domain>/api/messages --env .env
```

For .NET (`dotnet/bot-message-extensions`):

```bash
teams app create --name "Bot Message Extensions" --endpoint https://<your-devtunnel-domain>/api/messages --env appsettings.json
```

This single command creates a Microsoft Entra app registration, registers a Teams-managed bot pointing at your DevTunnels endpoint, generates and uploads the Teams app manifest, and writes `CLIENT_ID`, `CLIENT_SECRET`, and `TENANT_ID` into the environment file you specified (PascalCase keys under a `Teams` section for `appsettings.json`).

### Configure the App Manifest

Message extensions require a `composeExtensions` section in `manifest.json`. The Teams Developer CLI generates a baseline manifest, so add the search command and link-unfurling handler to the generated `manifest.json` before re-packaging and re-uploading the app (`teams app package` and `teams app update`):

```json
"composeExtensions": [
  {
    "botId": "${BOT_ID}",
    "canUpdateConfiguration": true,
    "commands": [
      {
        "id": "wikipediaSearch",
        "context": [ "compose", "commandBox" ],
        "description": "Search Wikipedia articles",
        "title": "Wikipedia Search",
        "type": "query",
        "parameters": [
          {
            "name": "searchQuery",
            "title": "Search Query",
            "description": "Your search query",
            "inputType": "text"
          }
        ]
      }
    ],
    "messageHandlers": [
      {
        "type": "link",
        "value": {
          "domains": ["*.wikipedia.org"]
        }
      }
    ]
  }
],
```

Once provisioning and manifest configuration are complete, start your bot and sideload the app from the prompt in Teams. See the [Teams Developer CLI documentation](https://microsoft.github.io/teams-sdk/cli/) for the full command reference.

## Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable.
- Ensure your `.env` or `appsettings.json` file is set up correctly.
- Use the Channels UI in Azure Bot Service in the Azure Portal to see detailed endpoint errors.

## Further Reading

- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
- [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/)
- [Message extensions overview](https://learn.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions)
- [Search commands](https://learn.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/search-commands/define-search-command)
- [Link unfurling](https://learn.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling)