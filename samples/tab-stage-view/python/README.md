---
page_type: sample
description: This sample app demonstrates the use of Teams tab in stage view using Python, featuring collaborative elements and interactive capabilities.
products:
- office-teams
- office
- office-365
languages:
- python
extensions:
 contentType: samples
 createdDate: "06/10/2025"
urlFragment: officedev-microsoft-teams-samples-tab-stage-view-python
---

# Tab Stage View

This sample app showcases the capabilities of Microsoft Teams tabs in stage view using Python. It demonstrates collaborative features such as multi-window support and interactive elements, allowing users to engage dynamically through adaptive cards and deep linking for a richer experience in Teams.

For reference, see [Tabs link unfurling and Stage View](https://docs.microsoft.com/microsoftteams/platform/tabs/tabs-link-unfurling).

## Included Features
* Bots
* Stage View (tabs)
* Collaborative Stageview

## Interaction with app

![Tab Stage ViewGif](Images/TabStageView.gif)

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

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because the Teams service needs to call into the bot.

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

3) Create [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Azure free account here](https://azure.microsoft.com/free/)

4) In a terminal, go to `samples/tab-stage-view/python`

5) Activate your desired virtual environment

6) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

7) Update the `config.py` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

8) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `${{AAD_APP_CLIENT_ID}}` and `${{TEAMS_APP_ID}}`
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

9) Run your bot with `python app.py`

## Running the sample

- From Teams left side bar, select the ellipses ●●● and choose your app from the list.

**Install App:**  
![InstallApp](Images/1.Install.png)

**Welcome message with feature explanation and Adaptive Card with actions:**  
![Welcome Message](Images/WelcomeMessage.png)

**Click view via card action:**  
![Stage View in tab](Images/ViewViaCard.png)

**Click view via deeplink:**  
![Tab View](Images/ViewViaDeepLink.png)

**Tab with execute deep link action to open stage view:**  
![Tab View](Images/TabStageView.png)

**Click execute deep-link:**  
![Tab View](Images/TabDeepLink.png)

- To view your app in Outlook on the web, go to [Outlook on the web](https://outlook.office.com/mail/) and sign in using your dev tenant account.
- On the side bar, select More Apps. Your sideloaded app title appears among your installed apps

![InstallOutlook](Images/InstallOutlook.png)
![AppOutlook](Images/AppOutlook.png)

## Office on the web

- Log into office.com with test tenant credentials
- Select the Apps icon on the side bar. Your sideloaded app title appears among your installed apps

![InstallOffice](Images/InstallOffice.png)
![AppOffice](Images/AppOffice.png)

**After opening Outlook web, click the "New mail" button.**  
![Open New Mail](Images/OpenNewMail.png)

**On the tool bar on top, select Apps icon. Your sideloaded app title appears among your installed apps**  
![OpenAppIcon](Images/OpenAppIcon.png)

**Opening stage view from unfurling link. If you copy and paste a link from https://tabstageview.com/card into the compose message area the link will unfurl.**  
![Outlook Unfurling](Images/OutlookUnfurling.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Tabs](https://learn.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
- [Stage view](https://learn.microsoft.com/microsoftteams/platform/tabs/tabs-link-unfurling#stage-view)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-stage-view-python" />