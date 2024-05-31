---
page_type: sample
description: Sample which demonstrates how to archive groupchat messages and send it to user as a file using bot.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "11/10/2021 11:35:25 PM"
urlFragment: officedev-microsoft-teams-samples-bot-archive-groupchat-messages-csharp
---

# Archive groupchat messages

Using this C# sample, a bot can archive chat messages of groupchat and send it to user.

This feature shown in this sample is currently available in Public Developer Preview only.

## Included Features
* Teams SSO (bots)
* Adaptive Cards
* Graph API

## Interaction with app

![GroupChatModule](FetchGroupChatMessages/Images/FetchGroupChatModule.gif)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [Ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

- [Teams Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Setup

## Run the app (Using Teams Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.9 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Teams Toolkit for Visual Studio [Teams Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. In Visual Studio, right-click your project and **Select Teams Toolkit > Prepare Teams App Dependencies**
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps.
1. Select **Debug > Start Debugging** or **F5** to run the menu in Visual Studio.
1. In the browser that launches, select the **Add** button to install the app to Teams.
> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.   
   
    *Enter the following  to Click Expose An API Menu and add a scope details to configure the admin and user consent prompts with values that are appropriate for the access_as_user scope.*
   - Added the WebApplication Info Resource and Application ID URI in expose and API like: api://Tunnelbaseurl/App-id"
   - Admin consent display name: Teams can access the user’s profile.
   - Admin consent description: Teams can call the app’s web APIs as the current user.
   - User consent display name: Teams can access your profile and make requests on your behalf.
   - User consent description: Teams can call this app’s APIs with the same rights as you have.
   
   ### Instruction on setting connection string for bot authentication on the behalf of user

   - In the Azure portal, select your resource group from the dashboard.
   - Select your bot channel registration link.
   - Open the resource page and select Configuration under Settings.
   - Select Add OAuth Connection Settings.
   - Complete the form as follows:

    - **Name:** Enter a name for the connection. You'll use this name in your bot in the appsettings.json file. For example BotTeamsAuthADv1.
    - **Service Provider:** Select Azure Active Directory V2. Once you select this, the Azure AD-specific fields will be displayed.
    - **Client id:** Enter the Application (client) ID .
    - **Client secret:** Enter the Application (client) secret.
    -  Provide **Scopes** like "User.Read Chat.ReadWrite ChatMessage.Read"

   ### Go to the Azure portal where app registration is created and click on API Permissions
 
    - Add this Delegated permission to app registration
    - Chat.ReadWrite
    - ChatMessage.Read
    ![Permissions](FetchGroupChatMessages/Images/permissions.png)

   ### Under left menu, select  **Authentication**  under  **Manage**  section
   
    - Select 'Accounts in any organizational directory (Any Azure AD directory - Multitenant)' under Supported account types and click "+Add a platform".
    -  On the flyout menu, Select "Web"    
    -  Add  `https://token.botframework.com/.auth/web/redirect`  under Redirect URLs and click Configure button.
    -  Once the flyout menu close, scroll bottom to section 'Implicit Grant' and select check boxes "Access tokens" and "ID tokens" and click "Save" at the top bar.

2. Setup for Bot
	
	- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0)
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK
 - Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

4. Setup for code

  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
  - Modify the `/appsettings.json` and fill in the following details:
  
  - `{{MICROSOFT-APP-ID}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal.
  - `{{ MICROSOFT-APP-PASSWORD}}` - Generated from Step 1, also referred to as Client secret
  - `{{ Connection Name }}` - Generated from Step 1, also referred as Instruction on setting connection.
  
  - From a terminal, navigate to `samples/bot-archive-groupchat-messages/csharp`

	  ```bash
	  # run the bot
	  dotnet run
	  ```
	  Or from Visual Studio
	     - Launch Visual Studio
	     - File -> Open -> Project/Solution
	     - Navigate to `bot-archive-groupchat-messages/csharp` folder
	     - Select `FetchGroupChatMessages.sln` file
	     - Press `F5` to run the project
      
 5. Setup Manifest for Teams
	- __*This step is specific to Teams.*__
	    - **Edit** the `manifest.json` contained in the ./AppManifest folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
	    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
      - **Edit** the `manifest.json` for `"webApplicationInfo"` resource  `"api://botid-<<MICROSOFT-APP-ID>>"` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your resource will be `api://botid-<<MICROSOFT-APP-ID>>`.
     
	    - **Zip** up the contents of the `AppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

	- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
	   - Go to Microsoft Teams. From the lower left corner, select Apps
	   - From the lower left corner, choose Upload a custom App
	   - Go to your project directory, the ./AppManifest folder, select the zip folder, and choose Open.
	   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-archive-groupchat-messages/csharp/FetchGroupChatMessages/AdapterWithErrorHandler.cs#L23) line and put your debugger for local debug.
    
## Running the Sample

- Sending archive chat messages text file of a groupchat to user

![setup bot](FetchGroupChatMessages/Images/setupbot.png)

![Welcome](FetchGroupChatMessages/Images/welcome.png)

![Bot command](FetchGroupChatMessages/Images/botCommandToGetChatMessages.png)

![Bot reply](FetchGroupChatMessages/Images/replyFromBot.png)

## Interacting with the bot in GroupChat

Select a groupchat and add the bot to chat.

Send `getchat` message to the bot, you will recieve a consent card by the bot in your personal scope.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-archive-groupchat-messages-csharp" />