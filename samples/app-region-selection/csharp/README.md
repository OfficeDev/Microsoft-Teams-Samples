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
- Select the region from the card
- bot sets the selected region and notify user in chat

## Interacting with Region Selection Tab

- Set up the region selection app as a Tab in channel
- Select the region from the configuration page and click on save
- Tab will display the selected region

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Build a Bot with Microsoft Teams](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-bot) for a complete list of deployment instructions.

## Further reading
- [Overview for Microsoft Teams App](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-first-app-overview)
- [Build a Configurable Tab for Microsoft Teams App](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-channel-tab)
- [Build a Bot](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-bot)

