---
page_type: sample
description: Sample which demonstrates different Adaptive Card action types using bot.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "12/27/2022 12:30:00 PM"
urlFragment: officedev-microsoft-teams-samples-bot-adaptivecard-actions-csharp
---

# Send Adaptive Card Including Different Actions

This sample shows the feature where user can send Adaptive Card actions using bot.

## Included Features
* Bots
* Adaptive Cards

## Interaction with app

![Module](AdaptiveCardActions/Images/AdaptiveCardActions.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams bot adaptivecard actions sample app:** [Manifest](/samples/bot-adaptive-card-actions/csharp/demo-manifest/bot-adaptivecard-actions.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
- [Teams Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Run the app (Using Teams Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.10 Preview 4 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Teams Toolkit for Visual Studio [Teams Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. In the debug dropdown menu of Visual Studio, select default startup project > **Microsoft Teams (browser)**
1. In Visual Studio, right-click your **TeamsApp** project and **Select Teams Toolkit > Prepare Teams App Dependencies**
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps.
1. Select **Debug > Start Debugging** or **F5** to run the menu in Visual Studio.
1. In the browser that launches, select the **Add** button to install the app to Teams.
> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup
   
1. Setup For Bot
	- Register a Microsoft Entra ID aap registration in Azure portal [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908).
	- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

1. Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3. Setup For Code

  - Clone the repository
    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

  - If you are using Visual Studio
 
  - Launch Visual Studio
  - File -> Open Folder
  - Navigate to `samples/bot-adaptive-card-actions/csharp/AdaptiveCardActions` folder
  - Select `AdaptiveCardActions.sln` solution file

   - Modify the `/appsettings.json` and fill in the following details:
     - `{{MicrosoftAppId}}` - Generated from Step 1 is the application app id
     - `{{MicrosoftAppPassword}}` - Generated from Step 1, also referred to as Client secret
  - Press `F5` to run the project
     
4. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{Domain-Name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-adaptive-card-actions/csharp/AdaptiveCardActions/AdapterWithErrorHandler.cs#L28) line and put your debugger for local debug.


## Running the sample

![App Setup](AdaptiveCardActions/Images/1.Install.png)

![Welcome](AdaptiveCardActions/Images/2.WelcomeMessage.png)

![Runnning Sample](AdaptiveCardActions/Images/9.SuggestedActions.png)

![Runnning Sample](AdaptiveCardActions/Images/3.Red.png)

![Runnning Sample](AdaptiveCardActions/Images/4.Yellow.png)

![Runnning Sample](AdaptiveCardActions/Images/5.Blue.png)

![Runnning Sample](AdaptiveCardActions/Images/6.CardActions.png)

![Runnning Sample](AdaptiveCardActions/Images/7.ActionSubmit.png)

![Runnning Sample](AdaptiveCardActions/Images/8.ActionShowCard.png)

![Runnning Sample](AdaptiveCardActions/Images/10.ToggleVisibiliyCard.png)

![Runnning Sample](AdaptiveCardActions/Images/11.VisibleOnClick.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Actions](https://learn.microsoft.com/adaptive-cards/rendering-cards/actions)
- [Send suggested actions](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?tabs=dotnet#send-suggested-actions)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-adaptive-card-actions-csharp" />