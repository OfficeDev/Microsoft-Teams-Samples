---
page_type: sample
description: This Teams bot collects user input via adaptive cards and saves it to a SharePoint list.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-bot-sharepoint-list-csharp
---

# Sharepoint List Bot

Bot Framework v4 SPListBot sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), This Microsoft Teams bot sample demonstrates how to capture user input through adaptive cards and store it in a SharePoint list. Built on the Bot Framework, it offers a seamless way to manage data directly from the Teams interface. The setup includes SharePoint registration, permissions configuration, and deployment in Teams, allowing easy integration and data handling within your Teams environment.

## Interaction with app

![SPListBot](SPListBot/Images/Preview.gif)


## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```

  [Teams Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
  
## Run the app (Using Teams Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.10 Preview 4 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Teams Toolkit for Visual Studio [Teams Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. In the debug dropdown menu of Visual Studio, select default startup project > **Microsoft Teams (browser)**
1. In Visual Studio, right-click your **TeamsApp** project and **Select Teams Toolkit > Prepare Teams App Dependencies**
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps.
1. Select **Debug > Start Debugging** or **F5** to run the menu in Visual Studio.
1. In the browser that launches, select the **Add** button to install the app to Teams.
> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## To try this sample

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `samples/bot-sharepoint-list/csharp` folder

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/bot-sharepoint-list/csharp` folder
  - Select `SPListBot.csproj` file
  - Press `F5` to run the project

  # Step 1: Register a new app

    You need to register a new addin/app in your Sharepoint site, this will generate a ClientID and a Client Secret,  which we will use to authenticate. Lets see how to do it.

    Go to `_layouts/15/appregnew.aspx` under the SP Online site which you want to use as document repository.

  # Step 2: Know your Tenant ID and Resource ID

    It is very important to know your tenant ID for triggering any kind of service calls.
    You can get your Tenant ID, Resource Id by following below points:
    1. Navigate to `https//{SharePointDomain}/_layouts/15/appprincipals.aspx`
    2. You will see Site Collection App Permissions under site settings.
    3. You can check your any App and get the Tenant Id and Resource Id from `App Identifier`. The   part after "@" is your `tenant ID` and the part before @ is `Resource ID`.


  # Step 3: Update your appSetting.json

  MicrosoftAppId: `<<Your Microsoft Bot_Id>>`

  "MicrosoftAppPassword": `<<Your Microsoft Bot_Secret>>`

  "BaseUrl": `<<Bot_endpoint_url>>`

  "TenantID":  `<<Sharepoint Tenant Id>>`

  "ResourceID": `<<SP_Resource_ID>>`

  "TenantName":  `<<sahrepoint Tenant Name>>`

  "SiteName":  `<<Shareppint site name>>`

  "ListName":  `<<Custom list name which created at site>>`

  "AppClientID": `<<Custom Client App ID created at sharepoint App>>`

  "AppSecret": `<<App Secret Id for sharepoint App>>` 

  # Step 4: Grant permissions
    New Client app has been created in SP Online site, now its time to decide what permissions this app should have on your site. You can grant Site collection, web or even at list level read or write permissions.

    Go to `/_layouts/15/appinv.aspx` and serach with ClientID we generated earlier. The application will fetch all other details based on your ClientID.


### This steps is specific to Microsoft Teams

- Navigate to `appPackage` folder
- Select the `Manifest.json` and update it with your `Your Bot Id`
- Now zip the manifest.json along with icons
- Go to teams and do `Upload a Custom App` 
- Add the Bot to Microsoft Teams
- Start the conversation with Bot

### Interaction with the Bot
- Ping the bot in 1:1 or channel scope
- Bot will send an Adaptive card having two fields name and address.
- Enter the values in Adaptive Card and click on Save button.
- Bot will save the card data in SharePoint List.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-sharepoint-list/csharp/SPListBot/AdapterWithErrorHandler.cs#L26) line and put your debugger for local debug.

### Screenshots
**Upload the custom app in Teams**

![image](SPListBot/Images/1.Install.png)

**Interaction with the Bot**

![image](SPListBot/Images/2.Installed.png)

**Ping the Bot**

![image](SPListBot/Images/3.Interaction_with_bot.png)


## Further reading
- [Conversational bots in teams](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots)
- [Conversation Basics](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet)
- [Granting Access using sharepoint app only](https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azureacs)
- [Sharepoint using Application Context](https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly)
- [Sharepoint API basic operations](https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/complete-basic-operations-using-sharepoint-rest-endpoints)




<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-sharepoint-list-csharp" />