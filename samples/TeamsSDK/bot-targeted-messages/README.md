# Bot Targeted Messages

This sample demonstrates how to use **targeted messaging** in Microsoft Teams. Targeted messages are private messages that appear in a shared channel or group chat but are **only visible to a specific user**. The sample implements a reminder bot where all bot responses — confirmations, reminder deliveries, active reminder lists, and snooze confirmations — are sent as targeted messages.

## Table of Contents

- [Included Features](#included-features)
- [Interaction with Bot](#interaction-with-bot)
- [Sample Implementations](#sample-implementations)
- [How to run these samples](#how-to-run-these-samples)
  - [Run in the Teams Client](#run-in-the-teams-client)
- [Configure the new project to use the new Teams Bot Application](#configure-the-new-project-to-use-the-new-teams-bot-application)
- [Pro Tip: Read the configuration settings using the Azure CLI](#pro-tip-read-the-configuration-settings-using-the-azure-cli)
- [Troubleshooting](#troubleshooting)
- [Further Reading](#further-reading)

## Included Features

- Bots
- Targeted Messaging (`MessageActivity.withRecipient()`)
- Proactive Targeted Messaging (`app.send()`)
- Adaptive Cards with `Action.Execute`
- Mention Stripping (`stripMentionsText()`)

## Interaction with Bot

The bot works in **channels** and **group chats** and supports the following commands:

### Set a Reminder

- `remind me in 5 minutes to check email`
- `remind me in 1 hour meeting starts`
- `remind me in 30 seconds test`
- `remind @John in 10 minutes review PR`

### Supported Time Formats

- Seconds: `30 seconds`, `30 secs`, `30s`
- Minutes: `5 minutes`, `5 mins`, `5m`
- Hours: `1 hour`, `2 hrs`, `1h`

### Manage Reminders

- `my-reminders` — View your active reminders (targeted, only you see the list)
- `cancel-reminder [id]` — Cancel a specific reminder
- `reminder-help` — Show help information

### Where Targeted Messages Are Used

Every bot response in this sample is a targeted message — only the intended recipient can see it:

| Action | Recipient | How |
|---|---|---|
| Setting a reminder | The command sender | `send(new MessageActivity(...).withRecipient(creator, true))` |
| Reminder delivery | The reminder target | `app.send(conversationId, new MessageActivity(...).withRecipient(recipient, true))` |
| Viewing active reminders | The command sender | `send(new MessageActivity(...).withRecipient(sender, true))` |
| Snooze confirmation | The snoozing user | `send(new MessageActivity(...).withRecipient(recipient, true))` |

## Sample Implementations

| Language | Framework | Directory |
|----------|-----------|-----------|
| TypeScript | Node.js | [nodejs/bot-targeted-messages](nodejs/bot-targeted-messages/README.md) |

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

## Configure the new project to use the new Teams Bot Application

For NodeJS and Python you will need a `.env` file with the next fields

```
TENANT_ID=
CLIENT_ID=
CLIENT_SECRET=
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
