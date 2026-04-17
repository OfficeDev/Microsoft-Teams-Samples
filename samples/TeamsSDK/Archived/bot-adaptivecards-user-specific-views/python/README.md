---
page_type: sample
description: This sample app shows how to create user-specific adaptive card views for Teams, enabling personalized content and auto-updating across various chat contexts.
products:
- office-teams
- office
- office-365
languages:
- python
extensions:
 contentType: samples
 createdDate: "03-06-2025 13:38:25"
urlFragment: officedev-microsoft-teams-samples-bot-adaptivecards-user-specific-views-python

---

# Teams Adaptive Card Views

- **Interaction with bot**
  ![UserSpecificView](docs/UserSpecificView_all.gif)

This sample demonstrates how to create user-specific views in Adaptive Cards within Microsoft Teams, using features like Action.Execute and auto-refresh for dynamic updates. The app enables personalized content, allowing seamless interaction across personal, group, and team contexts.

Specifically, it uses the Universal Action `Action.Execute` with `refresh` property, which enables developers to build different views for users in a common chat thread. 

Developers can consume this action to build different experiences in Teams like:
1. User-specific content in shared contexts like Group chat and Teams Channels.
2. Auto-update information in a card for a user in personal context / all users when they view it in a shared context. Think of updating an order status when a user views the message or an incident status when users view it.
3. Sequential workflows where each workflow is an adaptive card with user-specific view and options to invoke next / prev card.

For more details, refer to our [documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/user-specific-views?tabs=mobile%2CC).

## How does it work?

Apps can define `refresh` property with details about the refresh event and optionally add a list of users for whom the card should automatically refresh. (Refer to the image below).

![AdaptiveCardRefreshSchema](docs/AdaptiveCardRefreshSchema.png)

For more details on Adaptive card schema, refer to our [documentation](https://adaptivecards.io/explorer/).
____

The sample implements the following cards:

1. `Me`: The Adaptive card is configured to automatically refresh for the sender only. The sender will notice that the card refreshes for them automatically when the bot posts it. (Refresh count will go from 0 to 1). Other users will not get automatic refreshes, but they will have an option to do a manual refresh.
2. `All Users`: The Adaptive card is configured to automatically refresh for all the users in the context (Chat / Channel). Note that this works when the total number of users is <=60. If the total number of users is greater than 60, users will have to manually refresh the card.

Both the cards have an option to `Update Base card`, this action updates the base card for all the users in the context. We remove the refresh property from the updated card and that stops further refresh invoke actions for all the users. You may decide to keep it to enable auto-refresh for all or list of users.

All the cards display the following information:
1. **Card Type**: `Me` or `All Users`
2. **Card Status**: `Base`, `Updated` and `Final`.
3. **Trigger**: `automatic` trigger or `manual` trigger.
4. **View**: `personal` (user specific view) or `shared view`.


You can extend the `Me` card to automatically refresh for a list of users by adding a list of user MRIs to `userIds` in Adaptive Card.

## User specific view - workflow

The following GIF captures `Automatic refresh`, `Manual refresh`, and `Update Base Card` actions in `Me` card.

![bot-conversations ](docs/UserSpecificView_Me.png)

The diagram above captures the sequence of events for `Me` card.

Workflow:
1. User A selects `Me` card type, and the Bot sends an Adaptive card which is configured to automatically refresh for User A
2. User A will initially have a Base card with refresh count 0 which will get refreshed automatically to count 1.
3. User B's Base card will not automatically refresh and the refresh count will remain 0. User B will have the option to manually refresh.

In the case of `All Users`, the refresh will automatically be invoked for all users (in this case for user B as well).

## Teams Conversation Bot
Bot Framework v4 Conversation Bot sample for Teams.

This bot has been created using [Bot Framework](https://dev.botframework.com). This sample shows
how to incorporate basic conversational flow into a Teams application. It also illustrates a few of the Teams specific calls you can make from your bot.

## Included Features
* Bots
* Universal Adaptive Cards

## Interaction with bot

#### Me Action
![bot-conversations ](docs/UserSpecificView_Me.gif)

#### AllUser Action
![bot-conversations ](docs/UserSpecificView_all.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams bot adaptivecards user specific views sample app:** [Manifest](/samples/bot-adaptivecards-user-specific-views/csharp/demo-manifest/bot-adaptivecards-user-specific-views.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [Python SDK](https://www.python.org/downloads/) min version 3.6
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution


## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. Remember to select `requirements.txt` as dependencies to install when creating the virtual environment.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3) Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  
  A) Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  B) On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
  C) Navigate to **API Permissions**, and make sure to add the follow permissions:
   Select Add a permission
      * Select Add a permission
      * Select Microsoft Graph -\> Delegated permissions.
      * `User.Read` (enabled by default)
      * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.


4) Create [Azure Bot resource resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Azure free account here](https://azure.microsoft.com/free/)

5) In a terminal, go to `samples\bot-adaptivecards-user-specific-views`

6) Activate your desired virtual environment

7) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

8) Update the `config.py` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

9) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `${{AAD_APP_CLIENT_ID}}` and `${{TEAMS_APP_ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

10) Run your bot with `python app.py`

**Basic Tests**
* Tag the bot and send any message. The bot should respond with an adaptive card which should have options to try out the different cards.
* `Me` card should automatically refresh for sender only.
* `All Users` card should automatically refresh for all users in the chat. (as long as total number of users are <= 60)
* `Manual refresh` action should update the user-specific view for the user.
* `Update Base Card` action should update the message for all the users. The user should not have any option to refresh the message after this.

**Implementation:**
* `BotActivityHandler`: Has the logic to handle incoming bot messages (Invokes and user messages).
* `CardFactory`: Has the logic to prepare different types of Adaptive Cards (using Template library).
* `assets\templates\*`: contains all the adaptive cards definitions.

## Running the sample

You can interact with this bot in Teams by sending it a message, or selecting a command from the command list. The bot will respond to the following strings.

1. **Show Welcome**

  - **Result:** The bot will send the welcome card for you to interact with
  - **Valid Scopes:** personal, group chat, team chat

  - **Personal Scope Interactions:**
  
  **Search Application:**
  
  ![Serach-App ](docs/1.searchapp.png)

  **Add Application:**
  
  ![personal-AddBot ](docs/2.add-application.png)

  **Me Flow**

 **Show Welcome command interaction:**
 
  ![personal-WelcomeCard-Interaction ](docs/3.me-welcome-card.png)
  
 **Show Action command interaction with Me:**
 
 ![personal-WelcomeCard-Me](docs/4.me-action-card.png)
 
 **Show Response Card :**
 
 ![personal-WelcomeCard-Response](docs/5.response-card.png)
 
  **On Manual Refresh Click:**
  
 ![personal-WelcomeCard-ManualRefresh](docs/6.manual-refresh.png)
 
 **On Update Base Card Click:**
 
 ![personal-WelcomeCard-UpdatebaseCardClick](docs/7.response-update.png)
 
 **Response Update Base Card Click:**
 
 ![personal-WelcomeCard-ResponseUpdatebaseCardClick](docs/8.update-base-card-click.png)
 
**Flow with all Users**

 **Show Action command interaction with All Users:**
 
 ![personal-WelcomeCard-alluser](docs/9.alluser-action.png)
 
 **Show Response Card :**
 
 ![personal-WelcomeCard-allResponse](docs/10.alluser-response-card.png)
 
  **On Manual Refresh Click:**
  
 ![personal-WelcomeCard-allManualRefresh](docs/11.all-user-manual.png)
 
 **On Update Base Card Click:**
 
 ![personal-WelcomeCard-allUpdatebaseCardClick](docs/12.all-user-update.png)
 
 
  - **Team Scope Interactions:**
  
  **About UI**
  
  ![Team-scope-app](docs/13.add-to-team.png)
 
  **Team Selection**
  
  ![Team-scope-app](docs/14.team-selection.png)
  
  **Selected Team**
  
  ![Team-scope-app](docs/15.selected-team.png)
  
  **Welcome intraction card**
  
  ![Team-scope-app](docs/16.prompt.png)
  
  
  - **Chat Scope Interactions:**
  
  **About UI**
  
  ![Chat-scope-app](docs/17.welcome-interaction.png)
  
  **Chat Selection**
  
  ![Chat-scope-app](docs/18.add-to-chat.png)
  
  **Selected Chat**
  
  ![Chat-scope-app](docs/19.select-chat.png)
  
  **Welcome intraction card**
  
  ![Chat-scope-app](docs/20.welcome-card-at-chat.png)
  
  
  - **Meeting Scope Interactions:**
  
  **About UI**

  ![Meeting-scope-app](docs/21.add-to-meeting.png)
  
  **Meeting Selection**
  
  ![Meeting-scope-app](docs/22.select-meeting.png)
  
  **Selected Meeting**
  
  ![Meeting-scope-app](docs/23.call-to-card.png)
  
  **Welcome intraction card**
  
  ![Meeting-scope-app](docs/24.welcome-card.png)

  **Copilot Custom Engine Agents**

  Install App in copilot
  ![Copilot](docs/CopilotInstall.png) 

  **Welcome Card**
  ![Copilot](docs/Copilot1.png) 

  **User Specific Card**
  ![Copilot](docs/Copilot2.png) 

  **On Click Manual Refresh**
  ![Copilot](docs/Copilot3.png) 

  **On Click Update Message**
  ![Copilot](docs/Copilot4.png) 

  ![Copilot](docs/Copilot5.png) 

  **On Click All Users**
  ![Copilot](docs/Copilot6.png) 

  **On Click Manual Refresh**
  ![Copilot](docs/Copilot7.png) 

  **On Click Update Message**
  ![Copilot](docs/Copilot8.png)
  
## FAQ

##### How to implement user-specific views in a group of >60 users?
If your scenario requires a user-specific view for all the users in a chat, we recommend you do the following:

1. Add a `manual refresh` action in the base card (like the sample app) so that users know they need to refresh it to see relevant content.
2. Leave the `userids` field empty. If the total number of users is <=60, refresh invoke will be triggered automatically for all the users, else all the users will see the base card and they can refresh it manually.

> Note: you can configure up to 60 users for whom auto-refresh should be triggered. (if your scenario allows you to prioritize certain user role types). Others will see the base card and can refresh it manually.

##### How frequently do Teams clients trigger auto-refresh for users?
Assuming the AC contains refresh logic that should auto-refresh for the user - 
Teams clients will trigger a refresh when a user views the message and the last refresh response is older than a minute.

>Note: Developers can control if they want to continuously refresh the content for a user or not.

If the developers do not want to continuously refresh a card for a user, they should remove the `refresh` property from the updated user-specific card response. (Refer sequence diagram - Response with updated AC for the user)

##### Is the user-specific view response for a user immediately available on all the Teams clients (Web, Desktop, and mobile)?
Teams caches the refresh invoke a response in clients. Every client will trigger a refresh invoke when the user views the message.

Consider following:
1. Bot sends an AC with user-specific views for all users in a chat.
2. User A logins to Teams desktop application and opens the chat.
3. User A will see the base card and the Teams client will trigger auto-refresh and display the updated refresh card received from the bot.
4. When the same user A logins to Teams mobile/web application, and opens the chat, he/she will see the base card, and Teams client will trigger an auto-refresh to get the updated card from the bot.
If User A opens the chat again on either of these clients, it will show the cached card (updated refresh card).

## Further reading
[User Specific Views](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/user-specific-views?tabs=mobile%2CC)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-adaptivecards-user-specific-views-python" />