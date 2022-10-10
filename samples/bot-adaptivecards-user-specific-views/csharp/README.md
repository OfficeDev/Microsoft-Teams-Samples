---
page_type: sample
description: This sample illustrates a few different ways developers can consume user-specific views in Adaptive cards.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
  contentType: samples
  createdDate: "03/09/2022 11:00:00 AM"
urlFragment: officedev-microsoft-teams-samples-bot-adaptivecards-user-specific-views-csharp
---

# User Specific Views in Adaptive Cards

#### About
This sample illustrates a few different ways developers can consume user-specific views in Adaptive cards.

Specifically, it uses the Universal Action `Action.Execute` with `refresh` property, which enables developers to build different views for users in a common chat thread. 

Developers can consume this action to build different experiences in Teams like:
1. User-specific content in shared contexts like Group chat and Teams Channels.
2. Auto-update information in a card for a user in personal context / all users when they view it in a shared context. Think of updating an order status when a user views the message or an incident status when users view it.
3. Sequential workflows where each workflow is an adaptive card with user-specific view and options to invoke next / prev card.

For more details, refer to our [documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/user-specific-views?tabs=mobile%2CC).

##### How does it work?

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

###### User specific view - workflow

The following GIF captures `Automatic refresh`, `Manual refresh`, and `Update Base Card` actions in `Me` card.

![OnlyMe](docs/Me.gif)

![UML](docs/UserSpecificView_Me.png)

The diagram above captures the sequence of events for `Me` card.

Workflow:
1. User A selects `Me` card type, and the Bot sends an Adaptive card which is configured to automatically refresh for User A
2. User A will initially have a Base card with refresh count 0 which will get refreshed automatically to count 1.
3. User B's Base card will not automatically refresh and the refresh count will remain 0. User B will have the option to manually refresh.

In the case of `All Users`, the refresh will automatically be invoked for all users (in this case for user B as well).

#### Prerequisites
* Make sure you have an active [Azure subscription](https://azure.microsoft.com/en-us/free/).
* Install [Visual Studio](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2022) or [Visual Studio Code](https://code.visualstudio.com/download) to run and debug the sample code.
  * [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0
* Install [ngrok](https://ngrok.com/download) for local setup. (or any other tunneling solution). 
>Note: You may need a paid version of ngrok to be able to run the setup locally.

#### Setup

##### Bot setup
* [Register a bot with Azure](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-4.0&tabs=userassigned).
  * Make sure you copy and save the Azure Bot resource App ID and password.
* [Connect the bot to Microsoft Teams](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0).
* Run ngrok and point it to port: 3978.
    ```
    ngrok http -host-header=rewrite 3978 
    ```
    * Make sure you copy and save the `HTTPS` url (it should look like `https://<randomsubdomain>.ngrok.io`).

* Update Bot messaging endpoint to ngrok URL with the messaging endpoint. (ex. `https://<randomsubdomain>.ngrok.io/api/messages`

##### Project setup
* Clone the repo or download the sample code to your machine.
* Update the following settings in `appsettings.json`
  * `MicrosoftAppId` - App ID saved earlier.
  * `MicrosoftAppPassword` - App secret saved earlier.
* Build and run the sample code in Visual studio / Visual studio code.

##### Add AppPackage to Teams
* Update the following in the manifest.json.
  * `{AppId}` - Replace with app ID saved earlier.
* Zip the manifest and app icons. (say `sample.zip`)
* Side load the application or add the application to application catalog to try it.

#### Basic Tests
* Tag the bot and send any message. The bot should respond with an adaptive card which should have options to try out the different cards.
* `Me` card should automatically refresh for sender only.
* `All Users` card should automatically refresh for all users in the chat. (as long as total number of users are <= 60)
* `Manual refresh` action should update the user-specific view for the user.
* `Update Base Card` action should update the message for all the users. The user should not have any option to refresh the message after this.

#### Implementation:
* `BotActivityHandler`: Has the logic to handle incoming bot messages (Invokes and user messages).
* `CardFactory`: Has the logic to prepare different types of Adaptive Cards (using Template library).
* `assets\templates\*`: contains all the adaptive cards definitions.

#### FAQ

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
