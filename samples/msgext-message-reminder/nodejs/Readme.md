---
page_type: sample
description: This sample shows a feature where user can schedule a task from messaging extension action and get a reminder card at a scheduled time.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "24-11-2021 13:10:00"
urlFragment: officedev-microsoft-teams-samples-msgext-message-reminder-nodejs
---

# Message reminder with messaging extension action

This sample shows a feature where user can schedule a task from messaging extension action and get a reminder card at a scheduled time.

- Personal scope scenario
 Select `...` over message to get action `create-reminder` for scheduling task.

![Select message](Images/MessageAction.png)

 Task module to schedule a task.

![Task Details](Images/ScheduleTask.png)

 Reminder card of task at scheduled date and time.
 
![Task reminder](Images/TaskReminder.png)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  [NodeJS](https://nodejs.org/en/)
-  [ngrok](https://ngrok.com/) or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the 
   appropriate permissions to install an app.

## To try this sample

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call into the bot.

### 1. Setup for Bot
In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

### 2. Run your bot sample
1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

   - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

2) In the folder where repository is cloned navigate to `samples/bot-task-reminder/nodejs`

3) Install node modules

   Inside node js folder, open your local terminal and run the below command to install node modules. You can do the same in Visual studio code terminal by opening the project in Visual studio code 

    ```bash
    npm install
    ```
4) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```
5) Update the `.env` configuration file in your project folder for the bot to use the `MicrosoftAppId` and `MicrosoftAppPassword`, `BaseUrl` with application base url. For e.g., your ngrok url. (Note the MicrosoftAppId is the AppId created in step 1 (Setup for Bot), the MicrosoftAppPassword is referred to as the "client secret" in step 1 (Setup for Bot) and you can always create a new client secret anytime.)

6) Run your app

    ```bash
    npm start
    ```
7) Manually update the manifest.json
    - Edit the `manifest.json` contained in the  `appPackage/` folder to replace with your MicrosoftAppId (that was created in step1.1 and is the same value of MicrosoftAppId in `.env` file) *everywhere* you see the place holder string `{MicrosoftAppId}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - Zip up the contents of the `appPackage/` folder to create a `manifest.zip`
    - Upload the `manifest.zip` to Teams (in the left-bottom *Apps* view, click "Upload a custom app")

## Features of this sample

- Personal scope scenario
 Select `...` over message to get action `create-reminder` for scheduling task.

![Select message](Images/MessageAction.png)

 Task module to schedule a task.

![Task Details](Images/ScheduleTask.png)

 Reminder card of task at scheduled date and time.

![Task reminder](Images/TaskReminder.png)

- Team scope scenario
 Navigate to team where the app is installed

 Select `...` over message to get action `create-reminder` for scheduling task.

![Team message action ](Images/MessageActionTeam.png)

 Task module to schedule a task.

![Team Task Details ](Images/ScheduleTaskTeam.png)

 Reminder card of task at scheduled date and time.

![ TeamTask reminder](Images/TaskReminderTeam.png)
