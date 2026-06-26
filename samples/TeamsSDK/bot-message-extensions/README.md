# Bot Message Extensions

This sample demonstrates a search-based messaging extension in Microsoft Teams that allows users to search for Wikipedia articles. The extension supports search commands, item selection, and link unfurling.

## Table of Contents

- [Interaction with Bot](#interaction-with-bot)
- [Sample Implementations](#sample-implementations)
- [How to run these samples](#how-to-run-these-samples)
  - [Run in the Teams Client](#run-in-the-teams-client)
  - [Configure the App Manifest](#configure-the-app-manifest)
- [Configure the new project to use the new Teams Bot Application](#configure-the-new-project-to-use-the-new-teams-bot-application)
- [Pro Tip: Read the configuration settings using the Azure CLI](#pro-tip-read-the-configuration-settings-using-the-azure-cli)
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

You can run these samples locally using:

1. In the Teams Client after you have provisioned the Teams Application and configured the application with your local DevTunnels URL.

## Run in the Teams Client

To run these samples in the Teams Client, you need to provision your app in a M365 Tenant, and configure the app to your DevTunnels URL.

1. Install the tool DevTunnels https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started
2. Get Access to a M365 Developer Tenant https://learn.microsoft.com/en-us/office/developer-program/microsoft-365-developer-program-get-started
3. Create a Teams App with the Bot Feature in the Teams Developer Portal (in your tenant) https://dev.teams.microsoft.com

### Configure DevTunnels

Create a persistent tunnel for the port 3978 with anonymous access

```
devtunnel create -a my-tunnel  
devtunnel port create -p 3978  my-tunnel 
devtunnel host  my-tunnel
```

Take note of the URL shown after *Connect via browser:*

### Provisioning the Teams Application

Navigate to the Teams Developer Portal http://dev.teams.microsoft.com

#### Create a new Bot resource

1. Navigate to `Tools->Bot management`, and add a `New bot`
1. In Configure, paste the Endpoint address from devtunnels and append `/api/messages`
1. In Client secrets, create a new secret and save it for later

> Note. If you have access to an Azure Subscription in the same Tenant, you can also create the Azure Bot resource ([learn more](https://learn.microsoft.com/en-us/azure/bot-service/abs-quickstart?view=azure-bot-service-4.0&tabs=singletenant)).

#### Create a new Teams App

1. Navigate to `Apps` and create a `New App`
1. Fill the required values in Basic information (short and long name, short and long description and App URLs)
1. In `App features->Bot` select the bot you created previously
1. Select `Preview in Teams`

> Note. When using an Azure Bot resource, provide the ClientID instead of selecting an existing bot.

### Configure the App Manifest

Ensure the `manifest.json` includes the `composeExtensions` section as shown below:

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

## Configure the new project to use the new Teams Bot Application

For NodeJS and Python you will need a `.env` file with the next fields

```
CLIENT_ID=
CLIENT_SECRET=
TENANT_ID=
```

For dotnet you need to add these values to `appsettings.json` or `launchSettings.json` using the next syntax.

appsettings.json

```json
"urls" : "http://localhost:3978",
"Teams": {
    "ClientID": "",
    "ClientSecret": "",
    "TenantId": ""
 },
```

Or to use Env Vars from the profile defined in `launchSettings.json` (using the Environment Configuration Provider)

```json
 "teamsbot": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:3978",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "Teams__TenantId": "YOUR_TenantId",
        "Teams__ClientID": "YOUR_ClientId",
        "Teams__ClientSecret": "YOUR_ClientSecret"
      }
    }
```

For Python, run the bot using pip:

```bash
pip install -e .
python main.py
```

### Alternative: Using uv (Python)

```bash
uv sync
uv run main.py
```

The bot will start listening on `http://localhost:3978`.

## Pro Tip: Read the configuration settings using the Azure CLI

To obtain the TenantId, ClientId and ClientSecret you can use the Azure CLI with:

> Note. If you don't have access to an Azure Subscription you can still use the Azure CLI, make sure you login with `az login --allow-no-subscription`

```
az ad app credential reset --id $appId
```

## Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable.
- Ensure your .env or appsettings file is setup correctly.
- Use the Channels UI in Azure Bot Service in the Azure Portal to see detailed endpoint errors (not available in Teams Developer Portal).

## Further Reading

- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
- [Message extensions overview](https://learn.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions)
- [Search commands](https://learn.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/search-commands/define-search-command)
- [Link unfurling](https://learn.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling)
