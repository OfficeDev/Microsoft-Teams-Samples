# Teams Bot Auth Quickstart

This sample demonstrates how to implement Single Sign-On (SSO) authentication for Microsoft Teams bots using Azure Active Directory. It showcases:

- **Authentication**: Seamless SSO authentication using Azure AD with OAuth support
- **Microsoft Graph Integration**: Access user data and perform operations on behalf of authenticated users

> **IMPORTANT**: The manifest file in this app requires the following fields for the Teams SDK OAuth flow:
> - **`validDomains`**: Must include `"token.botframework.com"` to allow the OAuth token flow.
> - **`webApplicationInfo`**: Must be configured with your Azure AD app''s `id` (App Id / Client ID) and `resource` (e.g., `api://botid-{AppId}`). This is required for SSO authentication to work in Teams.
> Both fields must be present in any bot that uses Teams SSO or OAuth authentication.

## Table of Contents

- [Interaction with Bot](#interaction-with-bot-application)
- [Sample Implementations](#sample-implementations)
- [Microsoft Graph Integration](#microsoft-graph-integration)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Troubleshooting](#troubleshooting)
- [Further Reading](#further-reading)

## Interaction with Bot Application

![Bot Auth Quickstart](bot-auth-quickstart.gif)

The bot responds to the following commands:

- **signin** - Sign in using your Microsoft 365 account with SSO authentication
- **profile** - View your profile information (Name, Email, Job Title, Department, Office)
- **signout** - Sign out from the bot and clear authentication session

## Sample Implementations

| Language | Framework | Directory |
|----------|-----------|-----------|
| C# | ASP.NET Core | [dotnet/bot-auth-quickstart](dotnet/bot-auth-quickstart/README.md) |
| TypeScript | Node.js | [nodejs/bot-auth-quickstart](nodejs/bot-auth-quickstart/README.md) |
| Python | Python | [python/bot-auth-quickstart](python/bot-auth-quickstart/README.md) |

## Microsoft Graph Integration

This sample demonstrates comprehensive Microsoft Graph API integration to access Microsoft 365 data and services on behalf of authenticated users.

### Graph API Capabilities

The bot leverages Microsoft Graph to:

1. **User Profile Access**:
   - Retrieves authenticated user''s profile information
   - Accesses user display name, email, job title, department, and office location
   - Uses delegated permissions on behalf of the signed-in user

### Graph API Permissions Used

This sample requires the following Microsoft Graph permissions:

**Delegated Permissions** (user or admin consent required):
- `User.Read` - Read the signed-in user''s profile

### User Authentication Flow

The sample uses Azure AD SSO (Single Sign-On) with the Teams SDK OAuth flow:

1. User initiates login via the bot
2. Bot sends an Adaptive Card configured with the OAuth Signing Resource
3. Teams prompts the user for consent (on first use) - see [Consent Flow](#consent-flow) below
4. Bot exchanges the SSO token with permissions to access Graph resources
5. Bot makes Graph API calls on behalf of the user

### Consent Flow

Teams SSO involves two types of consent:

- **User consent**: On the first sign-in, the user is prompted to consent to the permissions (e.g., `User.Read`). After consent is granted, subsequent sign-ins are silent - no prompt is shown.
- **Admin consent**: An organization''s administrator can pre-consent on behalf of all users in the tenant. If admin consent has been granted, individual users will not see a consent prompt.

> **Note**: If you need to reset consent and re-trigger the consent prompt (e.g., for testing), you can do so by:
> 1. Go to [My Apps](https://myapps.microsoft.com) -> find your app -> **Remove** it, or
> 2. In the [Azure Portal](https://portal.azure.com) -> **Microsoft Entra ID** -> **Enterprise Applications** -> find your app -> **Permissions** -> **Revoke user consent**, or
> 3. Have the user sign out of the bot (`signout` command) and clear the Teams app cache.

### Graph API Endpoints Used

- `GET /me` - Get current user''s profile

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) latest version or equivalent tunneling solution
- Language-specific prerequisites:
  - **Node.js**: [NodeJS](https://nodejs.org/en/download/)
  - **.NET**: [.NET SDK](https://dotnet.microsoft.com/download)
  - **Python**: [Python](https://www.python.org/downloads/)
- An Azure subscription (the SSO/OAuth flow requires an Azure Bot resource, not a Teams-managed bot)

## Setup Instructions

The [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/) provisions the Microsoft Entra app, Azure Bot resource, and Teams app manifest, and writes the credentials into your project. The OAuth/SSO specifics (redirect URI, Application ID URI, manifest `webApplicationInfo` / `validDomains`, and the OAuth connection on the Azure Bot resource) are not yet automated by the CLI and are configured manually after provisioning.

### 1. Install the Teams Developer CLI

```bash
npm install -g @microsoft/teams.cli
teams login
```

### 2. Setup Local Tunnel

Create a persistent tunnel for port 3978 with anonymous access so it can be reused across projects:

```bash
devtunnel create -a my-tunnel
devtunnel port create -p 3978 my-tunnel
devtunnel host my-tunnel
```

Take note of the URL shown after *Connect via browser:*.

### 3. Provision the App with the Teams Developer CLI

From the language-specific sample directory you want to run, provision the Microsoft Entra app, Azure Bot resource, and manifest, and write credentials into your environment file. SSO requires an Azure-hosted bot, so the `--azure` flag is required:

For Node.js and Python (`nodejs/bot-auth-quickstart` or `python/bot-auth-quickstart`):

```bash
teams app create --name "Bot Auth Quickstart" --azure \
  --subscription <azure-subscription-id> \
  --resource-group <azure-resource-group> --create-resource-group \
  --endpoint https://<your-tunnel-domain>/api/messages --env .env
```

For .NET (`dotnet/bot-auth-quickstart`):

```bash
teams app create --name "Bot Auth Quickstart" --azure \
  --subscription <azure-subscription-id> \
  --resource-group <azure-resource-group> --create-resource-group \
  --endpoint https://<your-tunnel-domain>/api/messages --env appsettings.json
```

The command creates the Microsoft Entra app registration, an Azure Bot resource pointing at your tunnel endpoint, a Teams app manifest, and writes `CLIENT_ID`, `CLIENT_SECRET`, and `TENANT_ID` into the environment file you specified. Save the printed Teams app ID and Bot resource name for the next steps.

### 4. Configure the AAD App for SSO

Open the app registration the CLI just created in the [Microsoft Entra ID - App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

**A) Add the OAuth redirect URI:**

- Under **Manage**, navigate to **Authentication** -> **Add a platform** -> **Web**
- Set the **Redirect URI** to `https://token.botframework.com/.auth/web/redirect`
- Click **Configure**

**B) Expose an Application ID URI:**

- Under **Manage**, navigate to **Expose an API**
- Click **Add** next to **Application ID URI** and set it to `api://botid-<Application (client) ID>` (the value used in `webApplicationInfo.resource` in the manifest)

**C) Verify Graph permissions:**

- Navigate to **API Permissions**
- Ensure **Microsoft Graph** -> **Delegated** -> `User.Read` is present (added by default)
- Click **Grant admin consent** if your tenant requires it

For more SSO background, see [Bot SSO Setup document](BotSSOSetup.md).

### 5. Configure the SSO Manifest Fields

Edit `appPackage/manifest.json` (or `appManifest/manifest.json`) and add the SSO-specific blocks:

```json
"validDomains": [
  "token.botframework.com",
  "<your-tunnel-domain>"
],
"webApplicationInfo": {
  "id": "<Application (client) ID>",
  "resource": "api://botid-<Application (client) ID>"
}
```

Re-package and re-upload the manifest:

```bash
teams app package
teams app update <teamsAppId> --file appPackage/<package>.zip
```

### 6. Create the OAuth Connection on the Azure Bot

In the Azure Portal, open the Azure Bot resource created by the CLI -> **Settings** -> **Configuration** -> **OAuth Connection Settings** and add a new connection. Use **Azure Active Directory v2** as the service provider, your AAD app''s client ID/secret/tenant ID, and `User.Read` as the scope. Save the connection name and add it to your environment file as `CONNECTION_NAME` (the value referenced by the sample).

> The Teams Developer CLI does not currently configure OAuth connections on the Azure Bot resource; this step still needs to be done in the portal.

### 7. Setup Code

**Navigate to the sample directory:**

```bash
# For Node.js:
cd samples/TeamsSDK/bot-auth-quickstart/nodejs/bot-auth-quickstart

# For .NET:
cd samples/TeamsSDK/bot-auth-quickstart/dotnet/bot-auth-quickstart

# For Python:
cd samples/TeamsSDK/bot-auth-quickstart/python/bot-auth-quickstart
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

**Confirm environment variables:**

The CLI populates `CLIENT_ID`, `CLIENT_SECRET`, and `TENANT_ID` from step 3. Manually add the `CONNECTION_NAME` value from step 6 to the same environment file.

### 8. Start the Bot

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
python main.py
```

## Troubleshooting

- If Teams cannot communicate with your bot, verify your DevTunnels URL is reachable
- Ensure your configuration file (`.env`, `appsettings.json`) is set up correctly
- Verify that `"token.botframework.com"` is included in `validDomains` in your manifest.json
- For OAuth issues, confirm your Azure AD app registration has the correct redirect URIs
- Check that admin consent has been granted for the required Graph API permissions
- Use the Channels UI in Azure Bot Service in the Azure Portal to see detailed endpoint errors

### Enabling Verbose Logs

Detailed logs can help diagnose authentication and connection issues.

**For .NET**, set the log level in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug"
    }
  }
}
```

**For Node.js (TypeScript)**, logging is configured directly in the app via `ConsoleLogger`:
```typescript
const app = new App({
  logger: new ConsoleLogger(''@samples/TeamsSDK/bot-auth-quickstart'', { level: ''debug'' }),
});
```

**For Python**, logging is configured via `ConsoleLogger` in `main.py`:
```python
from microsoft_teams.common import ConsoleLogger, ConsoleLoggerOptions

logger = ConsoleLogger().create_logger("bot-auth-quickstart", ConsoleLoggerOptions(level="debug"))
app = App(logger=logger)
```

### Common Issues

**Consent not granted**
- The user declined the consent prompt or admin consent is required for the tenant.
- Ensure the Azure AD app has the correct API permissions configured and that you (or a tenant admin) have granted consent.
- Check that `webApplicationInfo` in `manifest.json` references the correct Application client ID.

**Invalid connection name**
- The OAuth connection name in your bot code does not match the connection name configured in Azure Bot Service.
- In the Azure Portal, go to your Azure Bot resource -> **Settings** -> **OAuth Connection Settings** and verify the connection name matches the value used in your code (e.g., `connectionName` in `.env` or `appsettings.json`).

**Invalid OAuth connection settings**
- The OAuth connection in Azure Bot Service is misconfigured (wrong client ID, client secret, or tenant ID).
- In the Azure Portal, open your OAuth Connection Settings and use the **Test Connection** button to validate the settings.
- Ensure the client secret has not expired and the Application (client) ID matches your Azure AD app registration.

**Redirect URI not set**
- The redirect URI required by the OAuth flow is missing from the Azure AD app registration.
- In the Azure Portal, open your app registration -> **Authentication** -> **Redirect URIs** and add `https://token.botframework.com/.auth/web/redirect`.

## Further Reading

### Teams Development
- [Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/) - Official Microsoft Teams platform documentation
- [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/) - Command-line provisioning for Teams apps

### Authentication & Graph API
- [Teams Bot Authentication](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots) - SSO authentication for Teams bots
- [Microsoft Graph API](https://developer.microsoft.com/graph) - Access Microsoft 365 data and services
- [Graph API Permissions](https://learn.microsoft.com/graph/permissions-reference) - Complete permissions reference

### Tools & Resources
- [Azure Bot Service](https://azure.microsoft.com/services/bot-services/) - Cloud-based bot development service