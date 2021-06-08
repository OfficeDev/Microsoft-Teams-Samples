# Proactive Installation Sample App

This sample app illustartes the proactive installation of app using Graph API and sending proactive notification to users from GroupChat or Channel.

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
  - Navigate to `samples/graph-proactive-installation/csharp` folder
  - Select `ProactiveBot.sln` file
  - Press `F5` to run the project
3. Run ngrok - point to port 3978
   ```ngrok http -host-header=rewrite 3978``
5. Go to appsettings.json and add ```MicrosoftAppId``` ,  ```MicrosoftAppPassword```, ```TeamsappcatalogAppId``` information.You can Navigate to following link in your browser [Get TeamsAppCatalogId](https://developer.microsoft.com/en-us/graph/graph-explorer?request=appCatalogs%2FteamsApps%3F%24filter%3DdistributionMethod%20eq%20'organization'&method=GET&version=v1.0&GraphUrl=https://graph.microsoft.com) from Microsft Graph explorer.  
You can  search with app name or based on Manifest App id  in Graph Explorer response and copy the `Id` [i.e AppCatalogTeamAppId]
6. Run your app, either from Visual Studio with ```F5``` or using ```dotnet run``` in the appropriate folder.
7. Update the manifest.json file with ```Microsoft-App-ID```, ```Base Url```   value.

### Required Microsoft graph Application level permissions to run this sample app

- `TeamsAppInstallation.Read.Group*`
- `TeamsAppInstallation.ReadWriteSelfForUser.All`
- `TeamsAppInstallation.ReadWriteForUser.All`
- `TeamsAppInstallation.ReadWriteSelfForChat.All`
- `TeamsAppInstallation.ReadWriteForChat.All`
- `TeamsAppInstallation.ReadWriteSelfForTeam.All`

### Interacting with the Proactive installation App in Teams
- Install the proactive app Bot in Team, GroupChat or Personal Scope.
![image](https://user-images.githubusercontent.com/50989436/120750023-3ba30a00-c523-11eb-9065-3a6b3ec706ab.png)

- Bot will send an welcome message after installation
![image](https://user-images.githubusercontent.com/50989436/120749546-6ccf0a80-c522-11eb-84a8-2191b1dcb08f.png)
- Run Check and install command for the Bot
- Bot will check the app installed for users or not, if not it will install the app in their personal scope and send a proactive notification else it will send a proactive notification saying app is already installed
![image](https://user-images.githubusercontent.com/50989436/120749801-d51dec00-c522-11eb-8eb9-5243eb9fe470.png)

## Further Reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Proactive App Installation using Graph API](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=Csharp)