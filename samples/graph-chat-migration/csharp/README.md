---
page_type: sample
description: This sample app demonstrates use of different bot conversation events available in bot framework v4 for personal and teams scope.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "10-04-2022 20:15:25"
urlFragment: officedev-microsoft-teams-samples-bot-conversation-nodejs
---

# Graph Chat Migration Bot

This is a Microsoft Teams bot sample that demonstrates how to use Microsoft Graph APIs for chat migration. Implements functionality to migrate chat messages with specific timestamps

## Included Features

* Graph API's
* Teams Bots.

## Graph Chat Migration Bot

![Conversation Bot](GraphChatMigration/Images/MigrationBot.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

1. Update the **appsettings.developement.json** with the **UserId** and **UserName** values
The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio [Microsoft 365 Agents Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`.
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
</br>![image](https://raw.githubusercontent.com/OfficeDev/TeamsFx/dev/docs/images/visualstudio/debug/debug-button.png)
1. In the opened web browser, select Add button to install the app in Teams
> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because the Teams service needs to call into the bot.

1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

1) Setup for Bot

   In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration).
    - For bot handle, make up a name.
    - Select "Use existing app registration" (Create the app registration in Microsoft Entra ID beforehand.)
    - __*If you don't have an Azure account*__ create an [Azure free account here](https://azure.microsoft.com/free/)
    
   In the new Azure Bot resource in the Portal, 
    - Ensure that you've [enabled the Teams Channel](https://learn.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - In Settings/Configuration/Messaging endpoint, enter the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages`

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

1) If you are using Visual Studio
   - Launch Visual Studio
   - File -> Open -> Project/Solution
   - Navigate to `samples/graph-chat-migration/csharp/GraphChatMigration` folder
   - Select `GraphChatMigration.csproj` file
   - Press `F5` to run the project 

1) Update the `appsettings.json` configuration for the bot to use the BOT_ID, BOT_PASSWORD, BOT_TENANT_ID generated in Step 2 (App Registration creation). (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)
    - Also, set BOT_TYPE in the `appsettings.json`. (**Allowed values are: MultiTenant(default), SingleTenant, UserAssignedMSI**)

1) Run your bot, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `appPackage` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app in personal/groupchat/team scope (supported scopes)

## Running the sample

1. Install App
![InstallApp](GraphChatMigration/Images/1.InstallApp.png)

2. App Installed
![AppInstalled](GraphChatMigration/Images/2.Installed.png)

3. Start Migration Group Chat
![AppInstalled](GraphChatMigration/Images/3.StartMigrationGroupChat.png)

4. Post Message Group Chat
![AppInstalled](GraphChatMigration/Images/4.PostMessageGroupChat.png)

5. Details filled and Message sent
![AppInstalled](GraphChatMigration/Images/5.DetailsFilledMessageSent.png)

6. Message added to group chat
![AppInstalled](GraphChatMigration/Images/6.MessageInGroupChat.png)

7. Completed Migration Group Chat
![AppInstalled](GraphChatMigration/Images/7.CompleteMigrationGroupChat.png)

8. Start Migration Channel
![AppInstalled](GraphChatMigration/Images/8.StartMigrationChannel.png)

9. Post Message Group Chat
![AppInstalled](GraphChatMigration/Images/9.DetailsFilledMessageSentChannel.png)

10. Details filled and Message sent
![AppInstalled](Images/9.DetailsFilledMessageSentChannel.png)

11. Message added to channel
![AppInstalled](GraphChatMigration/Images/10.MessageInChannel.png)

12. Completed Migration Channel
![AppInstalled](GraphChatMigration/Images/11.CompleteMigrationChannel.png)

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/graph-chat-migration-csharp" />