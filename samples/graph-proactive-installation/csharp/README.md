---
page_type: sample
description: This sample illustrates how you can use Proactive installation of app for user and send proactive notification by calling Microsoft Graph APIs.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-graph-proactive-installation-csharp
---

# Proactive Installation Sample App

This sample app illustartes the proactive installation of app using Graph API and sending proactive notification to users from GroupChat or Channel.

Language Used : C#

## Prerequisites
### Tools

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## To try this sample

- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. Clone the repository
   ```bash
    git clone https://github.com/OfficeDev/microsoft-teams-samples.git
    ```
2. Run the bot from a terminal or from Visual Studio:
    - Launch Visual Studio
    - File -> Open -> Project/Solution  
    - Navigate to `samples/graph-proactive-installation/csharp` folder
    - Select `ProactiveAppInstallation.sln` file
    - Press `F5` to run the project
3. Run ngrok to point to port 3978
   ```bash
   ngrok http -host-header=rewrite 3978
    ```
4. Update the manifest.json file with `Microsoft-App-ID` value and to get TeamsAppCatalogId upload your     Manifest  for my Organization.
![image](https://user-images.githubusercontent.com/85157377/122389115-38c9ff80-cf8e-11eb-8cda-0a836cb26b34.png)
5. Go to appsettings.json and update `MicrosoftAppId` ,  `MicrosoftAppPassword`, `TeamsappcatalogAppId` information. 
   - To get `TeamsappcatalogAppId` you first navigate to following link in your browser [Get TeamsAppCatalogId](https://developer.microsoft.com/en-us/graph/graph-explorer?request=appCatalogs%2FteamsApps%3F%24filter%3DdistributionMethod%20eq%20'organization'&method=GET&version=v1.0&GraphUrl=https://graph.microsoft.com) from Microsoft Graph explorer. Then search with app name or based on Manifest App id  in Graph Explorer response and copy the `Id` [i.e teamApp.Id]
6. Required Microsoft graph Application level permissions to run this sample app
     - TeamsAppInstallation.ReadWriteForUser.All
7. [Get consent for the Application permissions](https://docs.microsoft.com/en-us/graph/auth-v2-service?context=graph%2Fapi%2F1.0&view=graph-rest-1.0#3-get-administrator-consent) by following steps mentioned here.
8. Run your app, either from Visual Studio with ```F5``` or using ```dotnet run``` in the appropriate folder.



### Interacting with the Proactive installation App in Teams
- Install the Proactive App Installation demo in a Team or GroupChat.
     ![image](https://user-images.githubusercontent.com/31851992/122173819-cd096900-ce9f-11eb-93a1-8028e6b1a46e.png)

- **Team Scope**: Run Check and install to pro-actively installs the App for all the users in team. After installation send 'Send message' command to send proactive message.
     ![image](https://user-images.githubusercontent.com/31851992/122173110-0ee5df80-ce9f-11eb-8037-4257afa95406.png)
- **Group Chat**:  Run Check and install to pro-actively installs the App for all the users in team. After installation send 'Send message' command to send proactive message.
    ![image](https://user-images.githubusercontent.com/31851992/122173594-83208300-ce9f-11eb-9100-7e9373d2a531.png)


## Further Reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Proactive App Installation using Graph API](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=Csharp)
