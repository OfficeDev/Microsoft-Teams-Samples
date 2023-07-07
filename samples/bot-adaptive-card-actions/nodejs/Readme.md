---
page_type: sample
description: Sample which demonstrates different Adaptive Card action types using bot.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "12/27/2022 12:30:17 PM"
urlFragment: officedev-microsoft-teams-samples-bot-adaptivecard-actions-nodejs
---
# Send Adaptive Card Including Different Actions - NodeJS

This sample shows the feature where user can send adaptive card with different actions using bot.

## Interaction with app

![Bot Adaptive ActionsGif](Images/AdaptiveCardActions.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams bot adaptivecard actions sample app:** [Manifest](/samples/bot-adaptive-card-actions/csharp/demo-manifest/bot-adaptivecard-actions.zip)

## Included Features
* Bots
* Adaptive Cards

## Prerequisites

-  Microsoft Teams is installed and you have an account (not a guest account)
-  To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 18.12.1)
-  [ngrok](https://ngrok.com/) or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the 
   appropriate permissions to install an app.

## Setup

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
      > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

2. Setup for Bot:
  - Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/abs-quickstart?view=azure-bot-service-4.0&tabs=userassigned).
  - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
  - While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

3. Setup NGROK  
Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    ```

4. Setup For Code  
  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

  - Update the `.env` configuration for the bot to use the `MicrosoftAppId` and `MicrosoftAppPassword`. 
(**Note:** The MicrosoftAppId is the AppId created in step 1 (Setup AAD app registration in your Azure portal), the MicrosoftAppPassword is referred to as the "client secret" in step 1 (Setup for Bot) and you can always create a new client secret anytime.)

  - In a terminal, navigate to `samples/bot-adaptive-card-actions/nodejs`

  - Install modules

    ```bash
    npm install
    ```

  - Run your app

    ```bash
    npm start
    ```

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `<<Microsoft-AppId>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `<<Domain-Name>>` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-adaptive-card-actions/nodejs/index.js#L44) line and put your debugger for local debug.
 

## Running the sample

**Install App:**

![InstallApp](Images/1.AddApp.png)

**Welcome UI:**

![Initial message](Images/2.Welcome.png)

![Running Sample](Images/3.AdaptiveCard_Actions.png)

![Running Sample](Images/4.ActionSubmit.png)

![Running Sample](Images/5.ActionShowCard.png)

![Running Sample](Images/5.ActionSubmitted.png)

![Running Sample](Images/6.Togglevisible.png)

![Running Sample](Images/7.ToggleVisibleOnClick.png)

![Running Sample](Images/8.SuggestedActions.png)

![Running Sample](Images/9.RedColor.png)

![Running Sample](Images/10.BlueColor.png)

![Running Sample](Images/11.YellowColor.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Actions](https://learn.microsoft.com/adaptive-cards/rendering-cards/actions)
- [Send suggested actions](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?tabs=dotnet#send-suggested-actions)


  <img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-adaptive-card-actions-nodejs" />
