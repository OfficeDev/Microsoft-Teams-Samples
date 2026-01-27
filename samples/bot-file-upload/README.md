# Bot File Upload to OneDrive Sample

This sample demonstrates a file upload bot for Microsoft Teams that integrates with OneDrive. When a user uploads a file in a chat and mentions the bot, the bot will ask for permission to upload the file to the user's OneDrive. If the user accepts, the file is uploaded to OneDrive and the user is notified of the successful upload. If the user denies, no action is taken.

## Features

- **File Detection**: Automatically detects when a user uploads a file with a mention to the bot
- **Permission Request**: Prompts the user for consent before uploading to OneDrive
- **OneDrive Integration**: Securely uploads files to the user's OneDrive storage
- **User Notification**: Confirms successful upload or acknowledges denial

## How It Works

1. User uploads a file in the Teams chat and @mentions the bot
2. Bot receives the file attachment and asks the user for permission to upload to OneDrive
3. User responds with their choice:
   - **Accept**: Bot uploads the file to the user's OneDrive and sends a confirmation message
   - **Deny**: Bot acknowledges the response and takes no further action

## Sample Implementations

| Language | Framework | Directory |
|----------|-----------|-----------|
| C# | .NET 10 / ASP.NET Core | [dotnet](dotnet/bot-file-upload/README.md) |
| TypeScript | Node.js | [nodejs](nodejs/bot-file-upload/README.md) |
| Python | Python 3.x | [python](python/bot-file-upload/README.md) |

## Prerequisites

- Microsoft 365 tenant with Teams enabled
- Azure AD app registration with Microsoft Graph permissions for OneDrive access
- Bot Framework registration

# How to run these samples

You can run these samples locally using the `agentsplayground` tool, without provisioning the Teams App, or you can run in the Teams Client after you have provisioned the Teams Application and configured the application with your local DevTunnels URL.

## Run in the `agentsplayground`

Install the tool agentsplayground for your platform

Windows

```
winget install agentsplayground
```

Linux

```
curl -s https://raw.githubusercontent.com/OfficeDev/microsoft-365-agents-toolkit/dev/.github/scripts/install-agentsplayground-linux.sh | bash
```

Other platforms (like MacOS, via npm)

```
npm install -g @microsoft/m365agentsplayground
```

Once the tool is installed, you can run it from your terminal with the command `agentsplayground`, and it will try to connect to `localhost:3978` where your bot is running.

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

appSettings.json


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

To obtain the TenantId, ClientId and SecretId you can use the Azure CLI with:

> Note. If you don't have access to an Azure Subscription you can still use the Azure CLI, make sure you login with `az login --allow-no-subscription` 

```
az ad app credential reset --id $appId
```

## Further Reading

- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)