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

# Teams Conversation Bot

Bot Framework v4 Conversation Bot sample for Teams ([Messages in bot conversations](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?tabs=dotnet)).

This bot has been created using [Bot Framework](https://dev.botframework.com). This sample shows
how to incorporate basic conversational flow into a Teams application. It also illustrates a few of the Teams specific calls you can make from your bot.

## GitHub Copilot Agent Mode

This sample is optimized for use with GitHub Copilot Agents and supports vibe coding workflows.

- Follow the coding style and guidelines described in `.github/copilot-instructions.md`.
- Use the stepwise development plan in `sample-plan.md` for modular feature delivery.
- Run the bot locally with `npm start`.
- Lint and test with `npm run lint` and `npm test`.
- Use provided issue templates to create scoped tasks for Copilot Agents.

Agents can use CLI scripts and config files to help automate testing, linting, and deployment.

## Included Features
* Bots
* Adaptive Cards
* Teams Conversation Events
* Immersive Reading Support for Cards
* AI label
* Citations
* Feedback buttons
* Sensitivity label

## Interaction with bot

![Conversation Bot](Images/Bot_Conversation.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Teams Conversation Bot:** [Manifest](/samples/bot-conversation/csharp/demo-manifest/bot-conversation.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [NodeJS](https://nodejs.org/en/)
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

### Register your app with Azure AD.

  1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  2. Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
  4. Navigate to **API Permissions**, and make sure to add the follow permissions:
   Select Add a permission
      * Select Add a permission
      * Select Microsoft Graph -\> Delegated permissions.
      * `User.Read` (enabled by default)
      * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.


## Setup for bot
In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration).
    - For bot handle, make up a name.
    - Select "Use existing app registration" (Create the app registration in Microsoft Entra ID beforehand.)
    - __*If you don't have an Azure account*__ create an [Azure free account here](https://azure.microsoft.com/free/)
    
   In the new Azure Bot resource in the Portal, 
    - Ensure that you've [enabled the Teams Channel](https://learn.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - In Settings/Configuration/Messaging endpoint, enter the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages`

## Setup for code
1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

1) In a terminal, navigate to `samples/bot-conversation/nodejs`

1) Install modules

    ```bash
    npm install
    ```

1) Update the `.env` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.) `MicrosoftAppTenantId` will be the id for the tenant where application is registered.
 - Also, set MicrosoftAppType in the `.env`. (**Allowed values are: MultiTenant(default), SingleTenant, UserAssignedMSI**)

1) Run your bot at the command line:

    ```bash
    npm start
    ```

1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal/team/groupChat scope (Supported scopes)
   
**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-conversation/nodejs/index.js#L46) line and put your debugger for local debug.

## Running the sample

You can interact with this bot in Teams by sending it a message, or selecting a command from the command list. The bot will respond to the following strings.

1. **Show Welcome**
  - **Result:** The bot will send the welcome card for you to interact with
  - **Valid Scopes:** personal, group chat, team chat

  - **Personal Scope Interactions:**

   **Adding bot UI:**
  ![personal-AddBot ](Images/1.Adding_personal_Scope.png)

   **Added bot UI:**
  ![personal-AddedBot ](Images/2.Added.png)

   **Show Welcome command interaction:**
  ![personal-WelcomeCard-Interaction ](Images/3.Welcome_Card.png)

   - **Group Chat Scope Interactions:**

   **Adding bot UI:**
  ![groupChat-AddBot ](Images/4.Adding_to_GC.png)

   **Added bot UI:**
  ![groupChat-AddedBot ](Images/5.Addes_GC.png)

   **Show Welcome command interaction:**
  ![groupChat-BotCommands-interactions ](Images/6.Welcome_Card_GC.png)

  - **Team Scope Interactions:**

   **Adding bot UI:**
  ![team-AddBot ](Images/7.Adding_to_Team.png)

   **Added bot UI:**
  ![team-AddedBot ](Images/8.Added_to_Team.png)

   **Show Welcome command interaction:**
  ![team-WelcomeCommand-Card ](Images/9.Welcome_Card_Team.png)

