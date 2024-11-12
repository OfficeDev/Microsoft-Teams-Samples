---
page_type: sample
description: This sample app can be use to streaming scenarios in Teams using Azure Open AI and Bot Framework v4 for personal scope.
products:
- office-teams
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "10/08/2024"
---

# Teams Streaming Bot Sample

This bot has been created using [Bot Framework](https://dev.botframework.com) and [Azure Open AI](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal) as a secondary/alternative option to using [Teams AI SDK](https://github.com/microsoft/teams-ai/tree/main/js/samples/04.ai-apps/i.teamsChefBot-streaming). 

Its main purpose is to demonstrate how to build a bot connected to an LLM and send messages through Teams.

## Included Features
* Bots
* Azure Open AI
* Streaming

> [!IMPORTANT]
> This bot doesn't save any context calls. Therefore, each interaction is individual and unique.

## Prerequisites

- Microsoft Teams is installed and you have an account
- Have an Azure Open AI resource and a corresponding deployment
- Have an Azure Bot
- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution

## Setup

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

1) Clone the repository

    ```bash
    git clone https://github.com/SolangeAO/streamig-bot-demo.git
    ```

1) If you are using Visual Studio
   - Launch Visual Studio
   - File -> Open -> Project/Solution
   - Navigate to `samples/bot-conversation/csharp` folder
   - Select `TeamsConversationBot.csproj` or `TeamsConversationBot.sln`file

1) Update the `appsettings.json` configuration for the bot and the Azure Open AI resource.

1) Run your bot, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

1) __*This step is specific to Teams.*__
    - **Zip** up the contents of the `TeamsAppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal/team/groupChat scope (Supported scopes)
    - With CTRL + Alt + Shift + 8 open the UI dev tools pane. Go to "Settings" and search `enablebotstreaming`. Enable the flag and reload the page for the UI to reflect the changes and allow UI to reflect streaming feature changes.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-conversation/csharp/AdapterWithErrorHandler.cs#L25) line and put your debugger for local debug.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Messages in bot conversations](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?tabs=dotnet)
- [Receive a read receipt](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/conversations/conversation-messages?branch=pr-en-us-9184&tabs=dotnet1%2Capp-manifest-v112-or-later%2Cdotnet2%2Cdotnet3%2Cdotnet4%2Cdotnet5%2Cdotnet#receive-a-read-receipt)
- [Azure Open AI Client Library Documentation](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai-readme?view=azure-dotnet) 

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-conversation-csharp" />