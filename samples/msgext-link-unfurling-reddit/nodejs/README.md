---
page_type: sample
description: Messaging Extension sample with Link Unfurling feature for Reddit Links
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "10/27/2023 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-msgext-link-unfurling-reddit-nodejs
---

# Link Unfurling for Reddit Links

![Preview Image](doc/image/link.png)

This repository is a full implementation of [link unfurling](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling?tabs=dotnet) for [Reddit](https://reddit.com) links in nodejs.

This sample demonstrates the following concepts: 
- Link Unfurling

### Configure Reddit App
Go To The [Reddit App Preferences](https://www.reddit.com/prefs/apps/) and register a new app for Reddit using the following parameters. 

| Parameter        | Value                      |
|------------------|:---------------------------|
| __Type__         | `web app`                  |
| __redirect uri__ | Not required               |
| Description      | Your own description       |
| About Url        | Url to your own about page |

Afterwards be sure to save the `client id` and the `secret` for the next step. 
## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. In "env/.env.local" and "env/.env.local.dev" file, set values for `REDDIT_ID` and `SECRET_REDDIT_PASSWORD` that are gained in the last step
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.
The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.
You will need to complete the following before running the code

### Create Messaging Extension
Follow the directions for [creating a messaging extension](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/create-messaging-extension).
- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. You __must__ use the 'Bot Channel Registration' so Bot Framework token service can be registered to manage tokens. 
2. The `reddit.com` and `www.reddit.com` domains should be registered in the 'messageHandlers' for the Teams App. If these are not included, the extension will not trigger for reddit links!

Make sure to note the app id and password for later. 




## References

- [Link Unfurling BotBuilder Sample](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/55.teams-link-unfurling)




<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/msgext-link-unfurling-reddit-csharp" />