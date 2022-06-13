---
page_type: sample
description: Demonstrating on how to implement sequential flow, user specific view and upto date adaptive cards in bot.
products:
- office-teams
- office
- office-365
languages:
- nodejs
- javascript
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:26"
---

# Sequential workflow adaptive cards

This App talks about the Teams Bot User Specific Views and Sequential Workflows in adaptive card with Node JS

This bot has been created using [Bot Framework v4](https://dev.botframework.com), it shows how to create a simple bot that accepts food order using Adaptive Cards V1.4

This is a sample app that provides an experience of managing incidents. This sample makes use of Teams platform capabilities like Universal Bots with below mentioned capabilities.
[User Specific Views](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/user-specific-views)
[Sequential Workflows](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/sequential-workflows)
[Up to date cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/up-to-date-views)

## Key features

- Incident Creation
   - Choose Category
   - Choose Sub Category
   - Create Incident
   - Edit/ Approve/ Reject Incident
- List Incidents

## Prerequisites

1. Office 365 tenant. You can get a free tenant for development use by signing up for the [Office 365 Developer Program](https://developer.microsoft.com/en-us/microsoft-365/dev-program).

2. To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 10.14 or higher).

    ```bash
    # determine node version
    node --version
    ```

3. To test locally, you'll need [Ngrok](https://ngrok.com/) installed on your development machine.
Make sure you've downloaded and installed Ngrok on your local machine. ngrok will tunnel requests from the Internet to your local computer and terminate the SSL connection from Teams.

> NOTE: The free ngrok plan will generate a new URL every time you run it, which requires you to update your Azure AD registration, the Teams app manifest, and the project configuration. A paid account with a permanent ngrok URL is recommended.

## To try this sample

- Register Azure AD applications
    -   Register your bot using bot channel registration in Azure AD portal, following the instructions [here](Wiki/azure-bot-channels-registration.md).
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- In a console, navigate to `samples/bot-sequential-flow-adaptive-cards/nodejs`

    ```bash
    cd samples/bot-sequential-flow-adaptive-cards/nodejs
    ```

- Run ngrok - point to port `3978`

    ```bash
    ngrok http -host-header=localhost 3978
    ```


- Update the `.env` configuration for the bot to use the `MicrosoftAppId` (Microsoft App Id) and `MicrosoftAppPassword` (App Password) from the Bot Framework registration. 
> NOTE: the App Password is referred to as the `client secret` in the azure portal and you can always create a new client secret anytime.

- Install modules & Run the `NodeJS` Server 
    - Server will run on PORT:  `3978`
    - Open a terminal and navigate to project root directory
    
    ```bash
    npm run server
    ```
    
    > **This command is equivalent to:**
    _npm install  > npm start_

- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`) also update the `<<DOMAIN-NAME>>` with the ngrok URL
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

## Workflows
### Workflow for bot interaction

```mermaid
sequenceDiagram
    participant Teams User B    
    participant Teams User A
    participant Teams Client
    Teams User A->>+Teams Client: Enters create incident bot commands
    Sample App->>+Teams Client: loads card with option 
    Teams User A->>+Teams Client: Enters required details and assigns to user B
    Sample App-->>Teams Client: Posts the incidet card with auto-refresh for user A and user B
    Teams Client->>Teams User A: loads incident card with loading indicator 
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User A: Responds with Updated AC for the user A
    Teams User B->>Teams Client: User opens the chat
    Teams Client-->>Teams User B: Loads the incident base card
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User B: Responds with card for user B with option to approve/reject
```
### Workflow for messaging extension interaction

```mermaid
sequenceDiagram
    participant Teams User B    
    participant Teams User A
    participant Teams Client
    Teams User A->>+Teams Client: Clicks on Incidents ME action in a group chat
    opt App not installed flow
        Teams Client-->>Teams User A: App install dialog
        Teams User A->>Teams Client: Installs app
    end   
    Teams Client->>+Sample App: Launches Task Module
    Sample App-->>-Teams Client: Loads existing incidents created using Bot
    Teams User A->>Teams Client: Selects incident to share in chat
    Teams Client->>Sample App: Invoke action callback composeExtension/submitAction
    Sample App-->>Teams Client: Posts Base card with auto-refresh for user A and user B
    Teams Client->>Teams User A: loads incident card with loading indicator 
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User A: Responds with Updated AC for the user A
    Teams User B->>Teams Client: User opens the chat
    Teams Client-->>Teams User B: Loads the incident base card
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User B: Responds with card for user B with option to approve/reject
```

## Interacting with the bot in Teams

You can interact with this bot by `@Sequential Workflows` (BotName). The bot will respond with adaptive card requesting you the details.

- Install App

Navigate to `Manage apps` > `Upload a custom app` (Bottom-Right of the screen) > Upload `manifest.zip` > `Add`
![image](https://user-images.githubusercontent.com/85108465/123583709-b6e39d00-d7fd-11eb-83bc-737a17fbbadd.png)

![image](https://user-images.githubusercontent.com/85108465/123583855-f3af9400-d7fd-11eb-87df-a69d880680aa.png)

- Open The App

Type in Chat: `@Sequential Workflows` (BotName) and Enter

![image](https://user-images.githubusercontent.com/85108465/123767041-d0abdf80-d8e4-11eb-8cb3-3fa3eb0680ce.png)

Create New Incident

![image](https://user-images.githubusercontent.com/85108465/123586591-936f2100-d802-11eb-9d38-a43fc13672ee.png)
![image](https://user-images.githubusercontent.com/85108465/123586786-e34de800-d802-11eb-9355-ea12ebc67388.png)
![image](https://user-images.githubusercontent.com/85108465/123586874-06789780-d803-11eb-843b-76e69b9afad6.png)
![image](https://user-images.githubusercontent.com/85108465/123591452-9d485280-d809-11eb-83cd-412f4e6aaf5a.png)

> Only the `Created By` person have the option to `Edit`

![image](https://user-images.githubusercontent.com/85108465/123591565-c668e300-d809-11eb-829c-23a6396e0cfe.png)

Edit Incident

![image](https://user-images.githubusercontent.com/85108465/123591600-d385d200-d809-11eb-9edd-23f76a8687d8.png)

`Approve` or `Reject` Incidents

> Only the `Assigned To` person have the option to `Approve` or `Reject`

![image](https://user-images.githubusercontent.com/85108465/123768720-351b6e80-d8e6-11eb-9f6e-7525c761d034.png)
![image](https://user-images.githubusercontent.com/85108465/123768103-a9a1dd80-d8e5-11eb-8ec5-154eb36a8d62.png)
![image](https://user-images.githubusercontent.com/85108465/123768181-baeaea00-d8e5-11eb-9f79-4854c409edd3.png)

List Incidents

![image](https://user-images.githubusercontent.com/85108465/123595684-d0d9ab80-d80e-11eb-9798-82f535ba6486.png)
![image](https://user-images.githubusercontent.com/85108465/123769386-e28e8200-d8e6-11eb-96e0-3d1f8365c7ef.png)


## Interaction from messaging extension.

You can also interact with this app using messaging extension action which allows you to share incidents in group chats.

1. On selecting app from messaging extension,it checks whether bot is installed in chat/team. If not installed, user will get a option for justInTimeInstallation card.

   ![just in time installation card](Images/justInTimeInstallation.png)

2. After successful installation, list of all incident will be available in messaging extension.

   ![incident list card](Images/incidentListCard.png).
   
3. User can select any incident from the list and can share to that chat/team.

   ![image](https://user-images.githubusercontent.com/85108465/123768720-351b6e80-d8e6-11eb-9f6e-7525c761d034.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [User Specific Views](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/user-specific-views)
- [Sequential Workflows](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/sequential-workflows)
- [Up to date cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/up-to-date-views)
- [Universal Bot Action Model](https://docs.microsoft.com/en-us/adaptive-cards/authoring-cards/universal-action-model#actionexecute)
- [Azure Portal](https://portal.azure.com)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [dotenv](https://www.npmjs.com/package/dotenv)
- [Microsoft Teams Developer Platform](https://docs.microsoft.com/en-us/microsoftteams/platform/)

