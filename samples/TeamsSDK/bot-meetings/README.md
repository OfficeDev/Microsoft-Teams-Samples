# Bot Meetings

This sample demonstrates how to handle real-time meeting events and retrieve meeting transcripts in Microsoft Teams. It showcases:

- **Real-time Meeting Events** - Receives and displays meeting start, end, and participant join/leave events
- **Meeting Transcripts** - Retrieves and displays meeting transcripts via Microsoft Graph API
- **Adaptive Cards** - Interactive cards for viewing transcripts in task modules
- **RSC Permissions** - Resource-specific consent for meeting access

## Table of Contents

- [Interaction with Bot](#interaction-with-bot)
- [Sample Implementations](#sample-implementations)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Running the Sample](#running-the-sample)
- [Troubleshooting](#troubleshooting)
- [Further Reading](#further-reading)

## Interaction with Bot

![Bot Meetings](Images/bot-meetings.gif)

## Sample Implementations

| Language | Framework | Directory |
|----------|-----------|-----------|
| C# | .NET / ASP.NET Core | [dotnet/bot-meetings](dotnet/bot-meetings/README.md) |
| TypeScript | Node.js | [nodejs/bot-meetings](nodejs/bot-meetings/README.md) |
| Python | Python | [python/bot-meetings](python/bot-meetings/README.md) |

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) latest version or equivalent tunneling solution

## Setup Instructions

To run these samples in the Teams Client, you need to provision your app in a M365 Tenant, and configure the app to your DevTunnels URL.

1. Install the tool DevTunnels https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started
2. Get Access to a M365 Developer Tenant https://learn.microsoft.com/en-us/office/developer-program/microsoft-365-developer-program-get-started
3. Create a Teams App with the Bot Feature in the Teams Developer Portal (in your tenant) https://dev.teams.microsoft.com

### 1. Setup Local Tunnel

**Using dev tunnels:**

Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access:

```bash
devtunnel host -p 3978 --allow-anonymous
```

**Using ngrok:**

```bash
ngrok http 3978 --host-header="localhost:3978"
```


### 2. Register Azure AD Application

Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

**A) Create New Registration:**
- Select **New Registration** and on the *register an application page*, set following values:
  - Set **name** to your app name
  - Choose the **supported account types** (any account type will work)
  - Leave **Redirect URI** empty
  - Choose **Register**

**B) Save Application Details:**
- On the overview page, copy and save the **Application (client) ID** and **Directory (tenant) ID**
- You'll need these later when updating your Teams application manifest and configuration files

**C) Create Client Secret:**
- Under **Manage**, navigate to **Certificates & secrets**
- In the **Client secrets** section, click on **+ New client secret**
- Add a description (e.g., "Teams Bot Secret") and select an expiration period
- Click **Add**
- **Important**: Copy the client secret **Value** immediately and save it securely. You won't be able to see it again!

**D) Configure API Permissions:**
- Navigate to **API Permissions**
- Click **Add a permission**
- Select **Microsoft Graph** -> **Delegated permissions**:
  - `User.Read` (enabled by default)
- Select **Microsoft Graph** -> **Application permissions**:
  - `OnlineMeetings.Read.All` (for reading meeting information)
  - `OnlineMeetingTranscript.Read.All` (for reading meeting transcripts)
- Click **Add permissions**
- Click **Grant admin consent** to grant admin consent for the required permissions

> **Note**: Admin consent is required for application permissions. If you are not a Global Administrator, contact your tenant admin to grant consent, or use this link: `https://login.microsoftonline.com/common/adminconsent?client_id=<YOUR_APP_ID>`

**E) Configure Application Access Policy:**

