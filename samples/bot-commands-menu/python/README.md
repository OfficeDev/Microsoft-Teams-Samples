---
page_type: sample
description: Microsoft Teams Create Commands Menu
products:
- office-teams
- office
- office-365
languages:
- python
extensions:
 contentType: samples
 createdDate: "12-12-2024 13:38:25"
urlFragment: officedev-microsoft-teams-samples-bot-commands-menu-nodejs
---

# Create a commands menu
This sample add a command menu with a dropdown list of commands for your bot. Users can select a command from the list, which will insert the command title into the compose box. Then, they can select Send to execute the command.

## Included Features
* Bots
* Custom Engine Agents

## Interaction with app

![Preview Image](Images/Botcommandsmenu.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [Python SDK](https://www.python.org/downloads/) min version 3.6
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution


## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. Remember to select `requirements.txt` as dependencies to install when creating the virtual environment.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3) Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  
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


4) Create [Azure Bot resource resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Azure free account here](https://azure.microsoft.com/free/)

5) In a terminal, go to `samples\bot-commands-menu`

6) Activate your desired virtual environment

8) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

9) Update the `config.py` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

10) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `${{AAD_APP_CLIENT_ID}}` and `${{TEAMS_APP_ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

11) Run your bot with `python app.py`

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

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-commands-menu-python" />