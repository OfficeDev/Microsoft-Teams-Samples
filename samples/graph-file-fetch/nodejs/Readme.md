---
page_type: sample
description: This sample demonstrates how to fetch shared images and files in Microsoft Teams groupchats and team channels using graph API.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "28/05/2025 23:35:25 PM"
urlFragment: officedev-microsoft-teams-samples-graph-file-fetch-nodejs
---

# Install app using barcode sample

This sample demonstrates how to fetch shared images and files in Microsoft Teams groupchats and team channels using graph API. While leveraging features such as bot interaction, Teams SSO, adaptive cards.

`Currently, Microsoft Teams support for QR or barcode scanner capability is only available for mobile clients`

## Included Features
* Bot
* Teams SSO (bots)
* Graph API

## Interaction with bot - Desktop View

![App Installation Using QRCodeDesktopGif](Images/graph-file-fetch.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Graph File Fetch:** [Manifest](/samples/graph-file-fetch/nodejs/demo-manifest/graph-file-fetch.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 16.14.2  or higher)
-  [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the 
   appropriate permissions to install an app.
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

### Register your app with Azure AD.

  1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  2. Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
  4. Navigate to **API Permissions**, and make sure to add the follow permissions:
   Select Add a permission
      * Select Add a permission
      * Select Microsoft Graph -\> Delegated permissions.
      * `User.Read` (enabled by default)
      * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.


## Setup

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call into the bot.

1) Setup for Bot SSO
- Refer to [Bot SSO Setup document](../BotSSOSetup.md)

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the Azure bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
    
    > NOTE: When you create your app registration in Azure portal, you will create an App ID and App password - make sure you keep these for later.

2) Setup for code
- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- In a terminal, navigate to `samples/graph-file-fetch/nodejs`

- Install modules

    ```bash
    npm install
    ```

- Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    ```
   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

- Update the `.env` configuration file for the bot to use the `MicrosoftAppId` and `MicrosoftAppPassword` from the Microsoft Entra ID app registration in Azure portal or from the Bot Framework registration. (Note that the MicrosoftAppId is the AppId created in step 1 (Setup for Bot SSO), the MicrosoftAppPassword is referred to as the "client secret" in step 1 (Setup for Bot SSO) and you can always create a new client secret anytime.)
    - Also, update `connectionName` as the name of your Azure Bot connection created in previous steps.
    - `connectionName` - The OAuthConnection setting from step 1, from Azure Bot SSO setup.
    - `BaseUrl` with application base url. For e.g., your ngrok url https://xxx.ngrok-free.app, your dev tunnels url https://xxx.devtunnels.ms.

- Run your app

    ```bash
    npm start
    ```

3) **Manually update the manifest.json**
    - Edit the `manifest.json` contained in the  `appManifest/` folder to replace with your MicrosoftAppId (that was created in step1 and is the same value of MicrosoftAppId in `.env` file) *everywhere* you see the place holder string `{{MicrosoftAppId}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - `{{domain-name}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - Zip up the contents of the `appManifest/` folder to create a `manifest.zip`
    - Upload the `manifest.zip` to Teams (in the left-bottom *Apps* view, click "Upload a custom app")

    > IMPORTANT: The manifest file in this app adds "token.botframework.com" to the list of `validDomains`. This must be included in any bot that uses the Bot Framework OAuth flow.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-file-fetch/nodejs/index.js#L48) line and put your debugger for local debug.

## Running the sample

**Install App:**

![Install App](Images/1.Install.png)

**Select Scope:**

![Select Scope](Images/2.SelectScope.png)

**Welcome Message:**

![Welcome Message](3.WelcomeMessage.png)

**Oauth Prompt:**

![Oauth Prompt](Images/4.OauthPrompt.png)

**LoggedIn:**

![LoggedIn](Images/5.LoggedIn.png)

**Uploading File To Teams GroupChat:**

![Uploading File To Teams GroupChat](Images/6.UploadingFileToTeamsGroupChat.png)

**Bot Responds With FIle Link GroupChat:**

![Bot Responds With FIle Link GroupChat](Images/7.BotRespondsWithFIleLinkGroupChat.png)

**Uploading File To Teams Channel:**

![Uploading File To Teams Channel](Images/8.UploadingFileToTeamsChannel.png)

**Bot Responds With File Link Teams Channel:**

![Bot Responds With File Link Teams Channel](Images/9.BotRespondsWithFIleLinkTeamsChannel.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
