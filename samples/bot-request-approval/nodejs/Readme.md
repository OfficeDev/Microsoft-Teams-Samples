---
page_type: sample
description: This sample shows a feature where user can send task request to his manager and manager can approve/reject the request in group chat.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
contentType: samples
createdDate: "6-12-2021 17:00:25"
---

# Send task request using universal adaptive cards in group chat

This sample shows a feature where:
1. Requester : Can request for any task approval from manager by intiating request in groupchat using bot command `request` and only requester can edit the request card.
2. Manager : Can see the request raised by user in the same groupchat with an option of approve or reject.
3. Others: Other members in the group chat can see the request details only.

Requester:

- Initiated request using bot command `request` in group chat.

  ![Initial Card](Images/InitialCard.png)

- Card will refresh for requester to fill details.

  ![Request Card](Images/CreateTask.png)
  
- After submitting the request, requester can edit or cancel the request.
- Note: Only user who created the card will see the buttons to edit and cancel the request.

  ![Edit/Cancel Card](Images/UserCard.png)

Manager:

- After requester submit the request, manager can approve/reject the request.
- Note: Only the manager of the request will see the buttons to approve/reject the request.

  ![Approve/Reject Card](Images/ManagerCard.png)

- If manager approves or rejects the request card will be refreshed for all the members in groupchat.

  ![Status Card](Images/ApprovedRequest.png)
  
Others:

- Other members will only able to see details of the created request.

  ![Members Card](Images/OtherMembers.png)

## Prerequisites

- [NodeJS](https://nodejs.org/en/)
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

### 1. Setup for Bot
In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

### 2. Run your bot sample
1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) In the folder where repository is cloned navigate to `samples/bot-task-approval/nodejs`

3) Install node modules

   Inside node js folder, open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

    ```bash
    npm install
    ```
4) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```
5) Open the `.env` configuration file in your project folder (or in Visual Studio Code) and update the `ClientId` and `ClientSecret`, `BaseURL` with your app's base url. (Note the ClientId is the AppId created in step 1 (Setup for Bot), the ClientSecret is referred to as the "client secret" in step 1 (Setup for Bot) and you can always create a new client secret anytime.)

6) Run your app

    ```bash
    npm start
    ```
7) Manually update the manifest.json
    - Edit the `manifest.json` contained in the  `appPackage/` folder to replace with your MicrosoftAppId (that was created in step1.1 and is the same value of MicrosoftAppId in `.env` file) *everywhere* you see the place holder string `{MicrosoftAppId}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - Zip up the contents of the `appPackage/` folder to create a `manifest.zip`
    - Upload the `manifest.zip` to Teams (in the left-bottom *Apps* view, click "Upload a custom app")

**Note:** App should be installed for user's manager also to get task approval notification.