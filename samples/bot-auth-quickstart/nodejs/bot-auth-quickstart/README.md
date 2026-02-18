# Teams Bot Auth Quickstart with Proactive Installation

This sample demonstrates how to implement Single Sign-On (SSO) authentication for Microsoft Teams bots using Azure Active Directory. It showcases:

- **Authentication**: Seamless SSO authentication using Azure AD with OAuth support
- **Microsoft Graph Integration**: Access user data and perform operations on behalf of authenticated users  
- **Proactive App Installation**: Automatically install the Teams app for all members in a team or group chat using Graph APIs
- **Dynamic Catalog Discovery**: Automatically discover the app catalog ID without manual configuration
- **Proactive Messaging**: Send messages to team/chat members

> **IMPORTANT**: The manifest file in this app adds "token.botframework.com" to the list of `validDomains`. This must be included in any bot that uses the Teams SDK OAuth flow.

## Table of Contents

- [Interaction with Bot](#interaction-with-bot)
- [Sample Implementations](#sample-implementations)
- [Microsoft Graph Integration](#microsoft-graph-integration)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
  - [Option 1: Using Microsoft 365 Agents Toolkit](#option-1-using-microsoft-365-agents-toolkit-for-vs-code)
  - [Option 2: Manual Setup](#option-2-manual-setup)
- [Running the Sample](#running-the-sample)
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

| Language | Framework | Directory |
|----------|-----------|-----------|
| C# | .NET 10 / ASP.NET Core | [dotnet/bot-auth-quickstart](dotnet/bot-auth-quickstart/README.md) |
| TypeScript | Node.js | [nodejs/bot-auth-quickstart](nodejs/bot-auth-quickstart/README.md) |
| Python | Python | [python/bot-auth-quickstart](python/bot-auth-quickstart/README.md) |

## Microsoft Graph Integration

This sample demonstrates comprehensive Microsoft Graph API integration to access Microsoft 365 data and services on behalf of authenticated users.

### Graph API Capabilities

The bot leverages Microsoft Graph to:

1. **User Profile Access**:
   - Retrieves authenticated user's profile information
   - Accesses user display name, email, and other profile data
   - Uses delegated permissions with user consent

2. **Team and Chat Member Management**:
   - Reads team and chat membership information
   - Retrieves member details for proactive messaging
   - Uses `ChatMember.Read.All` application permission

3. **Proactive App Installation**:
   - Automatically installs the Teams app for team/chat members
   - Uses Graph API to manage app installations programmatically
   - Requires `TeamsAppInstallation.ReadWriteForUser.All` and `TeamsAppInstallation.ReadWriteForTeam.All` permissions

4. **Catalog Discovery**:
   - Dynamically discovers the app catalog ID
   - Eliminates manual configuration of catalog information
   - Uses Graph API to query installed apps

### Graph API Permissions Used

This sample requires the following Microsoft Graph permissions:

**Delegated Permissions** (user consent required):
- `User.Read` - Read the signed-in user's profile

**Application Permissions** (admin consent required):
- `User.Read.All` - Read all users' profiles
- `ChatMember.Read.All` - Read all chat and team members
- `TeamsAppInstallation.ReadWriteForUser.All` - Manage app installations for users
- `TeamsAppInstallation.ReadWriteForTeam.All` - Manage app installations for teams

### Authentication Flow

The sample uses Azure AD SSO (Single Sign-On) with the Teams SDK OAuth flow:

1. User initiates login via the bot
2. Bot requests SSO token from Teams
3. Teams prompts user for consent (first time only)
4. Bot exchanges SSO token for Graph access token
5. Bot makes Graph API calls on behalf of the user

### Graph API Endpoints Used

- `GET /me` - Get current user's profile
- `GET /users/{id}` - Get user information
- `GET /teams/{id}/members` - Get team members
- `GET /chats/{id}/members` - Get chat members
- `POST /users/{id}/teamwork/installedApps` - Install app for user
- `GET /appCatalogs/teamsApps` - Query Teams app catalog

For more information about Microsoft Graph, see the [Microsoft Graph documentation](https://developer.microsoft.com/graph) and [Graph Explorer](https://developer.microsoft.com/graph/graph-explorer) to test API calls.

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) latest version or equivalent tunneling solution
- Language-specific prerequisites:
  - **Node.js**: [NodeJS](https://nodejs.org/en/download/) version 16.14.2 or higher
  - **.NET**: [.NET 10 SDK](https://dotnet.microsoft.com/download)
  - **Python**: Python 3.8 or higher

> **Note**: Authentication samples require Microsoft Teams and cannot be tested using the `agentsplayground` tool. The OAuth flow and SSO authentication features only work within the Teams client environment.

## Setup Instructions

### Option 1: Using Microsoft 365 Agents Toolkit for VS Code

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
2. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
3. Select **File > Open Folder** in VS Code and choose your language-specific sample directory (nodejs/dotnet/python)
4. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
5. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client
6. In the browser that launches, select the **Add** button to install the app to Teams

> If you do not have permission to upload custom apps (sideloading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

### Option 2: Manual Setup

> Note: These instructions are for running the sample on your local machine. The tunnelling solution is required because the Teams service needs to call into the bot.

#### 1. Setup for Bot SSO

- Refer to [Bot SSO Setup document](BotSSOSetup.md)
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the Azure bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint

> NOTE: When you create your app registration in Azure portal, you will create an App ID and App password - make sure you keep these for later.

#### 2. Setup Local Tunnel

**Using ngrok:**

```bash
ngrok http 3978 --host-header="localhost:3978"
```

**Using dev tunnels:**

Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access:

```bash
devtunnel host -p 3978 --allow-anonymous
```

#### 3. Register Azure AD Application

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
  - `User.Read.All` (for reading user information)
  - `ChatMember.Read.All` (for reading chat members)
  - `TeamsAppInstallation.ReadWriteForUser.All` (for proactive installation)
  - `TeamsAppInstallation.ReadWriteForTeam.All` (for proactive installation in teams)
- Click **Add permissions**
- Click **Grant admin consent** to grant admin consent for the required permissions

#### 4. Setup Code

**Clone the repository:**

```bash
git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
```

**Navigate to your chosen language sample directory:**

```bash
# For Node.js:
cd samples/bot-auth-quickstart/nodejs/bot-auth-quickstart

# For .NET:
cd samples/bot-auth-quickstart/dotnet/bot-auth-quickstart

# For Python:
cd samples/bot-auth-quickstart/python/bot-auth-quickstart
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
pip install -r requirements.txt
```

**Configure environment variables:**

Update the configuration file (`.env`, `appsettings.json`, or `.env` depending on language) with the values from step 3 (Azure AD app registration):

- `MicrosoftAppId` / `CLIENT_ID` - The Application (client) ID from step 3
- `MicrosoftAppPassword` / `CLIENT_PASSWORD` - The client secret value from step 3C
- `MicrosoftAppType` / `BOT_TYPE` - Set to `MultiTenant` if your bot supports multiple tenants, otherwise `SingleTenant`
- `MicrosoftAppTenantId` / `TENANT_ID` - Your Directory (tenant) ID from step 3 (required for SingleTenant)
- `connectionName` / `CONNECTION_NAME` - The name of your Azure Bot OAuth connection created in step 1

**Note**: The `APP_CATALOG_TEAM_APP_ID` is discovered dynamically by the bot when you use the installation feature, so no manual configuration is needed.

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

#### 5. Setup Teams App Manifest

**Edit the manifest:**

- Navigate to the `appManifest/` or `appPackage/` folder
- Edit `manifest.json` and replace:
  - `{MicrosoftAppId}` or `<<MICROSOFT-APP-ID>>` - Replace with your Application (client) ID from step 3 (appears in multiple places)
  - `<<DOMAIN-NAME>>` - Replace with your tunnel domain:
    - For ngrok: `1234.ngrok-free.app` (from `https://1234.ngrok-free.app`)
    - For dev tunnels: `12345.devtunnels.ms`

**Create app package:**

- Zip up the contents of the `appManifest/` or `appPackage/` folder to create a `manifest.zip`

**Upload to Teams:**

- In Microsoft Teams, go to **Apps** (left sidebar)
- Click **Manage your apps** → **Upload an app** → **Upload a custom app**
- Select the `manifest.zip` file

## Running the Sample

Once the bot is running and added to Teams, you can interact with it using the following commands:

### Bot Commands

**Authentication:**
- **Login** - Sign in to the bot using your Microsoft 365 account
  - The bot will request your consent to access your profile information
  - After consent, the bot exchanges an SSO token and accesses Microsoft Graph on your behalf
  - You remain logged in until you explicitly logout

**Proactive Installation:**
- **Check and Install** or **Install** - Proactively installs the app for all members in the team or group chat
  - The bot automatically discovers the catalog ID from installed apps
  - Shows installation status for each member (newly installed, already installed, or errors)

**Messaging:**
- **Send message** or **Send** - Sends a message mentioning all members in the conversation
  - Retrieves all members from the team/chat
  - Posts a message mentioning everyone

**Sign Out:**
- **Logout** - Sign out from the bot and clear your authentication session

## Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable
- Ensure your configuration file (`.env`, `appsettings.json`) is setup correctly
- Verify that "token.botframework.com" is included in `validDomains` in your manifest.json
- For OAuth issues, confirm your Azure AD app registration has the correct redirect URIs
- Check that admin consent has been granted for the required Graph API permissions
- Use the Channels UI in Azure Bot Service in the Azure Portal to see detailed endpoint errors (not available in Teams Developer Portal)

## Further Reading

### Teams Development
- [Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/) - Official Microsoft Teams platform documentation
- [Microsoft Teams Developer Platform](https://docs.microsoft.com/en-us/microsoftteams/platform/) - Comprehensive guide for Teams app development

### Authentication & Graph API
- [Teams Bot Authentication](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots) - SSO authentication for Teams bots
- [Microsoft Graph API](https://developer.microsoft.com/graph) - Access Microsoft 365 data and services
- [Graph API Permissions](https://learn.microsoft.com/graph/permissions-reference) - Complete permissions reference

### Proactive Messaging
- [Proactive Installation using Graph API](https://learn.microsoft.com/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages) - Guide for proactive bot implementation
- [Send Proactive Messages](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages) - Best practices for proactive messaging

### Tools & Resources
- [Microsoft 365 Agents Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) - VS Code extension for Teams development
- [Azure Bot Service](https://azure.microsoft.com/services/bot-services/) - Cloud-based bot development service
