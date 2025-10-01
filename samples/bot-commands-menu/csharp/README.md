---
page_type: sample
description: Microsoft Teams Create Commands Menu
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "05/17/2024 01:38:25 PM"
urlFragment: officedev-microsoft-teams-samples-bot-commands-menu-csharp
---

# Create a commands menu
This sample add a command menu with a dropdown list of commands for your bot. Users can select a command from the list, which will insert the command title into the compose box. Then, they can select Send to execute the command.

## Included Features
* Bots
* Custom Engine Agents

## Interaction with app

![Preview Image](Images/Botcommandsmenu.gif)

 ## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Bot Commands Menu:** [Manifest](/samples/bot-commands-menu/csharp/demo-manifest/bot-commands-menu.zip)

## Prerequisites

Verify you have the right account for building Teams apps and install some recommended development tools.

- You need a Teams account that allows [custom app uploading](https://docs.microsoft.com/microsoftteams/platform/build-your-first-app/build-first-app-overview#set-up-your-development-account).
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Microsoft 365 Agents Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio [Microsoft 365 Agents Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`.
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
</br>![image](https://raw.githubusercontent.com/OfficeDev/TeamsFx/dev/docs/images/visualstudio/debug/debug-button.png)
1. In the opened web browser, select Add button to install the app in Teams
> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup
1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  A) Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  B) On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
  C) Navigate to **API Permissions**, and make sure to add the following permissions:
   Select Add a permission
      * Select Add a permission
      * Select Microsoft Graph -\> Delegated permissions.
      * `User.Read` (enabled by default)
      * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
      
    > NOTE: When you create your app registration, you will create an App ID and App password (Secret) - make sure you keep these for later.

2. Setup for Bot
	
	- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0)
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

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
  - `{{MicrosoftAppId}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal.
  - `{{MicrosoftAppPassword}}` - Generated from Step 1, also referred to as Client secret

- Run the bot from a terminal or from Visual Studio:

5. If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `bot-commands-menu\csharp` folder
  - Select `CommandsMenu.csproj` file

6. This step is related to Microsoft Teams app manifest
    - **Edit** the `manifest.json` contained in the `appPackage`  folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `{{MicrosoftAppId}}` and <<Azure Bot ID>> and for the contentUrl "<<Tunnel Url>>?culture={locale}" (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
   - **Provide turnnelling Url** in `manifest.json` for contentUrl in case of tabs and for messaging endpoint in case of bots if enabled
   - replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `Manifest` or `Manifest_hub` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")
    
**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-commands-menu/csharp/CommandsMenu/AdapterWithErrorHandler.cs#L24) line and put your debugger for local debug.

# Running the sample

**Install App Personal**
![image](Images/1.InstallApp.png)

![2.PromptStarters](Images/2.PromptStarters.png)

![3.SearchFlights](Images/3.SearchFlights.png)

![4.SearchHotels](Images/4.SearchHotels.png)

![3.SearchFlightsDetails](Images/3.SearchFlightsDetails.png)

![3.SearchFlightsSearch](Images/3.SearchFlightsSearch.png)

![5.ViewPrompts](Images/5.ViewPrompts.png)

**Copilot Custom Engine Agents**
![Copilot](Images/CopilotInstall.png) 

![Copilot](Images/Copilot1.png) 

![Copilot](Images/Copilot2.png) 

![Copilot](Images/Copilot3.png) 

![Copilot](Images/Copilot4.png) 

![Copilot](Images/Copilot5.png) 

![Copilot](Images/Copilot6.png) 

![Copilot](Images/Copilot7.png) 

![Copilot](Images/Copilot8.png) 

![Copilot](Images/Copilot9.png) 

![Copilot](Images/Copilot10.png) 

## Further reading

- [Create a commands menu](https://review.learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/create-a-bot-commands-menu?branch=pr-en-us-10866&tabs=desktop%2Cdotnetlocalization)
- [Create prompt suggestions](https://review.learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/prompt-suggestions?branch=pr-en-us-10866&tabs=manually-in-the-app-manifest%2Cjavascript)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-commands-menu-csharp" />