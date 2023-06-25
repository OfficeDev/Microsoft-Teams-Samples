---
page_type: sample
description: This sample illustrates how you can use Teams App Installation Life Cycle by calling Microsoft Graph APIs through teams tab.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07-07-2021 13:38:26"
urlFragment: officedev-microsoft-teams-samples-graph-app-installation-lifecycle-csharp
---

# App Installation

This sample app demonstarte the installation lifecycle for Teams [Apps](https://docs.microsoft.com/graph/api/resources/teamsappinstallation?view=graph-rest-1.0) which includes create, update delete Apps

## Included Features
* Tabs
* Graph API

## Interaction with app

![](AppInstallation/Images/GraphAppInstallationLifecycleGif.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**App Installation:** [Manifest](/samples/graph-app-installation-lifecycle/csharp/demo-manifest/graph-app-installation-lifecycle.zip)

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

  - You need to add following permissions mentioned in the below screenshots to call respective Graph   API
    ![](https://user-images.githubusercontent.com/50989436/116188975-e155a300-a745-11eb-9ce5-7f467007e243.png)
    
2. Setup for Bot
  - Register a AAD aap registration in Azure portal.
  - Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
  - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
  - While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK
- Run ngrok - point to port 3978

```bash
# ngrok http 3978 --host-header="localhost:3978"
```

4. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Modify the `/appsettings.json` and fill in the following details:
  - `{{Your Microsoft App Id}}` - Generated from Step 1 while doing AAd app registration in Azure portal.
  - `{{ Your Microsoft App Password }}` - Generated from Step 1, also referred to as Client secret
  
  
 - Build your solution
   - Launch Visual Studio
   - File -> Open -> Project/Solution
   - Navigate to `samples/graph-app-installation-lifecycle/csharp/AppInstallation` folder
   - Select `AppInstallation.csproj` file
   - Press `F5` to run the project
5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./Manifest folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `Manifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./Manifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

- [Upload app manifest file](https://docs.microsoft.com/microsoftteams/platform/concepts/deploy-and-publish/apps-upload#load-your-package-into-teams) (zip file) to your team

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-appcatalog-lifecycle/nodejs/index.js#L45) line and put your debugger for local debug.

## Running the sample

![](AppInstallation/Images/Install.png)

![](AppInstallation/Images/AddAppListTab.png)

![](AppInstallation/Images/ListApp.png)

![](AppInstallation/Images/GetUserApp.png)

![](AppInstallation/Images/AppDescription.png)

![](AppInstallation/Images/DeleteApp.png)

## Further Reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Graph-app-installation](https://learn.microsoft.com/microsoftteams/plan-teams-lifecycle)
  
 
  
  
 



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/graph-app-installation-lifecycle-csharp" />