Follow this link - [Configure application access policy](https://docs.microsoft.com/en-us/graph/cloud-communication-online-meeting-application-access-policy)

Follow this link - [Manage policies via PowerShell](https://learn.microsoft.com/en-us/microsoftteams/teams-powershell-managing-teams#manage-policies-via-powershell)


![Policy](Images/Policy.png)

### 3. Create Bot Registration

Navigate to the Teams Developer Portal http://dev.teams.microsoft.com

**Create a new Bot resource:**

1. Navigate to `Tools->Bot management`, and add a `New bot`
2. In Configure, paste the Endpoint address from devtunnels and append `/api/messages`
3. In Client secrets, create a new secret and save it for later

> Note. If you have access to an Azure Subscription in the same Tenant, you can also create the Azure Bot resource ([learn more](https://learn.microsoft.com/en-us/azure/bot-service/abs-quickstart?view=azure-bot-service-4.0&tabs=singletenant)).

**Enable Meeting Participant Events:**

To receive real-time participant join and leave events, enable Meeting event subscriptions for `Participant Join` and `Participant Leave` in your bot on Teams Developer Portal by following the guidance in the [meeting participant events](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet#receive-meeting-participant-events) documentation.

![Extra Setup](Images/event_subscription.png)

### 4. Setup Code

**Navigate to your chosen language sample directory:**

```bash
# For Node.js:
cd samples/TeamsSDK/bot-meetings/nodejs/bot-meetings

# For .NET:
cd samples/TeamsSDK/bot-meetings/dotnet/bot-meetings

# For Python:
cd samples/TeamsSDK/bot-meetings/python/bot-meetings
```

**Install dependencies:**

For Node.js:
```bash
npm install
```

For .NET:
```bash
dotnet restore
```

For Python:
```bash
pip install -e .
```

**Configure environment variables:**

Update the configuration file for your selected language (for Node.js/Python, the `.env` file; for .NET, `appsettings.json` or `launchSettings.json`) with the values from step 2 (Azure AD app registration):

For NodeJS and Python you will need a `.env` file with the following fields:

```
TENANT_ID=<Your Directory (tenant) ID>
CLIENT_ID=<Your Application (client) ID>
CLIENT_SECRET=<Your client secret value>
```

For .NET you need to add these values to `appsettings.json` or `launchSettings.json` using the next syntax:

appSettings.json:

```json
"urls" : "http://localhost:3978",
"Teams": {
    "ClientID": "<Your Application (client) ID>",
    "ClientSecret": "<Your client secret value>",
    "TenantId": "<Your Directory (tenant) ID>"
  },
```

Or to use Env Vars from the profile defined in `launchSettings.json` (using the Environment Configuration Provider):

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

> **Pro Tip**: To obtain the TenantId, ClientId and SecretId you can use the Azure CLI with: `az ad app credential reset --id $appId`
> 
> Note. If you don't have access to an Azure Subscription you can still use the Azure CLI, make sure you login with `az login --allow-no-subscription`

**Start the bot:**

For Node.js:
```bash
npm start
```

For .NET:
```bash
dotnet run
```

For Python:
```bash
python app.py
```

### 5. Setup Teams App Manifest

**Edit the manifest:**

- Navigate to the `appManifest/` or `appPackage/` folder
- Edit `manifest.json` and replace:
  - `{MicrosoftAppId}` or `<<MICROSOFT-APP-ID>>` - Replace with your Application (client) ID from step 2 (appears in multiple places)
  - `<<DOMAIN-NAME>>` - Replace with your tunnel domain:
    - For ngrok: `1234.ngrok-free.app` (from `https://1234.ngrok-free.app`)
    - For dev tunnels: `12345.devtunnels.ms`

The scopes section must include team, and groupChat:

```json
"bots": [
  {
    "botId": "",
    "scopes": [
      "team",
      "personal",
      "groupChat"
    ],
    "isNotificationOnly": false
  }
]
```

**Configure RSC Permissions:**

Add the following RSC (Resource-Specific Consent) permissions to your Teams app `manifest.json` file in the `webApplicationInfo.authorization.permissions.resourceSpecificPermissions` array:

```json
{
    "name": "OnlineMeeting.ReadBasic.Chat",
    "type": "Application"
},
{
    "name": "OnlineMeetingTranscript.Read.Chat",
    "type": "Application"
},
{
    "name": "ChannelMeeting.ReadBasic.Group",
    "type": "Application"
},
{
    "name": "OnlineMeetingParticipant.Read.Chat",
    "type": "Application"
}
```

These permissions allow the bot to:
- Read basic meeting information
- Access meeting transcripts
- Read channel meeting details
- Monitor meeting participant events

**Create app package:**

- Zip up the contents of the `appManifest/` or `appPackage/` folder to create a `manifest.zip`

**Upload to Teams:**

- In Microsoft Teams, go to **Apps** (left sidebar)
- Click **Manage your apps** → **Upload an app** → **Upload a custom app**
- Select the `manifest.zip` file

## Running the Sample

Once the bot is running and added to Teams, you can interact with it in meetings to:

- Receive real-time notifications when meetings start, end, or when participants join/leave
- Request and view meeting transcripts
- Use adaptive cards to interact with meeting data

## Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable
- Ensure your .env or appsettings file is setup correctly
- Verify that admin consent has been granted for the required Graph API permissions
- Check that the application access policy has been configured correctly for your user
- Confirm that Meeting event subscriptions are enabled in the bot registration
- Use the Channels UI in Azure Bot Service in the Azure Portal to see detailed endpoint errors (not available in Teams Developer Portal)

## Further Reading

### Teams Development
- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/) - Official Microsoft Teams platform documentation
- [Microsoft Teams Developer Platform](https://docs.microsoft.com/en-us/microsoftteams/platform/) - Comprehensive guide for Teams app development

### Meeting Events & Transcripts
- [Meeting participant events](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet#receive-meeting-participant-events) - Real-time meeting participant events
- [Configure application access policy](https://docs.microsoft.com/en-us/graph/cloud-communication-online-meeting-application-access-policy) - Application access for online meetings

### Tools & Resources
- [Microsoft 365 Agents Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) - VS Code extension for Teams development
- [Azure Bot Service](https://azure.microsoft.com/services/bot-services/) - Cloud-based bot development service
