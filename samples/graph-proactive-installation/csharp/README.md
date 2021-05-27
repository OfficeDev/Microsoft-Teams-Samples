# Proactive Messages

Proactive installation of apps using Graph API and send messages

This bot has been created using [Bot Framework](https://dev.botframework.com), This sample app demonstarte the proactive installation of teams App using Graph API if app is not installed and send proactive notification.

## Concepts introduced in this sample

Your bot can proactively install an App for a user and send proactive notification using Graph API, it must be installed either as a personal app or in a team or Groupchat where the user is a member

Typically, each message that a bot sends to the user directly relates to the user's prior input. In some cases, a bot may need to send the user a message that is not directly related to the current topic of conversation. These types of messages are called proactive messages.

Exposure to `teamsApp` Graph API for installing app for User, Team or GroupChat.


## Prerequisites
### Tools

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version

- [Ngrok](https://ngrok.com/download) (Only for devbox testing) Latest (any other tunneling software can also be used)
  ```bash
  # run ngrok locally
  ngrok http -host-header=localhost 3978

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
  ```

## To try this sample

1. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
2. Run the bot from a terminal or from Visual Studio:

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/graph-proactive-installation/csharp` folder
  - Select `ProactiveBot.sln` file
  - Press `F5` to run the project
3. Run ngrok - point to port 3978
   ```ngrok http -host-header=rewrite 3978``
4. Create a new Bot by following steps mentioned in [Build a bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots?view=msteams-client-js-latest#build--a-bot-for-teams-with-the-microsoft-bot-framework) documentation.
5. Go to appsettings.json and add ```MicrosoftAppId``` ,  ```MicrosoftAppPassword```, ```MicrosoftTeamAppId``` information.
6. Run your app, either from Visual Studio with ```F5``` or using ```dotnet run``` in the appropriate folder.
7. Update the manifest.json file with ```Microsoft-App-ID```, ```Base Url```   value.
8. Installed in a team or the personal scope of a user or Groupchat.

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the latest Bot Framework Emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Interacting with the bot

In addition to responding to incoming messages, bots are frequently called on to send "proactive" messages based on activity, scheduled tasks, or external events.

In order to send a proactive message using Bot Framework, the bot must first capture a conversation reference from an incoming message using `TurnContext.getConversationReference()`. This reference can be stored for later use.

To send proactive messages, acquire a conversation reference, then use `adapter.continueConversation()` to create a TurnContext object that will allow the bot to deliver the new outgoing message.

## Deploy this bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Proactive installation of apps using Graph API and send messages](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=csharp)
- [Overview to teamsApp Graph API](https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0&preserve-view=true)
- [Send proactive notifications to users](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp&preserve-view=true)