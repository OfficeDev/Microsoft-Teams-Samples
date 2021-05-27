# Proactive Messages

Proactive installation of apps using Graph API and send messages

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to send proactive messages to user (personal) or team (channel) or GroupChat after installing the TeamApp by capturing a conversation reference, then using it later to initialize outbound messages.

## Concepts introduced in this sample

Your bot can proactively message a user, it must be installed either as a personal app or in a team or Groupchat where the user is a member

Typically, each message that a bot sends to the user directly relates to the user's prior input. In some cases, a bot may need to send the user a message that is not directly related to the current topic of conversation. These types of messages are called proactive messages.

Proactive messages can be useful in a variety of scenarios. If a bot sets a timer or reminder, it will need to notify the user when the time arrives. Or, if a bot receives a notification from an external system, it may need to communicate that information to the user immediately. For example, if the user has previously asked the bot to monitor the price of a product, the bot can alert the user if the price of the product has dropped by 20%. Or, if a bot requires some time to compile a response to the user's question, it may inform the user of the delay and allow the conversation to continue in the meantime. When the bot finishes compiling the response to the question, it will share that information with the user.

This project has a notify endpoint that will trigger the proactive messages to be sent to
all users who have previously messaged the bot.

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
    git clone https://github.com/microsoft/botbuilder-samples.git
    ```
2. Run the bot from a terminal or from Visual Studio:

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/csharp_dotnetcore/proactive-Appinstallation-messages` folder
  - Select `ProactiveBot.csproj` file
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

Install Proactive Bot in Personal scope
![image](https://user-images.githubusercontent.com/50989436/119795172-daad7d80-bef5-11eb-9874-3465d8d8ca3e.png)

![image](https://user-images.githubusercontent.com/50989436/119795259-ef8a1100-bef5-11eb-89a2-a4d4b3b496a3.png)

Proactive Bot in Teams
![image](https://user-images.githubusercontent.com/50989436/119795336-016bb400-bef6-11eb-8860-b2ba19d5dc0c.png)

Proactive Bot in Group Chat
![image](https://user-images.githubusercontent.com/50989436/119795404-0fb9d000-bef6-11eb-8609-62dddd811f6c.png)

## Deploy this bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [continueConversation Method](https://docs.microsoft.com/en-us/javascript/api/botbuilder/botframeworkadapter#continueconversation)
- [getConversationReference Method](https://docs.microsoft.com/en-us/javascript/api/botbuilder-core/turncontext#getconversationreference)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
