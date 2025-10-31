---
page_type: sample
description: This sample app demonstrates how to implement a Details Tab in Microsoft Teams meetings, allowing users to create polls and gather participant feedback through interactive chats.
products:
- office-teams
- office
- office-365
languages:
- python
extensions:
 contentType: samples
 createdDate: "07-28-2025 10:00:00"
urlFragment: officedev-microsoft-teams-samples-meetings-details-tab-python
---

# Meeting Details Tab Sample Python Backend

This sample demonstrates how to extend Microsoft Teams meetings by implementing a Details Tab that allows users to create and manage polls. Participants can submit their feedback through adaptive cards, and the results are easily viewable in both the meeting chat and the tab itself. Built with Python Flask backend and Teams Bot Framework, this app provides a seamless polling experience within Teams meetings.

## Included Features
* Tabs
* Bots
* Adaptive Cards
* Flask Web Server
* Teams Bot Framework

## Interaction with app

![PreviewImage](Images/Preview.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Meetings Details Tab Sample:** [Manifest](/samples/meetings-details-tab/python-backend/demo-manifest/meetings-details-tab.zip)

## Prerequisites

- [Python](https://python.org) version 3.8 or higher

    ```bash
    # determine python version
    python --version
    ```
      
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [Ngrok](https://ngrok.com/download) (Only for devbox testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

## Setup

1. App Registration

### Register your application with Azure AD

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
4. Navigate to **API Permissions**, and make sure to add the follow permissions:
    * Select Add a permission
    * Select Microsoft Graph -> Delegated permissions.
    * `User.Read` (enabled by default)
    * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

2. Setup for Bot
	- Register a Microsoft Entra ID app registration in Azure portal.
	- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK or Dev Tunnels
 - Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

4. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

5. Setup Python Environment

    - Navigate to the Python backend directory
    
        ```bash
        cd samples/meetings-details-tab/python-backend
        ```

    - Create a virtual environment (recommended)
    
        ```bash
        python -m venv venv
        ```

    - Activate the virtual environment
    
        ```bash
        # Windows
        venv\Scripts\activate
        
        # macOS/Linux
        source venv/bin/activate
        ```

    - Install required packages
    
        ```bash
        pip install -r requirements.txt
        ```

    - Create a `.env` file in the root directory and add the following variables:
    
        ```
        MicrosoftAppId=<Your Bot App ID>
        MicrosoftAppPassword=<Your Bot App Password>
        BaseUrl=<Your tunnel URL>
        ```

    - Start the Python Flask server
    
        ```bash
        python app.py
        ```
        
        The server will run on PORT: `3978`

**Note**: If you are facing any issue in your app, please uncomment debugging lines in `server/bot/bot_activity_handler.py` and add breakpoints for local debugging.

6. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appManifest folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appManifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.



## Running the sample

Interact with Details Tab in Meeting.

**Add Chat:**
![InstallApp](Images/1.InstallApp.png)

**Select Group:**
![GroupChat](Images/2.GroupChat.png)

**Newly added agenda will be added to Tab:**
![DetailsTab](Images/3.DetailsTab.png)

**Click on Add Agenda:**
![AddAgenta](Images/4.AddAgenta.png)

**Add a Poll:**
![CreatePoll](Images/5.CreatePoll.png)

**Participants in meeting can submit their response in adaptive card:**
![HomeAddPoll](Images/6.HomeAddPoll.png)

**Response will be recorded and Bot will send an new adaptive card with response:**
![SelectHot](Images/7.SelectHot.png)

**Participants in meeting can view the results from meeting chat or Tab itself:**
![8.Results](Images/8.Results.png)


## Further reading

- [Build tabs for meeting](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/build-tabs-for-meeting?tabs=desktop)
- [Meeting apps APIs](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet)
- [Meeting Side Panel](https://learn.microsoft.com/microsoftteams/platform/sbs-meetings-sidepanel?tabs=vs)
- [Install the App in Teams Meeting](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)
- [Flask Documentation](https://flask.palletsprojects.com/)
- [Bot Framework Python SDK](https://docs.microsoft.com/azure/bot-service/python/bot-builder-python-quickstart?view=azure-bot-service-4.0)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meetings-details-tab-python" />