# Proactive Installation Sample App
Proactive installation of apps using Graph API and send messages
It shows how to send proactive messages to team (channel) or GroupChat after installing the App.

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version
    ```

## To try this sample

- Clone the repository
    ```bash
    git clone https://github.com/OfficeDev/microsoft-teams-samples.git
    ```
- In a terminal, navigate to `samples/javascript_nodejs/Proactive-installation-Sample-node`
    ```bash
    cd samples/javascript_nodejs/Proactive-installation-Sample-node
    ```
- Install modules
    ```bash
    npm install
    ```
- Start the bot
    ```bash
    npm start
    ```

2. If you are using Visual Studio code
- Launch Visual Studio code
- Folder -> Open -> Project/Solution
    cd samples/javascript_nodejs/Proactive-installation-Sample-node
- Navigate to ```samples\javascript_nodejs\Proactive-installation-Sample-node\``` folder
- Select ```Proactive-installation-Sample-node``` Folder
3. To run the application required  node modules.Please use this command to install modules npm i
4. Run ngrok - point to port 3978
   ```ngrok http -host-header=rewrite 3978```
5. Create a new Bot by following steps mentioned in [Build a bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots?view=msteams-client-js-latest#build--a-bot-for-teams-with-the-microsoft-bot-framework) documentation.
6. Go to .env file  and add ```MicrosoftAppId``` ,  ```MicrosoftAppPassword``` and ``` AppCatalogTeamAppId ``` information.
7. Run your app, either from Visual Studio code  with ``` npm start``` or using ``` Run``` in the Terminal.
8. Update the manifest.json file with ```Microsoft-App-ID``` and ```ContentUrl``` value.
9. Install the app in Teams.


### Microsoft Graph Applications Permisssions helps you install the Application in Team (channel) or GroupChat

- TeamsAppInstallation.Read.Group*
- TeamsAppInstallation.ReadWriteSelfForUser.All
- TeamsAppInstallation.ReadWriteForUser.All
- TeamsAppInstallation.ReadWriteSelfForChat.All
- TeamsAppInstallation.ReadWriteForChat.All
- TeamsAppInstallation.ReadWriteSelfForTeam.All

### Instruction on setting  Application Permissions on Azure Portal   
- Go to Azure portal,Regiater your application
- Set the All the application Permissions like below
- ![image](https://user-images.githubusercontent.com/85157377/121150027-cc018780-c860-11eb-9c0b-581903c33878.png)

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
7.  To install the App in in a Channel or Groupchat we required AppCatalogTeamAppId. You can Navigate to following link in your browser [Get TeamsAppCatalogId](https://developer.microsoft.com/en-us/graph/graph-explorer?request=appCatalogs%2FteamsApps%3F%24filter%3DdistributionMethod%20eq%20'organization'&method=GET&version=v1.0&GraphUrl=https://graph.microsoft.com) from Microsft Graph explorer.
8. You can  search with app name or based on Manifest App id  in Graph Explorer response and copy the `Id` [i.e AppCatalogTeamAppId]

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
- [Proactive App Installation using Graph API](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=nodejs)