---
page_type: sample
description: This sample app shows the interaction between teams bot and SharePoint List, Bot saves the specified details in SharePoint List as back-end
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

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a simple bot that accepts input from the user and save it into sharepoint's List.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

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

- Navigate to `teamsAppManifest` folder
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

![image](https://user-images.githubusercontent.com/50989436/109759882-c36f3480-7c13-11eb-9a38-e69d2c7139e7.png)

**Interaction with the Bot**

![image](https://user-images.githubusercontent.com/50989436/109759972-eb5e9800-7c13-11eb-9246-e8fa02fbef64.png)

**Ping the Bot**

![image](https://user-images.githubusercontent.com/50989436/109760014-fe716800-7c13-11eb-932d-9c692d4a67ae.png)


## Further reading
- [Conversational bots in teams](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots)
- [Conversation Basics](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet)
- [Granting Access using sharepoint app only](https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azureacs)
- [Sharepoint using Application Context](https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly)
- [Sharepoint API basic operations](https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/complete-basic-operations-using-sharepoint-rest-endpoints)




<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-sharepoint-list-csharp" />