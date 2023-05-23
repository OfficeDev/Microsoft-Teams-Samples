---
page_type: sample
description: This sample app demonstrate iss how to use the Bot Framework support for oauth in your bot
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "28/02/2023 13:38:25 PM"
urlFragment: officedev-microsoft-teams-samples-bot-teams-authentication-nodejs
---
# Teams Auth Bot

Bot Framework v4 bot using Teams authentication

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to get started with authentication in a bot for Microsoft Teams.

The focus of this sample is how to use the Bot Framework support for oauth in your bot. Teams behaves slightly differently than other channels in this regard. Specifically an Invoke Activity is sent to the bot rather than the Event Activity used by other channels. _This Invoke Activity must be forwarded to the dialog if the OAuthPrompt is being used._ This is done by subclassing the ActivityHandler and this sample includes a reusable TeamsActivityHandler. This class is a candidate for future inclusion in the Bot Framework SDK.

The sample uses the bot authentication capabilities in [Azure Bot Service](https://docs.botframework.com), providing features to make it easier to develop a bot that authenticates users to various identity providers such as Azure AD (Azure Active Directory), GitHub, Uber, etc. The OAuth token is then used to make basic Microsoft Graph queries.

> IMPORTANT: The manifest file in this app adds "token.botframework.com" to the list of `validDomains`. This must be included in any bot that uses the Bot Framework OAuth flow.

- **Interaction with the bot**
![bot-teams-auth ](Images/bot-teams-auth.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Teams Auth Bot:** [Manifest](/samples/bot-teams-authentication/csharp/demo-manifest/bot-teams-authentication.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- [Node.js](https://nodejs.org)
   ```bash
    # determine node version
    node --version
    ```
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## Setup
### 1. Setup for App Registration

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call into the bot.

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the `.env`.
4. Navigate to **API Permissions**, and make sure to add the follow permissions:
    -   Select Add a permission
    -   Select Microsoft Graph -\> Delegated permissions.
        - `User.Read` (enabled by default)
        - `openid`
    -   Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
5. Navigate to **Authentication**
    If an app hasn't been granted IT admin consent, users will have to provide consent the first time they use an app.
- Set another redirect URI:
    * Select **Add a platform**.
    * Select **web**.
    * Enter the **redirect URI** `https://token.botframework.com/.auth/web/redirect`. This will be use for bot authenticaiton. 
6.  Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description(Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the `.env`.

7. Create a Bot Registration
   - Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
   - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
   - While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
   - Select Configuration section.
   - Under configuration -> Add OAuth connection string.
   - Provide connection Name : for eg `ConnectTeamsAuthentication`
   - Select service provider ad `Azure Active Directory V2`
   - Complete the form as follows:

     1) **Name:** Enter a name for the connection. You'll use this name in your bot in the .env file.
     2) **Client id:** Enter the Application (client) ID that you recorded for your Azure identity provider app in the steps above.
     3) **Client secret:** Enter the secret that you recorded for your Azure identity provider app in the steps above.
     4) **Tenant ID:**  Enter the Application (tenant) ID that you recorded for your Azure identity provider app in the steps above.
     5) **Token Exchange Url:** Leave it blank because it's used for SSO in Azure AD v2 only.
     6) Provide **Scopes** like "User.Read openid"

8. Setup NGROK
    - Run ngrok - point to port 3978
	```bash
	# ngrok http 3978 --host-header="localhost:3978"
	```   
9. Setup for code

   - Clone the repository
    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
  - In a terminal, navigate to `samples\bot-teams-authentication\nodejs` folder

  - Install modules
    
     ```bash
    npm install
    ```
  - Modify the `.env` file in your project folder (or in Visual Studio Code) and fill in below details:
       1) `<<YOUR-MICROSOFT-APP-TYPE>>` (**Allowed values are: MultiTenant(default), SingleTenant, UserAssignedMSI**)
       2) `<<YOUR-MICROSOFT-APP-ID>>` - Generated from Step 1 (Application (client) ID)is the application app id
       3) `<<YOUR-MICROSOFT-APP-PASSWORD>>` - Generated from Step 6, also referred to as Client secret
       4) `<<YOUR-MICROSOFT-APP-TENANT-ID>>` - Generated from Step 1(Directory (tenant) ID) is the tenant id
       5) `<<YOUR-CONNECTION-NAME>>` - Generated from step 7.

  - Run your app

    ```bash
    npm start
    ```
1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the MicrosoftAppId may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `{{domain-name}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal scope or 1:1 chat (Supported scope)

**Note:**
-   If you are facing any issue in your app,  [please uncomment this line](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-teams-authentication/nodejs/index.js#L52) and put your debugger for local debug.

## Running the sample

**Install app:**

![add-App ](Images/add-App.png)

**Welcome to teamsbot:**

![added-App ](Images/added-App.png)

**Login UI:**

![auth-login ](Images/auth-login.png)

**Authentication success:**

![auth-Success ](Images/auth-Success.png)

**Authentication token:**

![auth-Token ](Images/auth-Token.png)

**Logout UI:**

![logout ](Images/logout.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Bot Authentication Basics](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/bot-sso-overview)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-teams-authentication-nodejs" />