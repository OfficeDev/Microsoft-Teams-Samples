---
page_type: sample
description: This sample shows a feature where user can check sentiment for teams message by using messaging extension.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "08/07/2023 04:00:00 PM"
urlFragment: officedev-microsoft-teams-samples-msgext-sentiment-analysis-nodejs
---

# Sentiment Analysis with messaging extension

This sample shows a feature where user can check sentiment for teams message by using messaging extension.

## Included Features
* Bots
* Open AI

## Interaction with app

![Sentiment Analysis](Images/Sentiment_Analysis.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Sentiment Analysis ME:** [Manifest](/samples/)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  [NodeJS](https://nodejs.org/en/)
-  [ngrok](https://ngrok.com/) or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the 
   appropriate permissions to install an app.
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
1. In the `env/.env.local` or `env/.env.local.user` file, fill all the required values for below and other values will be generated automatically once you debug/start the app.

## Setup
1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

2. Setup for Bot
- In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
**NOTE:** When you create app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK
 - Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    ```
4. Setup for code
   - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
   - Update the `.env` configuration for the bot to use the `MicrosoftAppId`, `MicrosoftAppPassword`, `apiKey` and `BaseUrl`  with application base url. For e.g., your ngrok url. (Note the MicrosoftAppId is the AppId created in step 1 (Setup for Bot), the MicrosoftAppPassword is referred to as the "client secret" in step 1 (Setup for Bot) and you can always create a new client secret anytime.)

   - For `apiKey` you need to registered and login to this link(https://platform.openai.com/docs/quickstart/build-your-application) from here you will get an option to create Key.

- In the folder where repository is cloned navigate to `samples/msgext-sentiment-analysis/nodejs`
- Install modules

    ```bash
    npm install
    ```
5) Run your app

    ```bash
    npm start
    ```

6) Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` o replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{AAD_APP_CLIENT_ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{BOT_DOMAIN}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `AppPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/msgext-message-reminder/nodejs/index.js#L44) line and put your debugger for local debug.

## Running the sample

Install Sample to Teams
![Add Sample ](Images/1.Add_Sample.PNG)

Welcome Message then click on 3 dots navigate to ME sentiment analysis
![Welcome](Images/2.Welcome_SentimentAnalysis.PNG)

 Click Continue 
![Click Continue](Images/3.Click_Continue.PNG)

Its shows Sentiment(positive/negative/neutral) for selected message
![Sentiment Analysis Reuslt ](Images/4.Result.PNG)

## Further reading
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/overview)
- [Messaging Extension](https://learn.microsoft.com/microsoftteams/platform/messaging-extensions/how-to/action-commands/define-action-command)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/msgext-sentiment-analysis-nodejs" />