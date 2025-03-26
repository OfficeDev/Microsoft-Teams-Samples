---
page_type: sample
description: This sample app demonstrates use of different batch operations as part of the Microsoft Teams APIs.
products:
  - office-teams
languages:
  - nodejs
extensions:
  contentType: samples
  createdDate: "08/24/2023 1:00:00 PM"
urlFragment: officedev-microsoft-teams-samples-bot-batch-operations-nodejs
---

# Teams Batch Operations Bot

Bot Framework v4 Batch Operations Bot sample for Teams.

This bot has been created using [Bot Framework](https://dev.botframework.com). This sample demonstrates the different batch operations you can make from your bot.

## Included Features

- Bots
- Adaptive Cards
- Batch API calls

## Prerequisites

- Microsoft Teams is installed and you have an account
- [NodeJS](https://nodejs.org/en/)
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution
- [Teams Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call into the bot.

1. Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"

   ```

## Setup for Bot

In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration).

- For bot handle, create a name.
- Select "Use existing app registration" (Create the app registration in Azure Active Directory beforehand.)
- **_If you don't have an Azure account_** create an [Azure free account here](https://azure.microsoft.com/free/)

In the new Azure Bot resource in the Portal,

- Ensure that you've [enabled the Teams Channel](https://learn.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- In Settings/Configuration/Messaging endpoint, enter the current `https` URL you were given by running ngrok. Append with the path `/api/messages`

## Setup for code

1. Clone the repository

   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

1. In a terminal, navigate to `samples/bot-batch-operations/nodejs`

1. Install modules

   ```bash
   npm install
   ```

1. Update the `.env` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.) `MicrosoftAppTenantId` will be the id for the tenant where application is registered.

- Also, set MicrosoftAppType in the `.env`. (**Allowed values are: MultiTenant(default), SingleTenant, UserAssignedMSI**)

1. Run your bot at the command line:

   ```bash
   npm start
   ```

1. **_This step is specific to Teams._**
   - **Edit** the `manifest.json` contained in the `appPackage` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) _everywhere_ you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
   - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
   - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
   - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
   - Add the app to personal/team/groupChat scope (Supported scopes)

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-conversation/nodejs/index.js#L46) line and put your debugger for local debug.

## Running the sample

You can interact with this bot in Teams by sending it a message, or selecting a command from the command list. The bot will respond to the following strings.

1. **Show Welcome**

- **Result:** The bot will send the welcome card for you to interact with
- **Valid Scopes:** personal, group chat, team chat

![image](Images/WelcomeCard.png)

2. **Message a List of Users**

- **Result:** The bot will send a message to each corresponding user from the list of user IDs provided in the dialog and return the operation ID.
- **Valid Scopes:** personal, group chat, team chat

![image](Images/MessageAListOfUsers.png)

3. **Send message to all users in a tenant**

- **Result:** The bot will send a 1-on-1 message to each user in the current tenant and return the operation ID
- **Valid Scopes:** personal, group chat, team chat

![image](Images/MessageAllUsersInTenant.png)

4. **Message All Users in a Team:**

- **Result:** The bot will send a 1-on-1 message to each user in the team corresponding to the ID provided in the dialog and return the operation ID
- **Valid Scopes:** personal, group chat, team chat

![image](Images/MessageAllUsersInTeam.png)

5. **Message A List of Channels**

- **Result:** The bot will send a message to each channel from the list of channel IDs provided in the dialog (by default the current channel) and return the operation ID.
- **Valid Scopes:** personal, group chat, team chat

![image](Images/MessageAListOfChannels.png)

6. **Get Operation State**

- **Result:** The bot will open a dialog and ask for the operation ID and return if the corresponding operation is Ongoing, Completed, or Failed
- **Valid Scopes:** personal, group chat, team chat

![image](Images/GetOperationState.png)

7. **Get Failed Entries Paginated**

- **Result:** The bot will show a list of the failed entries, and provide the continuation token in case of multiple pages of results
- **Valid Scopes:** personal, group chat, team chat

![image](Images/GetFailedEntries.png)

8. **Cancel Operation**

- **Result:** The bot will open a dialog and ask for the operation ID and cancel the corresponding operation.
- **Valid Scopes:** personal, group chat, team chat

![image](Images/CancelOperation.png)

You can select an option from the command list by typing `@TeamsBatchOperationsBot` into the compose message area and `What can I do?` text above the compose area.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Messages in bot conversations](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?tabs=dotnet)
