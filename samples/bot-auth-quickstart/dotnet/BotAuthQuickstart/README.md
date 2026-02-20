# Auth Bot

A Microsoft Teams bot with SSO authentication and Microsoft Graph API integration.

## Features

- **SSO Authentication** - Single Sign-On with Microsoft Entra ID
- **Graph API Integration** - List group chats where the user is a member


## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- One of the following:
  - [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) with Microsoft 365 Agents Toolkit
  - [VS Code](https://code.visualstudio.com/) with Teams Toolkit extension
  - .NET CLI (no IDE required)

## Run the sample

> **Note:** This sample uses SSO authentication and therefore **does not work** in Microsoft 365 Agents Playground. You must test it in Microsoft Teams.

### Using Microsoft 365 Agents Toolkit (Recommended)

1. Open the solution in Visual Studio
2. Right-click the `M365Agent` project and select **Microsoft 365 Agents Toolkit > Prepare Teams App Dependencies**
3. Press `F5` to start debugging

### Using Command Line

Before running with CLI, you need to configure OAuth:

1. **Register an Azure AD App:**
   - Go to [Azure Portal](https://portal.azure.com) > **App registrations** > **New registration**
   - Set a name and select **Accounts in any organizational directory**
   - Add a redirect URI: `https://token.botframework.com/.auth/web/redirect`

2. **Create an Azure Bot resource:**
   - Go to [Azure Portal](https://portal.azure.com) > **Create a resource** > **Azure Bot**
   - Configure the bot with your App ID and create an OAuth connection setting named `oauthbotsetting` with:
     - Service Provider: `Azure Active Directory v2`
     - Client ID: Your Azure AD App ID
     - Client Secret: Your Azure AD App Secret
     - Scopes: `User.Read Chat.Read`

3. **Update `appsettings.json`:**

```json
{
  "Teams": {
    "ClientId": "<your-bot-app-id>",
    "ClientSecret": "<your-bot-app-secret>",
    "TenantId": "<your-tenant-id>",
    "ConnectionName": "oauthbotsetting"
  }
}
```

4. **Navigate to this directory and run:**

```sh
cd BotAuthQuickstart
dotnet run
```

The bot will start listening on `http://localhost:5130`.

## Further Reading

See the [root README](../../README.md) for detailed setup and configuration instructions.

