---
page_type: sample
description: Microsoft Teams tab sample app for demonstrating tab in stage view
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "06/10/2021 01:48:56 AM"
urlFragment: officedev-microsoft-teams-samples-tab-stage-view-csharp
---

# Stage View

This App talks about the Teams tab in stage view with CSharp.
For reference please check [Tabs link unfurling and Stage View](https://docs.microsoft.com/microsoftteams/platform/tabs/tabs-link-unfurling)

## Included Features
* Bots
* Stage View (tabs)

## Interaction with app

![TabStageView Modules](TabInStageView/Images/tabstageviewmodule.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Stage View:** [Manifest](/samples/tab-stage-view/csharp/demo-manifest/tab-stage-view.zip)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

2. Setup for Bot
 - Register a AAD aap registration in Azure portal.
 - Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
 - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
 - While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

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

- Modify the `/appsettings.json` and fill in the following details:
  - `{{Bot Id}}` - Generated from Step 1 while doing AAd app registration in Azure portal.
  - `{{ Bot Password}}` - Generated from Step 1, also referred to as Client secret
  - `{{ Application Base URL }}` - Your application's base url. E.g. https://12345.ngrok-free.app if you are using ngrok.
 
- In a terminal, navigate to `TabInStageView`

    ```bash
    # change into project folder
    cd # TabInStageView
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `TabInStageView` folder
  - Select `TabInStageView.csproj` file
  - Press `F5` to run the project
  
5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./Manifest folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id or Bot-id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `TabInStageView\Manifest_Hub` folder with the required values.
    - **Zip** up the contents of the `Manifest` folder to create a `Manifest.zip` or `Manifest_Hub` folder to create a `Manifest_Hub.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./Manifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/tab-stage-view/csharp/TabInStageView/AdapterWithErrorHandler.cs#L26) line and put your debugger for local debug.

## Interacting with the bot in Teams
    You can use this tab by following the below steps:
    - In the navigation bar located at the far left in Teams, select the ellipses ●●● and choose your app from the list.

## Running the sample

- Opening stage view in tab with Adaptive card action.

![Card Actions](TabInStageView/Images/cardActions.png)

- Opening Collaborative Stage View. Please refer [Collaborative Stage view](https://review.learn.microsoft.com/en-us/microsoftteams/platform/tabs/tabs-link-unfurling?branch=pr-en-us-7891#collaborative-stage-view) for more details

![Stage View](TabInStageView/Images/CollaborativeStageView.png)

- Opening stage view from Adaptive card via deep link.

![Stage View Deep Link](TabInStageView/Images/viaDeeplink.png)

- Opening the stage view from unfurling link. If you copy and paste a link from https://www.BotFramework.com into the compose message area the link will unfurl.

![Link Unfurling Text](TabInStageView/Images/LinkUnfurlingText.png)

![Link Unfurling Card](TabInStageView/Images/LinkUnfurlingCard.png)

![Link Unfurling StageView](TabInStageView/Images/LinkUnfurlingStageView.png)

- Tab with execute deep link action to open stage view.

![Stage View in tab](TabInStageView/Images/viaTabDeeplink.png)

## Outlook on the web

- To view your app in Outlook on the web.

- Go to [Outlook on the web](https://outlook.office.com/mail/)and sign in using your dev tenant account.

**On the side bar, select More Apps. Your sideloaded app title appears among your installed apps**

![InstallOutlook](TabInStageView/Images/InstallOutlook.png)

**Select your app icon to launch and preview your app running in Outlook on the web**

![AppOutlook](TabInStageView/Images/AppOutlook.png)

**After opening Outlook web, click the "New mail" button.**

![Open New Mail](Images/OpenNewMail.png)

**On the tool bar on top, select Apps icon. Your sideloaded app title appears among your installed apps**

![OpenAppIcon](Images/OpenAppIcon.png)

**Opening the stage view from unfurling link. If you copy and paste a link from https://www.BotFramework.com into the compose message area the link will unfurl.**

![Outlook Unfurling](Images/OutlookUnfurling.png)

**Note:** Similarly, you can test your application in the Outlook desktop app as well.

## Office on the web

- To preview your app running in Office on the web.

- Log into office.com with test tenant credentials

**Select the Apps icon on the side bar. Your sideloaded app title appears among your installed apps**

![InstallOffice](TabInStageView/Images/InstallOffice.png)

**Select your app icon to launch your app in Office on the web**

![AppOffice](TabInStageView/Images/AppOffice.png) 

**Note:** Similarly, you can test your application in the Office 365 desktop app as well.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Tabs](https://learn.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
- [Stage view](https://learn.microsoft.com/microsoftteams/platform/tabs/tabs-link-unfurling#stage-view)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-stage-view-csharp" />