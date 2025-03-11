---
page_type: sample
description: This Microsoft Teams sample app demonstrates single sign-on (SSO) integration for Tabs, Bots, and Messaging Extensions, leveraging Azure AD and MSAL.js.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:25 PM"
urlFragment: officedev-microsoft-teams-samples-app-sso-nodejs
---
# App SSO Node

TA Microsoft Teams sample app demonstrating single sign-on (SSO) integration across Tabs, Bots, and Messaging Extensions using Azure AD and MSAL.js. It supports capabilities like Adaptive Cards, OAuth flow with Azure Bot Service, and making Microsoft Graph calls for various scenarios such as profile retrieval and messaging extensions actions.

__Tab SSO__
This sample shows how to implement Azure AD single sign-on support for tabs. It will

- Obtain an access token for the logged-in user using SSO
- Call a web service - also part of this project - to exchange this access token
- Call Graph and retrieve the user's profile

__Bot, ME SSO__
Bot Framework v4 bot using Teams authentication

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to get started with authentication in a bot for Microsoft Teams.

The focus of this sample is how to use the Bot Framework support for oauth in your bot. Teams behaves slightly differently than other channels in this regard. Specifically an Invoke Activity is sent to the bot rather than the Event Activity used by other channels. _This Invoke Activity must be forwarded to the dialog if the OAuthPrompt is being used._ This is done by subclassing the ActivityHandler and this sample includes a reusable TeamsActivityHandler. This class is a candidate for future inclusion in the Bot Framework SDK.

The sample uses the bot authentication capabilities in [Azure Bot Service](https://docs.botframework.com), providing features to make it easier to develop a bot that authenticates users to various identity providers such as Microsoft Entra ID, GitHub, Uber, etc. The OAuth token is then used to make basic Microsoft Graph queries. Refer the **SSO** setup [documentation](https://docs.microsoft.com/microsoftteams/platform/bots/how-to/authentication/add-authentication?tabs=node-js%2Cnode-js-dialog-sample).

> IMPORTANT: The manifest file in this app adds "token.botframework.com" to the list of `validDomains`. This must be included in any bot that uses the Bot Framework OAuth flow.

## Included Features
* Teams SSO (bots, tabs, messaging extensions, link unfurling)
* Adaptive Cards
* MSAL.js 2.0 support

## Interaction with app

![Preview Image](Images/preview_APP_SSO_Node.gif)

 ## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**App SSO:** [Manifest](/samples/app-sso/csharp/demo-manifest/App-SSO.zip)

## Prerequisites

- To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 16.14.2  or higher).

    ```bash
    # determine node version
    node --version
    ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution

1. A global administrator account for an Office 365 tenant. Testing in a production tenant is not recommended! You can get a free tenant for development use by signing up for the [Office 365 Developer Program](https://developer.microsoft.com/microsoft-365/dev-program) (not a guest account).

2. To test locally, you'll need [Ngrok](https://ngrok.com/) or [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) installed on your development machine.
If you use Ngrok, make sure you've downloaded and installed Ngrok on your local machine. ngrok will tunnel requests from the Internet to your local computer and terminate the SSL connection from Teams.

> NOTE: The free ngrok plan will generate a new URL every time you run it, which requires you to update your Azure AD registration, the Teams app manifest, and the project configuration. A paid account with a permanent ngrok URL is recommended.

- [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

### 1. Setup for Bot
- Setup for Bot SSO
Refer to [Bot SSO Setup document](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-conversation-sso-quickstart/BotSSOSetup.md).

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

- [Install the App in Teams Meeting](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)

 ### 2. Setup NGROK  
1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

### 3. Setup for code   
- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
- In a terminal, navigate to `mples/app-sso/nodejs`

- Install modules & Run the `NodeJS` Server 
    - Server will run on PORT:  `4001`
    - Open a terminal and navigate to project root directory
    
    ```bash
    npm run server
    ```
    > **This command is equivalent to:**
    _npm install > npm run build-client > npm start_

- Install modules & Run the `React` Client
    - Client will run on PORT:  `3978`
    - Open a terminal and navigate to project root directory
    
    ```bash
    npm run client
    ```
      > **This command is equivalent to:** _cd client > npm install > npm start

- Update the `.env` configuration for the bot to use the `MicrosoftAppId` (Microsoft App Id), `MicrosoftAppPassword` (App Password) and `connectionName` (OAuth Connection Name) from the Azure Bot registration. 
    > NOTE: the App Password is referred to as the `client secret` in the azure portal and you can always create a new client secret anytime.

**Bot Configuration:**

![BotConfg](Images/BotConfg.png)

**Bot OAuth Connection:**

![Bot Connections](Images/BotConnections.png)

### 4. Register your Teams Auth SSO with Azure AD

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.
4. Under **Manage**, select **Expose an API**. 
5. Select the **Set** link to generate the Application ID URI in the form of `api://{AppID}`. Insert your fully qualified domain name (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: `api://fully-qualified-domain-name/botid-{AppID}`
    * ex: `api://%ngrokDomain%.ngrok-free.app/botid-00000000-0000-0000-0000-000000000000`.
