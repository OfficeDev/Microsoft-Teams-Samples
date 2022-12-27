---
page_type: sample
description: Demonstrating the feature where user can send adaptivecard actions using bot.
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

# AdaptiveCard Actions

This sample shows the feature where user can send Adaptive Card actions using bot.

## Interaction with app

![Module](AdaptiveCardActions/Images/AdaptiveCardActions.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0
- [ngrok](https://ngrok.com/download) or equivalent tunnelling solution

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup
    
1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

2. Setup for Bot
	- Register a AAD aap registration in Azure portal.
	- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK
      - Run ngrok - point to port 3978

	```bash
	 #ngrok http -host-header=rewrite 3978
	```   
4. Setup for code

  - Clone the repository
    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
   - Modify the `/appsettings.json` and fill in the following details:
     - `{{MicrosoftAppId}}` - Generated from Step 1 is the application app id
     - `{{MicrosoftAppPassword}}` - Generated from Step 1, also referred to as Client secret
   
   - In a terminal, navigate to `samples/bot-adaptivecard-actions/csharp`

     change into project folder
      ```bash
      cd # AdaptiveCardActions
      ```
   - From a terminal
     ```bash
     # run the bot
     dotnet run
     ```

   - Or from Visual Studio
     - Launch Visual Studio
     - File -> Open -> Project/Solution
     - Navigate to `AdaptiveCardActions` folder
     - Select `AdaptiveCardActions.csproj` file
     - Press `F5` to run the project
     
5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./AppPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{Domain-Name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.
    - **Zip** up the contents of the `AppPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.


## Running the sample

![App Setup](AdaptiveCardActions/Images/1.AddApp.png)

![Welcome](AdaptiveCardActions/Images/2.Welcome.png)

![Runnning Sample](AdaptiveCardActions/Images/3.AdaptiveCard_Actions.png)

![Runnning Sample](AdaptiveCardActions/Images/4.ActionSubmit.png)

![Runnning Sample](AdaptiveCardActions/Images/5.ActionShowCard.png)

![Runnning Sample](AdaptiveCardActions/Images/6.Togglevisible.png)

![Runnning Sample](AdaptiveCardActions/Images/7.ToggleVisibleOnClick.png)

![Runnning Sample](AdaptiveCardActions/Images/8.SuggestedActions.png)

![Runnning Sample](AdaptiveCardActions/Images/9.RedColor.png)

![Runnning Sample](AdaptiveCardActions/Images/10.BlueColor.png)

![Runnning Sample](AdaptiveCardActions/Images/11.YellowColor.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)