2. **MentionMe**
  - **Result:** The bot will respond to the message and mention the user
  - **Valid Scopes:** personal, group chat, team chat

  - **Personal Scope Interactions:**

   **MentionMe command interaction:**
  ![personal-MentionMeCommand ](Images/10.Mention_Me_Chat.png)

   - **Group Chat Scope Interactions:**

   **MentionMe command interaction:**
  ![groupChat-BotCommands-interactions ](Images/11.Mention_Me_GC.png)

  - **Team Scope Interactions:**

   **MentionMe command interaction:**
  ![team-MentionCommand-Interaction ](Images/12.Mention_Me_Team.png)

3. **MessageAllMembers**
  - **Result:** The bot will send a 1-on-1 message to each member in the current conversation (aka on the conversation's roster).
  - **Valid Scopes:** personal, group chat, team chat

  - **Personal Scope Interactions:**

   **MessageAllMembers command interaction:**
  ![personal-MessageAllMembersCommand ](Images/13.Message_all_Members_chat.png)

   - **Group Chat Scope Interactions:**

   **MessageAllMembers command interaction:**
   ![groupChat-MessageAllMembers-interaction ](Images/14.Message_all_Members_GC.png)

  - **Team Scope Interactions:**

   **MessageAllMembers command interaction:**
  ![team-MessageAllMembers-interactions ](Images/15.Message_all_Members_Team.png)

  4.**Read Receipt**
  **Check Read count**
  - **Result:** The bot will check the count of members who have read your message which sent through `MessageAllMembers`. User can also reset the count using `Reset read count` command
  - **Valid Scopes:** personal, group chat, team chat

   - **Group Chat Scope Interactions:**
   **Check Read count command interaction:**
  ![groupChat-CheckReadCount-interaction](Images/Check_Read_Count_Before.png)

   **Reset Read count command interaction:**
  ![groupChat-ResetReadCount-interaction](Images/Reset_Read_Count.png)

 5. **ImmersiveReader**
- You can use the immersive reader property of adaptive cards by using the speak property.
`immersivereader` command will send an adpative card in teams chat.
![immersive-reader-card](Images/34.Immersive_Reader_card_chat.png)

- A new screen will be open and the text will be read by default which is mentioned inside the speak property of adaptive card.
![immersive-reader-screen](Images/immersiveReaderScreen.png)

 6. **Message update events for user messages**
- You will recieve event updates for message edit/delete features.

- Message is edited
![message-edit](Images/36.Message_Edited.png)

- Message is soft-deleted
![message-soft-delete](Images/37.Message_Deleted.png)

- Message is restored
![message-undelete](Images/38.Message_Restored.png)

 7. **Format AI bot messages**
- Consists features such as citations, feedback buttons, and sensitivity label that enables better user engagement

- `AI label` - enables user to identify that the message was generated using AI.
![AI-label](Images/AI-label.png)

- `Citations` - enables user to refer to the source of the bot's message through in-text citations and the reference.
![Citations](Images/citations.png)

- `Feedback buttons` - enables user to provide positive or negative feedback based on their experience.
![Feedback-buttons](Images/Feedback-buttons.png)

![Feedback-buttons1](Images/Feedback-buttons1.png)

![Feedback-buttons2](Images/Feedback-buttons2.png)

![Feedback-buttons3](Images/Feedback-buttons3.png)

- `Sensitivity label` - enables user to understand the confidentiality of the bot's message.
![Sensitivity-label](Images/Sensitivity-label.png)

- `Send AI message` - Replies back with a bot message containing all formats: AI label, Citations, Feedback buttons, and Sensitivity label.
![sendtext](Images/sendtext.png)

You can select an option from the command list by typing ```@TeamsConversationBot``` into the compose message area and ```What can I do?``` text above the compose area.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Messages in bot conversations](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?tabs=dotnet)
- [Receive a read receipt](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?branch=pr-en-us-9184&tabs=dotnet1%2Capp-manifest-v112-or-later%2Cdotnet2%2Cdotnet3%2Cdotnet4%2Cdotnet5%2Cdotnet#receive-a-read-receipt)
- [Format AI bot messages](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/format-ai-bot-messages?branch=pr-en-us-10798&tabs=js)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-conversation-nodejs" />
