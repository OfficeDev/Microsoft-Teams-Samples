---
page_type: sample
description: This Microsoft Teams app allows users to select and set a region using a Bot and Tab.
products:
- office-teams
- office
- office-365
languages:
- python
extensions:
 contentType: samples
 createdDate: "13-02-2025 13:38:25"
urlFragment: officedev-microsoft-teams-samples-app-region-selection-python
---

# Region Selection App

A Microsoft Teams sample app for region selection, leveraging both Bot and Tab interactions. The app features Adaptive Cards to facilitate region configuration and provides a seamless experience to manage data center selection through the Teams client.

Bot Framework v4 Region Selection sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), for the region selection for the app's data center using Bot and Tab.

## Included Features
* Bots
* Tabs
* Adaptive Cards

## Interaction with app
![region-selection-bot ](RegionSectionApp/Images/region-selection.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [Python SDK](https://www.python.org/downloads/) min version 3.6
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution


## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. Remember to select `requirements.txt` as dependencies to install when creating the virtual environment.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (uploading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

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

3) Create [Azure Bot resource resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Azure free account here](https://azure.microsoft.com/free/)

4) In a terminal, go to `samples\app-region-selection`

5) Activate your desired virtual environment

6) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

7) Update the `config.py` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

8) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `${{AAD_APP_CLIENT_ID}}` and `${{TEAMS_APP_ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

9) Run your bot with `python app.py`

## Running the sample

Install the Region Selection App manifest in Microsoft Teams. @mention the region selection bot to start the conversation.
- Bot sends an Adaptive card in chat
![image](RegionSectionApp/Images/region-details-bot.png)
- Select the region from the card. Bot sets the selected region and notify user in chat
![image](RegionSectionApp/Images/region-change-bot.png)

## Interacting with Region Selection Tab

- Set up the region selection app as a Tab in channel
![image](RegionSectionApp/Images/region-config.png)
- Tab will display the selected region
![image](RegionSectionApp/Images/region-details.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.


## Further reading
- [Overview for Microsoft Teams App](https://docs.microsoft.com/microsoftteams/platform/build-your-first-app/build-first-app-overview)
- [Build a Configurable Tab for Microsoft Teams App](https://docs.microsoft.com/microsoftteams/platform/build-your-first-app/build-channel-tab)
- [Build a Bot](https://docs.microsoft.com/microsoftteams/platform/build-your-first-app/build-bot)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/app-region-selection-python" />