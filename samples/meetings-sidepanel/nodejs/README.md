---
page_type: sample
description: Microsoft Teams meeting extensibility sample for iteracting with Side Panel in-meeting
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "07-07-2021 13:38:27"
urlFragment: officedev-microsoft-teams-samples-meetings-sidepanel-nodejs
---

# Meetings SidePanel

This sample illustrates how to implement [Side Panel](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/create-apps-for-teams-meetings?view=msteams-client-js-latest&tabs=dotnet#notificationsignal-api) In-Meeting Experience.

## Included Features
* Meeting Stage
* Meeting SidePanel
* Live Share SDK
* Adaptive Cards
* RSC Permissions

## Interaction with app

![Preview Image](Images/Preview.gif)

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version
    ```
    
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup
- Register an AAD app in Azure portal and also register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. Run ngrok - point to port 3001 (pointing to ClientApp)

    ```bash
    # ngrok http 3001 --host-header="localhost:3001"
    ```

2. Clone the repository
      ```bash
      git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
      ```

3. Open .env file from this path folder (samples/meetings-sidepanel/nodejs/server) and update ```MicrosoftAppId```,  ```MicrosoftAppPassword``` information with values generated values while doing AAD App Registration.
- Update ```BaseURL``` with your application domain URL like ngrok URL: https://xxxx.ngrok-free.app

4. Install node modules

   Inside node js folder,  navigate to `samples/meetings-sidepanel/nodejs/server` open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

   - Repeat the same step in folder `samples/meetings-sidepanel/nodejs/ClientApp`

    ```bash
    npm install
    ```

5. We have two different solutions to run, so follow below steps:
 
- In a terminal, navigate to `samples/meetings-sidepanel/nodejs/server` folder, Open your local terminal and run the below command to install node modules. You can do the same in Visual studio code terminal by opening the project in Visual studio code
```bash
npm install
```

```bash
npm start
```

If you face any dependency error while installing node modules, try using below command

```bash
npm install --legacy-peer-deps
```

- In a different terminal, navigate to `samples/meetings-sidepanel/nodejs/ClientApp` folder, Open your local terminal and run the below command to install node modules. You can do the same in Visual studio code terminal by opening the project in Visual studio code 
```bash
cd client
npm install
```

```bash
npm start
```

If you face any dependency error while installing node modules, try using below command

```bash
npm install --legacy-peer-deps
```

6. Run your app, either from Visual Studio code  with ``` npm start``` or using ``` Run``` in the Terminal.

7) Setup Manifest for Teams (__*This step is specific to Teams.*__)
    - **Edit** the `manifest.json` contained in the `teamsAppManifest` folder and replace your Microsoft App Id (that was created when you registered your app earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `configurationUrl` inside `configurableTabs` . Replace `{{BASE-URL}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Update** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Update** the `manifest.json` for `<<Manifest-id>>` with any GUID or with your MicrosoftAppId, generated during App registration in Azure portal.

    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)

**Note**: If you are facing any issue in your app, [please uncomment this line](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/meetings-sidepanel/nodejs/server/index.js#L48) and put your debugger for local debug.

## Running the sample

**Interacting with the app in Teams Meeting**

Interact with SidePanel by clicking on the App icon present on the top menu beside the "more actions" during a meeting.

1. Once the app is clicked, sidepanel appears with the default agenda list. Only organizer gets the feasibility to add new agenda points to the list using "Add New Agenda Item" button.

![Dashboard](Images/Dashboard.png)

2. On click of "Add" button, agenda point will be added to the agenda list by organizer.

![AddNewAgendaItem](Images/AddNewAgendaItem.png)

![AddedNew](Images/AddedNew.png)

3. On click of "Publish Agenda", the agenda list will be sent to the meeting chat.

![Notification](Images/Notification.png)

**User interactions(Meeting Organizer)**
- **Add New Agenda Item** - Gives provision to add new Agenda point.
- **Add** - Adds the agenda from Textinput to the SidePanel agenda list.
- **Publish Agenda** - Sends the agenda list to the meeting chat.

## Deploy the bot to Azure

-  To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading.

- [Meeting apps APIs](https://learn.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet)
- [Meeting Side Panel](https://learn.microsoft.com/en-us/microsoftteams/platform/sbs-meetings-sidepanel?tabs=vs)
- [Build tabs for meeting](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/build-tabs-for-meeting?tabs=desktop)
- [Install the App in Teams Meeting](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meetings-sidepanel-nodejs" />