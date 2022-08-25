---
page_type: sample
description: Microsoft Teams app show end user region selection using Bot and Tab
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "19-03-2021 13:38:25"
---

# Region Selection App

Bot Framework v4 Region Selection sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), for the region selection for the app's data center using Bot and Tab.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## To try this sample

- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `samples/app-region-selection/RegionSelectionApp`

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/app-region-selection/RegionSelectionApp` folder
  - Select `RegionSelectionApp.sln` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the latest Bot Framework Emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)
- Put the `Microsoft App Id` and `Microsoft-App-Password` and `message endpoint` in Bot configuration
- Connect your Bot with Emulator, Ping the Bot to start the conversation

### This is specific to Microsoft Teams

 - **Edit** the `manifest.json` contained in the `Manifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere*      you,see the placeholder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Add** the ngrok domain to the valid domains array in the manifest. 
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

## Interacting with the bot

Install the Region Selection App manifest in Microsoft Teams. @mention the region selection bot to start the conversation.
- Bot sends an Adaptive card in chat
![image](https://user-images.githubusercontent.com/50989436/108982436-cf0ea880-76b3-11eb-9d95-5f0394f92851.png)
- Select the region from the card
![image](https://user-images.githubusercontent.com/50989436/108982477-dafa6a80-76b3-11eb-8165-994feb4e0f75.png)
- Bot sets the selected region and notify user in chat
![image](https://user-images.githubusercontent.com/50989436/108982510-e483d280-76b3-11eb-9501-a382e7fba6e6.png)

## Interacting with Region Selection Tab

- Set up the region selection app as a Tab in channel
![image](https://user-images.githubusercontent.com/50989436/108982548-ef3e6780-76b3-11eb-8133-7578a121dac9.png)
- Select the region from the configuration page and click on save
![image](https://user-images.githubusercontent.com/50989436/108982582-f796a280-76b3-11eb-815e-c3c51e5c54a3.png)
- Tab will display the selected region
![image](https://user-images.githubusercontent.com/50989436/108982607-fe251a00-76b3-11eb-9fc6-a572cc29109f.png)


## Further reading
- [Overview for Microsoft Teams App](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-first-app-overview)
- [Build a Configurable Tab for Microsoft Teams App](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-channel-tab)
- [Build a Bot](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-bot)

