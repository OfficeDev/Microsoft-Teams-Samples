# App SSO Node

This app talks about the Teams Tab, Bot, ME - search, action, linkunfurl SSO with Node JS

__Tab SSO__
This sample shows how to implement Azure AD single sign-on support for tabs. It will

- Obtain an access token for the logged-in user using SSO
- Call a web service - also part of this project - to exchange this access token
- Call Graph and retrieve the user's profile

__Bot, ME SSO__
Bot Framework v4 bot using Teams authentication

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to get started with authentication in a bot for Microsoft Teams.

The focus of this sample is how to use the Bot Framework support for oauth in your bot. Teams behaves slightly differently than other channels in this regard. Specifically an Invoke Activity is sent to the bot rather than the Event Activity used by other channels. _This Invoke Activity must be forwarded to the dialog if the OAuthPrompt is being used._ This is done by subclassing the ActivityHandler and this sample includes a reusable TeamsActivityHandler. This class is a candidate for future inclusion in the Bot Framework SDK.

The sample uses the bot authentication capabilities in [Azure Bot Service](https://docs.botframework.com), providing features to make it easier to develop a bot that authenticates users to various identity providers such as Azure AD (Azure Active Directory), GitHub, Uber, etc. The OAuth token is then used to make basic Microsoft Graph queries. Refer the **SSO** setup [documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/add-authentication?tabs=node-js%2Cnode-js-dialog-sample).

> IMPORTANT: The manifest file in this app adds "token.botframework.com" to the list of `validDomains`. This must be included in any bot that uses the Bot Framework OAuth flow.

## Prerequisites

1. A global administrator account for an Office 365 tenant. Testing in a production tenant is not recommended! You can get a free tenant for development use by signing up for the [Office 365 Developer Program](https://developer.microsoft.com/en-us/microsoft-365/dev-program) (not a guest account).

2. To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 10.14 or higher).

    ```bash
    # determine node version
    node --version
    ```

3. To test locally, you'll need [Ngrok](https://ngrok.com/) installed on your development machine.
Make sure you've downloaded and installed Ngrok on your local machine. ngrok will tunnel requests from the Internet to your local computer and terminate the SSL connection from Teams.

> NOTE: The free ngrok plan will generate a new URL every time you run it, which requires you to update your Azure AD registration, the Teams app manifest, and the project configuration. A paid account with a permanent ngrok URL is recommended.

4. Required Permissions
        * Make sure you have the following Graph permissions enabled: `email`, `offline_access`, `openid`, `profile`, and `User.Read` (default).
        * For permissions `Manage > API Permissions`
        * Our SSO flow will give you access to the first 4 permissions, and we will have to exchange the token server-side to get an elevated token for the `profile` permission (for example, if we want access to the user's profile photo).
        
![image](https://user-images.githubusercontent.com/85108465/121638666-f80f4980-caa8-11eb-9b75-09b0e86c6d6a.png)

## To try this sample

1. Setup for Bot SSO
Refer to [Bot SSO Setup document](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-conversation-sso-quickstart/BotSSOSetup.md).
    >**Important**: As we are building app with Bot & Tab in [Step 1.3](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-conversation-sso-quickstart/BotSSOSetup.md#13-expose-api-endpoint) change the `api://botid-{YourBotId` to  `api://fully-qualified-domain-name.com/botid-{YourBotId}`
    >
    >**Sample Application Id URI:** `api://43dfa1bc0d1e.ngrok.io/botid-eddbe35e-4878-99d2-.......946c4aac7`

1. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

1. In a console, navigate to `samples/app-sso/nodejs`

    ```bash
    cd samples/app-sso/nodejs
    ```

1. Run ngrok - point to port `3978`

    ```bash
    ngrok http -host-header=localhost 3978
    ```


1. Update the `.env` configuration for the bot to use the `MicrosoftAppId` (Microsoft App Id), `MicrosoftAppPassword` (App Password) and `connectionName` (OAuth Connection Name) from the Bot Framework registration. 
    > NOTE: the App Password is referred to as the `client secret` in the azure portal and you can always create a new client secret anytime.


1. Install modules & Run the `NodeJS` Server 
    - Server will run on PORT:  `4001`
    - Open a terminal and navigate to project root directory
    
    ```bash
    npm run server
    ```
    
    > **This command is equivalent to:**
    _npm install > npm run build-client > npm start_

1. Install modules & Run the `React` Client
    - Client will run on PORT:  `3978`
    - Open a terminal and navigate to project root directory
    
    ```bash
    npm run client
    ```
    
      > **This command is equivalent to:** _cd client > npm install > npm start_

      > **NOTE:** 
        You might see an error _sometimes_ like below but it shouldn't be a problem if your Server is running on PORT `4001`
        ![image](https://user-images.githubusercontent.com/85108465/122787796-502d2380-d2d3-11eb-832f-b50d317a4869.png)

1. __*Teams manifest changes.*__
    - **Edit** the `manifest.json` contained in the  `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`) also update the `<<DOMAIN-NAME>>` with the ngrok URL
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")


## Interacting with the bot in Teams
> Note: This `manifest.json` specified that the bot will be installed in a "personal" scope only. Please refer to Teams documentation for more details.

You can interact with this bot by sending it a message. The bot will respond by requesting you to login to AAD, then making a call to the Graph API on your behalf and returning the results.
- Install App

![image](https://user-images.githubusercontent.com/85108465/121664976-acb76400-cac5-11eb-8256-53a8d7980476.png)

- Type *anything* on the compose box and send
- The bot will perform `Single Sign-On` and Profile card will be displayed along with the option prompt to view the `token`

![image](https://user-images.githubusercontent.com/85108465/122400808-ae879880-cf99-11eb-8cc2-5da9b8e420ca.png)

> **NOTE:** 
If the user is using the application for the first time and user consent is required for additional permissions, the following dialog box appears to continue with the consent experience
![image](https://user-images.githubusercontent.com/85108465/122513983-c3f7d380-d028-11eb-913c-1794c5ff851a.png)
![image](https://user-images.githubusercontent.com/85108465/122557279-3503af00-d05a-11eb-981d-bf8db77ff2ac.png)

>If the bot couldn't perform `SSO` then it will fallback to normal Authentication method and show a `Sign In` card like below
![image](https://user-images.githubusercontent.com/85108465/122401855-9a906680-cf9a-11eb-94f1-87840c6662b4.png)

- Open `Messaging Extension`(Search), it will show profile details

![image](https://user-images.githubusercontent.com/85108465/121668748-3ddc0a00-cac9-11eb-8c0e-cc3d60f2b5a8.png)
![image](https://user-images.githubusercontent.com/85108465/121669133-a32ffb00-cac9-11eb-9427-50321a317550.png)
![image](https://user-images.githubusercontent.com/85108465/121669220-bc38ac00-cac9-11eb-982e-880ebb8258a8.png)

- Open `Messaging Extension`(Action), it will show profile details

![image](https://user-images.githubusercontent.com/85108465/121669676-2f422280-caca-11eb-8659-3f8f08d11bbd.png)

__*or*__

![image](https://user-images.githubusercontent.com/85108465/121669730-3e28d500-caca-11eb-9ba5-fca1b97e7f20.png)
![image](https://user-images.githubusercontent.com/85108465/121669695-32d5a980-caca-11eb-9893-2d3a3a3b1821.png)

- Open `Messaging Extension`(linkunfurl), The link will unfurl and show profile details

**Paste** https://profile.botframework.com on the compose box

![image](https://user-images.githubusercontent.com/85108465/121669972-93fd7d00-caca-11eb-87bb-e07e0e7aa5e4.png)
![image](https://user-images.githubusercontent.com/85108465/121669990-98c23100-caca-11eb-9a31-30c3d5065853.png)

> NOTE: If `SSO` couldn't be performed then it will fallback to normal Authentication method and you will get a default `Sign In` action

Consent the *ME Search* by clicking the `Sign In` link like below 

![image](https://user-images.githubusercontent.com/85108465/121671255-f2772b00-cacb-11eb-9321-1317696eaccc.png)

Consent the *ME Action* by clicking the `Setup` button like below 

![image](https://user-images.githubusercontent.com/85108465/122556633-6039ce80-d059-11eb-9fa5-e0fe4db4939d.png)

- Open `SSO Tab`, Continue and then Accept and it'll show the profile details

![image](https://user-images.githubusercontent.com/85108465/121671560-5568c200-cacc-11eb-954b-44155e039915.png)
![image](https://user-images.githubusercontent.com/85108465/121671603-61ed1a80-cacc-11eb-9754-ff0b2aaac671.png)
![image](https://user-images.githubusercontent.com/85108465/121672009-e344ad00-cacc-11eb-860f-c0ed5153edc7.png)



## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [SSO for Bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots)
- [SSO for Messaging Extensions](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/enable-sso-auth-me)
- [SSO for Tab](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso)
- [Azure Portal](https://portal.azure.com)
- [Add Authentication to Your Bot Via Azure Bot Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [dotenv](https://www.npmjs.com/package/dotenv)
- [Microsoft Teams Developer Platform](https://docs.microsoft.com/en-us/microsoftteams/platform/)