6. Select the **Add a scope** button. In the panel that opens, enter `access_as_user` as the **Scope name**.
7. Set **Who can consent?** to `Admins and users`
8. Fill in the fields for configuring the admin and user consent prompts with values that are appropriate for the `access_as_user` scope:
    * **Admin consent title:** Teams can access the user’s profile.
    * **Admin consent description**: Allows Teams to call the app’s web APIs as the current user.
    * **User consent title**: Teams can access the user profile and make requests on the user's behalf.
    * **User consent description:** Enable Teams to call this app’s APIs with the same rights as the user.
9. Ensure that **State** is set to **Enabled**
10. Select **Add scope**
    * The domain part of the **Scope name** displayed just below the text field should automatically match the **Application ID** URI set in the previous step, with `/access_as_user` appended to the end:
        * `api://[ngrokDomain].ngrok-free.app/00000000-0000-0000-0000-000000000000/access_as_user.
11. In the **Authorized client applications** section, identify the applications that you want to authorize for your app’s web application. Each of the following IDs needs to be entered:
    * `1fec8e78-bce4-4aaf-ab1b-5451cc387264` (Teams mobile/desktop application)
    * `5e3ce6c0-2b1f-4285-8d4b-75ee78787346` (Teams web application)
**Note** If you want to test or extend your Teams apps across Office and Outlook, kindly add below client application identifiers while doing Azure AD app registration in your tenant:
   * `4765445b-32c6-49b0-83e6-1d93765276ca` (Office web)
   * `0ec893e0-5785-4de6-99da-4ed124e5296c` (Office desktop)
   * `bc59ab01-8403-45c6-8796-ac3ef710b3e3` (Outlook web)
   * `d3590ed6-52b3-4102-aeff-aad2292ab01c` (Outlook desktop)      
12. Navigate to **API Permissions**, and make sure to add the follow permissions:
-   Select Add a permission
-   Select Microsoft Graph -\> Delegated permissions.
    * User.Read (enabled by default)
    * email
    * offline_access
    * OpenId
    * profile
-   Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
![APIpermissions](Images/APIpermissions.png)
13. Navigate to **Authentication**
    If an app hasn't been granted IT admin consent, users will have to provide consent the first time they use an app.
    Set a redirect URI:
    * Select **Add a platform**.
    * Select **web**.
    * Enter the **redirect URI** for the app in the following format: 
    1) https://%ngrokDomain%.ngrok-free.app/Auth/End
    2) https://token.botframework.com/.auth/web/redirect
    
    Enable implicit grant by checking the following boxes:  
    ✔ ID Token  
    ✔ Access Token  
![Authentication](Images/Authentication.png)
14.  Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description      (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json.

### 6. Setup Manifest for Teams

- **This step is specific to Teams.**
    - **Edit** the `manifest.json` contained in the  `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`) also update the `<<DOMAIN-NAME>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `app-sso\nodejs` folder with the required values.
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip` or `Manifest_Hub` 
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

**Note**: This `manifest.json` specified that the bot will be installed in a "personal" scope only. Please refer to Teams documentation for more details.

- If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/app-sso/nodejs/server/api/botController.js#L32) line and put your debugger for local debug.

## Running the sample

You can interact with this bot by sending it a message. The bot will respond by requesting you to login to Microsoft Entra ID, then making a call to the Graph API on your behalf and returning the results.

**Install App:**

![InstallApp](Images/add_app.png)

**Welcome Card:**

![WelcomeCard](Images/WelcomeCard.png)

- Type *anything* on the compose box and send
- The bot will perform `Single Sign-On` and Profile card will be displayed along with the option prompt to view the `token`

![SingleSignIn](Images/Single_SignIn.png)

**Would you like to view your token:**

![TokeYesOrNo](Images/TokeYesOrNo.png)

**Click token Yes:**

![TokenYes](Images/TokenYes.png)

**Open Messaging Extension (Search), it will show profile details:**

![MELogin](Images/MELogin.png)

**Open App SSO**
![MEProfile](Images/MEProfile.png)

**Open Messaging Extension (Action), it will show profile details:**

![MEProfile2](Images/MEProfile2.png)

**Click profile UI:**

![Profile](Images/Profile.png)

**Select profile UI:**

![ProfileAction](Images/profile_action.png)

**Click profile UI:**

![ClickProfileDetails](Images/ClickProfile_details.png)

**Open Messaging Extension (linkunfurl), The link will unfurl and show profile details:**

**Paste** https://profile.botframework.com on the compose box

![MECompose](Images/MECompose.png)

**Open SSO Tab Continue and then Accept and it'll show the profile details:**

![Tab](Images/Tab.png)

**Install app other tenant:**

![InstallAppSecondUser](Images/InstallAppSecondUser.png)

> NOTE: If `SSO` couldn't be performed then it will fallback to normal Authentication method and you will get a default `Sign In` action

**Consent the ME Search by clicking the Sign In link like below:**

![MESignIn](Images/ME_SignIn.png)

**Consent the ME Action by clicking the Setup button like below:**

![MESignIn1](Images/ME_SignIn1.png)
## Outlook on the web

- To view your app in Outlook on the web.

- Go to [Outlook on the web](https://outlook.office.com/mail/)and sign in using your dev tenant account.

**On the side bar, select More Apps. Your sideloaded app title appears among your installed apps**

![InstallOutlook](Images/InstallOutlook.png)

**Select your app icon to launch and preview your app running in Outlook on the web**

![AppOutlook](Images/AppOutlook.png)

**Note:** Similarly, you can test your application in the Outlook desktop app as well.

## Office on the web

- To preview your app running in Office on the web.

- Log into office.com with test tenant credentials

**Select the Apps icon on the side bar. Your sideloaded app title appears among your installed apps**

![InstallOffice](Images/InstallOffice.png)

**Select your app icon to launch your app in Office on the web**

![AppOffice](Images/AppOffice.png) 

**Note:** Similarly, you can test your application in the Office 365 desktop app as well.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [SSO for Bot](https://docs.microsoft.com/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots)
- [SSO for Messaging Extensions](https://docs.microsoft.com/microsoftteams/platform/messaging-extensions/how-to/enable-sso-auth-me)
- [SSO for Tab](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso)
- [Azure Portal](https://portal.azure.com)
- [Add Authentication to Your Bot Via Azure Bot Service](https://docs.microsoft.com/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Extend Teams apps across Microsoft 365](https://learn.microsoft.com/microsoftteams/platform/m365-apps/overview)



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/app-sso-nodejs" />