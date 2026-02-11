---
page_type: sample
description: Teams bot with SSO authentication and proactive messaging capabilities.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07-07-2021 13:38:26"
urlFragment: officedev-microsoft-teams-samples-bot-auth-quickstart-csharp_dotnetcore
---
# Auth Bot

A Microsoft Teams bot sample demonstrating two key functionalities:
1. **SSO Authentication** - Single Sign-On with Microsoft Entra ID to authenticate users and access Microsoft Graph data
2. **Proactive Messaging** - Install the bot app for team members and send proactive notifications

## Included Features
* Teams SSO (bots)
* Proactive Messaging
* Graph API
* App Installation for Team Members

## Bot Commands

| Command | Description |
|---------|-------------|
| `login` / `signin` | Sign in using SSO |
| `logout` / `signout` | Sign out from the bot |
| `install` | Install the app for all team/group chat members |
| `send` | Send proactive messages to all team/group chat members |
| Any other text | Triggers sign-in if not authenticated, shows user details if authenticated |

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- [.NET](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) version 10.0
- [Dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) latest version or equivalent tunneling solution
- [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app
- [Microsoft 365 Agents Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.

1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio [Microsoft 365 Agents Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
1. In the opened web browser, select Add button to install the app in Teams

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Manual Setup

> Note: These instructions are for running the sample on your local machine. The tunnelling solution is required because the Teams service needs to call into the bot.

### 1. Setup Tunneling

Run ngrok - point to port 5130:

```bash
ngrok http 5130 --host-header="localhost:5130"
```

Alternatively, you can use dev tunnels:

```bash
devtunnel host -p 5130 --allow-anonymous
```

### 2. Register Microsoft Entra ID Application

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal
2. Select **New Registration** and set:
   - **Name**: Your app name
   - **Supported account types**: Any account type will work
   - Leave **Redirect URI** empty
   - Click **Register**
3. On the overview page, copy and save the **Application (client) ID** and **Directory (tenant) ID**
4. Navigate to **API Permissions** and add:
   - Microsoft Graph -> Delegated permissions: `User.Read` (enabled by default)
   - For proactive messaging, add Application permissions: `TeamsAppInstallation.ReadWriteForUser.All`, `Chat.ReadWrite.All`
   - Grant admin consent for the required permissions
5. Navigate to **Certificates & Secrets** and create a new client secret

### 3. Configure the Application

Modify the `appsettings.json` file with your values:

```json
{
  "Teams": {
    "ClientId": "<Your-App-Client-ID>",
    "ClientSecret": "<Your-Client-Secret>",
    "TenantId": "<Your-Tenant-ID>",
    "ConnectionName": "<Your-OAuth-Connection-Name>",
    "TeamsAppId": "<Your-Teams-App-Catalog-ID>"
  }
}
```

### 4. Run the Application

Using Visual Studio:
1. Open `BotAuthQuickstart.slnx` in Visual Studio
2. Press `F5` to run the project

### 5. Update and Upload App Manifest

1. Edit the `manifest.json` in the `M365Agent/appPackage/` folder with your App ID
2. Zip up the contents of the `appPackage/` folder
3. Upload the manifest to Teams (Apps > Upload a custom app)


## Further Reading

- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Proactive Messages](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages)
- [Microsoft Teams Developer Platform](https://docs.microsoft.com/en-us/microsoftteams/platform/)
