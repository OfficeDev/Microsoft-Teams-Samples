---
page_type: sample
description: This sample application illustrates how to create channel and group tabs in Microsoft Teams, enabling collaborative spaces for web-based content. It includes setup instructions, prerequisites, and features for embedding Teams-aware webpages directly into your Teams environment.
products:
- office-teams
- office
- office-365
languages:
- python
- flask
extensions:
 contentType: samples
 createdDate: "06/16/2025 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-channel-group-quickstart-python
---

# Tab Channel quick start Python

This sample application illustrates how to create channel and group tabs in Microsoft Teams, enabling collaborative spaces for web-based content. It includes setup instructions, prerequisites, and features for embedding Teams-aware webpages directly into your Teams environment using Python Flask.

## Included Features
* Tabs

## Interaction with app

![Sample Module](Images/4.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Tab Channel quick start:** [Manifest](/samples/tab-channel-group-quickstart/python/demo-manifest/tab-channel-group-quickstart.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- To test locally, [Python](https://www.python.org/downloads/) must be installed on your development machine (version 3.8 or higher)
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunneling solution
- [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

1) App Registration

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

2) Setup NGROK
 - Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3. Setup for code
- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Install dependencies

    ```bash
    pip install -r requirements.txt
    ```
- Run your app

    ```bash
    python app.py
    ```

4. Setup Manifest for Teams

- **This step is specific to Teams.**
    - Edit the `manifest.json` contained in the `appManifest/` folder to replace with your MicrosoftAppId (that was created in step1.1 and is the same value of MicrosoftAppId in `.env` file) *everywhere* you see the place holder string `${{TEAMS_APP_ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `${{TAB_DOMAIN}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - Zip up the contents of the `appManifest/` folder to create a `manifest.zip`
    - Upload the `manifest.zip` to Teams (in the left-bottom *Apps* view, click "Upload a custom app")

## SSL Certificate Setup (HTTPS)

Teams integration requires HTTPS. The app automatically detects SSL certificates and runs in HTTPS mode if available.

### Quick Setup Options:

1. **Environment Variables:**
   ```bash
   SSL_CRT_FILE=/path/to/certificate.crt
   SSL_KEY_FILE=/path/to/private.key
   ```

2. **Default Location:** Place certificates in `./certs/` directory:
   ```
   ./certs/localhost.crt
   ./certs/localhost.key
   ```

3. **Generate Self-Signed (Development):**
   ```bash
   mkdir certs
   openssl req -x509 -newkey rsa:2048 -keyout ./certs/localhost.key -out ./certs/localhost.crt -days 365 -nodes -subj "/CN=localhost"
   ```

4. **Teams Toolkit:** Copy existing certificates:
   ```bash
   cp ~/.fx/certificate/localhost.* ./certs/
   ```

**Note:** App falls back to HTTP mode if certificates not found.

## Deploy to Teams
Start debugging the project by hitting the `F5` key or click the debug icon in Visual Studio Code and click the `Start Debugging` green arrow button.

### NOTE: First time debug step
On the first time running and debugging your app you need to allow the localhost certificate. After starting debugging when Chrome is launched and you have installed your app it may fail to load.

- Open a new tab `in the same browser window that was opened`
- Navigate to `https://localhost:3978/tab`
- Click the `Advanced` button
- Select the `Continue to localhost`

### NOTE: Debugging
Ensure you have the Python extension installed for Visual Studio Code from the marketplace.



## Running the sample

![tabconfigure](Images/1.png)

![setuptab](Images/2.png)

!(Images/3.png)


## Further Reading

[Create a group tab](https://learn.microsoft.com/microsoftteams/platform/tabs/how-to/create-channel-group-tab?pivots=node-java-script)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-channel-group-quickstart-python" />