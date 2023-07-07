---
page_type: sample
description: This sample app shows how to send app icon badging in Teams meetings using targeted notifications API.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "23/05/2023 13:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-meetings-app-icon-badging-nodejs
---

## App Icon Badging In Teams Meeting Notification

This sample illustrates how to send app icon badging notification on tab icon in Teams meetings.

## Included Features
* Bots
* In-Meeting Notifications
* RSC Permissions
* App Icon Badging

## Interaction with app

![App Icon Badging](Images/app_icon_badging.gif)

## Prerequisites

- To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 16.14.2  or higher).

    ```bash
    # determine node version
    node --version
    ```
- Office 365 tenant. You can get a free tenant for development use by signing up for the [Office 365 Developer Program](https://developer.microsoft.com/microsoft-365/dev-program).

- To test locally, you'll need [Ngrok](https://ngrok.com/) installed on your development machine.
Make sure you've downloaded and installed Ngrok on your local machine. Ngrok will tunnel requests from the Internet to your local computer and terminate the SSL connection from Teams.

## Setup

> NOTE: The free ngrok plan will generate a new URL every time you run it, which requires you to update your Azure AD registration, the Teams app manifest, and the project configuration. A paid account with a permanent ngrok URL is recommended.

1) Setup for Bot
- Register Azure AD application resource in Azure portal
- In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    
    > NOTE: When you create your app registration in Azure portal, you will create an App ID and App password - make sure you keep these for later.

2) Setup NGROK  
    - Run ngrok - point to port `3978`

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    ```

3) Setup for code   
- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- In a terminal, navigate to `samples\meetings-app-icon-badging\nodejs`

    ```bash
    cd samples/meetings-app-icon-badging/nodejs
    ```

- Install modules

    ```bash
    npm install
    ```

- Go to .env file in your project folder and update `MicrosoftAppId`, `MicrosoftAppPassword` with the values received from your AAD app registration.
  - Update `BaseUrl` as per your domain like ngrok url: https://1234.ngrok-free.app 

- Start the bot

    ```bash
    npm start
    ```
 
4) Setup Manifest for Teams

- Modify the `manifest.json` file placed in `/appPackage` folder and replace the <<YOUR-MICROSOFT-APP-ID>> with your Microsoft App Id received via doing AAD app registration in your Azure Portal.
    - **Edit** the `manifest.json` for `validDomains` and replace <<Valid-Domain>> with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip`
    - - **Upload** the `manifest.zip` to Teams
         - Select **Apps** from the left panel.
         - Then select **Upload a custom app** from the lower right corner.
         - Then select the `manifest.zip` file from `appPackage`.
         - [Install the App in Teams Meeting](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/meetings-app-icon-badging/nodejs/index.js#L45) line and put your debugger for local debug.

## Running the sample

**Setup Configurable Tab:**

![Setup Tab](Images/1.set_up_config_tab.png)

**Interacting with the app in Teams Meeting**

Type `SendNotification` in bot chat to send In-Meeting notifications.

**Notification On Tab Icon as 'Icon Badging':**

![App Icon Badging](Images/2.app_icon_badging.png)

**Stage View and App Icon Badging Notification:**

![Stage View and AppIconBadging](Images/3.StageView_and_AppIconBadging.png)


## Further reading

- [Enable app icon badging for your Teams app](https://review.learn.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/app-icon-badging-for-your-app?branch=pr-en-us-8495)

- [Grant RSC permissions to your app](https://learn.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/grant-resource-specific-consent#install-your-app-in-a-team-or-chat)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meetings-app-icon-badging-nodejs" />
