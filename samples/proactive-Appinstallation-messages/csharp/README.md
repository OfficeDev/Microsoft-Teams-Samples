# Proactive Installation Sample App

Proactive installation of apps using Graph API and send messages

It shows how to send proactive messages to team (channel) or GroupChat after installing the App.

## Prerequisites
### Tools

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## To try this sample

1. Clone the repository
   ```bash
    git clone https://github.com/OfficeDev/microsoft-teams-samples.git
    ```
2. Run the bot from a terminal or from Visual Studio:

  - Launch Visual Studio
  - File -> Open -> Project/Solution  
  - Navigate to `samples/csharp_dotnetcore/Proactive_Installation _Sample` folder
  - Select `ProactiveBot.sln` file
  - Press `F5` to run the project
3. Run ngrok - point to port 3978
   ```ngrok http -host-header=rewrite 3978``
5. Go to appsettings.json and add ```MicrosoftAppId``` ,  ```MicrosoftAppPassword```, ```MicrosoftTeamAppId``` information.
6. Run your app, either from Visual Studio with ```F5``` or using ```dotnet run``` in the appropriate folder.
7. Update the manifest.json file with ```Microsoft-App-ID```, ```Base Url```   value.

## Concepts introduced in this sample

### Descriptions MS TeamsApp resource type

- Graph API Authentication  using Application Permissions
- List of apps in team 
- Add App to Team
- List of Apps in Groupchat
- Add App to Groupchat
- Add App to the Personal scope of Groupchat members


1. Graph API Authentication  using Application Permissions
    Deploy this app to Azure and give API permissions as Application.For more information (https://docs.microsoft.com/en-us/graph/auth-v2-service)
2. List of apps in team
    Retrieve a list of apps installed in the specified team [To get the details we need application Permission].
    For more information (https://docs.microsoft.com/en-us/graph/auth-v2-service)
3. Add App to Team
    If the app is not installed  in the specified team  we have to install the app proactively and send message [To install this app into Teams we need application Permissions].
    For more information (https://docs.microsoft.com/en-us/graph/api/team-post-installedapps?view=graph-rest-1.0&tabs=http)
4. List of apps in GroupChat
    Retrieve a list of apps installed in the specified Groupchat [To get the details we need application Permissions].For more information (https://docs.microsoft.com/en-us/graph/api/chat-list-installedapps?view=graph-rest-1.0&tabs=http)
5. Add App to GroupChat
    If the app is not installed  in the specified GroupChat  we have to install the app proactively and send message [To install this app into GroupChat we need application Permissions].
    For more information (https://docs.microsoft.com/en-us/graph/api/chat-post-installedapps?view=graph-rest-1.0&tabs=http)
6. Add App to the Personal scope of Groupchat members
    While installing the app in groupchat we have to check the app in their personal scope if its not installed we have install and send a message.[To install this app from  GroupChat into personal Scope we need application Permissions], For more information (https://docs.microsoft.com/en-us/graph/api/userteamwork-post-installedapps?view=graph-rest-1.0&tabs=http)
7.  To install the App in in a Channel or Groupchat we required TeamAppId. It will get from this link 
    (https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/) from Microsft Graph explorer 


- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Portal](https://portal.azure.com)
