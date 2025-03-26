---
page_type: sample
products:
- office-365
languages:
- javascript
title: Microsoft Teams NodeJS Bot Consent Sample
description: Microsoft Teams bot consent sample app in Node.js
extensions:
  contentType: samples
  createdDate: 11/3/2017 12:53:17 PM
---

# Microsoft Teams Azure AD Consent Bot Sample.

This sample demonstrates how to handle Azure AD Consent when you are in a conversation with a Microsoft Teams bot.

[Teams Bot SSO](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots)

## More Information

This Teams Bot Sample demonstrates how to handle Azure AD Consent, when required. It uses special Action Types in Bot Adaptive Cards, to open a modal pop-out window (not an iFrame), which then will allow you to use MSAL to get consent from the user.
Once the user has consented succesfully, we are then able to handle this in the bot, and provide the user with a profile card, that is populated using information that was gathered from Graph API about the logged in user.

Once granted, if you want to revoke consent, log in to the [Azure Portal](https://portal.azure.com/) as an administrator, in the tenant where you are messaging this bot from, go to Azure AD/Enterprise Apps, find the application, go to Properties, and then delete the Enterprise App.
After a minute or 2, the consent that was previously provided will be revoked, and if you send a 'hello' to the bot and ask it to get your profile, you will be walked through the consent process again.

To get started with this sample, read through the rest of this readme file, and you will be guided through the steps required to setup Azure AD and the Bot Service to make this sample work.

## Getting started

1. Install some sort of tunnelling service. These instructions assume you are using ngrok: https://ngrok.com/
1. Begin your tunnelling service to get an https endpoint. For this example ngrok is used. Start an ngrok tunnel with the following command (you'll need the https endpoint for the bot registration):<br>

    ```bash
    ngrok http 5000 --host-header=localhost
    ```

1. Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

> **IMPORTANT**: Do not use the legacy Bot Framework portal, nor App Studio, to create the bot. Your bot MUST be registered with
> Azure Bot Service to use the authentication functionality provided by Azure Bot Service.

4. Create an app manifest. Navigate to the file, manifest/manifest.json - Change:
    1. <<REGISTERED_BOT_ID>> (there are 3) change to your registered bot's app ID (NOTE: This is the same as the MicrosoftAppID)
    1. <<BASE_URI_DOMAIN>> (there are 2) change to your https endpoint from ngrok excluding the "https://" part
    1. Save the file and zip this file and the 2 .png files (located next in the same directory as the manifest file) together to create a manifest.zip file

## Setup

To be able to use an identity provider, first you have to register your application.

### Changing app settings


This sample can be run locally but you'll need to set up some environment variables. There are many ways to do this, but the easiest, if you are using Visual Studio Code, is to add a `.env` file in the src directory with the following content:

```
MicrosoftAppId=00000000-0000-0000-0000-000000000000
MicrosoftAppPassword=YourBotAppPassword
BaseUrl=https://########.ngrok.io
OAuthConnectionName=AAD
```

Where:

    1. `MicrosoftAppId` - This is the Azure AD Client (application) ID that will be provided to you when you create the Bot Service resource in the Azure AD Portal
    2. `MicrosoftAppPassword` - Again, the Azure Portal will provide this value when you create your Bot Service resource
    3. `BaseUrl` - This is your tunneling URL that will forward traffic to your locally running instance of the Bot. For example it should look like `https://jalew123.eu.ngrok.io/`
    4. `OAuthConnectionName` - This is the OAuth connection name that you will provision when you are configuring the bot. It is highly likely this will be `AAD`

### [Using Azure AD](#using-azure-ad)

Registering a bot with the Microsoft Bot Framework automatically creates a corresponding Azure AD application with the same name and ID.

1. Go to the [Application Registration Portal](https://aka.ms/appregistrations) and sign in with the same account that you used to register your bot.
1. Find your application in the list and click on the name to edit.
1. Navigate to **Authentication** under **Manage** and add the following SPA redirect URLs:

    - `https://<your_ngrok_url>/loginend`
1. Then add the following Web redirect URLs:

    - `https://token.botframework.com/.auth/web/redirect`

1. Additionally, under the **Implicit grant** subsection select **Access tokens** and **ID tokens**

1. Click on **Expose an API** under **Manage**. Select the Set link to generate the Application ID URI in the form of api://{AppID}. Insert your fully qualified domain name (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: api://<your_ngrok_url>/botid-{AppID}
1. Select the **Add a scope** button. In the panel that opens, enter `access_as_user` as the **Scope name**.
1. Set Who can consent? to Admins and users
1. Fill in the fields for configuring the admin and user consent prompts with values that are appropriate for the `access_as_user` scope. Suggestions:
    - **Admin consent title:** Teams can access the user’s profile
    - **Admin consent description**: Allows Teams to call the app’s web APIs as the current user.
    - **User consent title**: Teams can access your user profile and make requests on your behalf
    - **User consent description:** Enable Teams to call this app’s APIs with the same rights that you have
1. Ensure that **State** is set to **Enabled**
1. Select **Add scope**
    - Note: The domain part of the **Scope name** displayed just below the text field should automatically match the **Application ID** URI set in the previous step, with `/access_as_user` appended to the end; for example:
        - `api://<your_ngrok_url>/botid-<aad_application_id>/access_as_user`
1. In the **Authorized client applications** section, you identify the applications that you want to authorize to your app’s web application. Each of the following IDs needs to be entered:
    - `1fec8e78-bce4-4aaf-ab1b-5451cc387264` (Teams mobile/desktop application)
    - `5e3ce6c0-2b1f-4285-8d4b-75ee78787346` (Teams web application)
1. Navigate to **API Permissions**, and make sure to add the following delegated permissions:
    - User.Read
    - email
    - offline_access
    - openid
    - profile
    - User.Presence.Read
1. Then add the following Application permissions:
    - User.Read.All
1. Scroll to the bottom of the page and click on "Add Permissions".

1. The bot uses `MICROSOFT_APP_ID` and `MICROSOFT_APP_PASSWORD`, so these should already be set.

### Update your Microsoft Teams application manifest

1. Add new properties to your Microsoft Teams manifest:

    - **WebApplicationInfo** - The parent of the following elements.
    - **Id** - The client ID of the application. This is an application ID that you obtain as part of registering the application with Azure AD 1.0 endpoint.
    - **Resource** - The domain and subdomain of your application. This is the same URI (including the `api://` protocol) that you used when registering the app in AAD. The domain part of this URI should match the domain, including any subdomains, used in the URLs in the section of your Teams application manifest.

    ```json
    "webApplicationInfo": {
    "id": "<AAD_application_id here>",
    "resource": "<web_API resource here>"
    }
    ```

1. Add permissions and update validDomains to allow token endpoint used by bot framework. Teams will only show the sign-in popup if its from a whitelisted domain.

    ```json
    "permissions": [
        "identity"
    ],
    "validDomains": [
        "<<BASE_URI_DOMAIN>>",
        "token.botframework.com"
    ],
    ```

Notes:

-   The resource for an AAD app will usually just be the root of its site URL and the appID (e.g. api://subdomain.example.com/botid-c6c1f32b-5e55-4997-881a-753cc1d563b7). We also use this value to ensure your request is coming from the same domain.
-   You need to be using manifest version 1.5 or higher for these fields to be used.
-   Scopes aren’t supported in the manifest and instead should be specified in the API Permissions section in the Azure portal

### Add the Azure AD OAuth connection to the bot

1. Navigate to your bot's Bot Channels Registration page on the [Azure Portal](https://ms.portal.azure.com/#blade/HubsExtension/BrowseResourceBlade/resourceType/Microsoft.BotService%2FbotServices).
1. Click **Settings**.
1. Under **OAuth Connection Settings** near the bottom of the page, click **Add Setting**.
1. Fill in the form as follows:

    1. For **Name**, enter a name for your connection (e.g., "AAD")
    1. For **Service Provider**, select **Azure Active Directory v2**. Once you select this, the Azure AD-specific fields will be displayed.
    1. For **Client id**, enter your bot's client ID.
    1. For **Client secret**, enter your bot's client secret.
    1. For **Tenant ID**, enter `common`.
    1. For **Scopes**, enter `User.Read`.

1. Click **Save**.
1. In your .env file, set `OAuthConnectionName` to the name that you chose for this OAuth connection (probably "AAD")

### Testing the OAuth connections

Before proceeding, it's wise to test the OAuth connections that you have configured with the Azure Bot Service.

1. Open the [Bot Channels Registrations](https://ms.portal.azure.com/#blade/HubsExtension/BrowseResourceBlade/resourceType/Microsoft.BotService%2FbotServices) blade on the Azure Portal
1. Navigate to your Bot Channels Registration resource.
1. Click **Settings**.
1. Under **OAuth Connection Settings** near the bottom of the page, click on the connection.
1. Click on **Test connection**.
1. Sign in and authorize your app when prompted.

If the connection was configured correctly, you will be taken to a page with the access token that your bot would have received.

## Security notes

-   The verification code mechanism prevents a potential ["man in the middle" attack](https://hueniverse.com/explaining-the-oauth-session-fixation-attack-aa759250a0e7) by requiring evidence that the user who authorized the bot in the browser is the same person as the user who is chatting with the bot. **Don't** remove the need for a verification code without understanding what it is protecting against, and weighing the risk against your use case and threat model.
-   Don't use the `signin/verifyState` message to pass sensitive data (e.g., access tokens) directly to your bot in plaintext. The `state` value should not be usable without additional information that's available only to your bot.
-   The Teams app sends the `signin/verifyState` invoke message in a way that's equivalent to the user typing a message to your bot. This means that although the user information in the message is not falsifiable, a malicious user **can** tamper with the payload, or send additional invoke messages that were not initiated by your app.
-   Store your users’ access tokens in such a way that they are encrypted at rest, especially if you are also storing refresh tokens. Consider, based on your use case and threat model, how often to rotate the encryption key. (The sample uses an in-memory store for simplicity; do not do this in your production app!)
-   If you are using OAuth, remember that the `state` parameter in the authentication request must contain a unique session token to prevent request forgery attacks. The sample uses a randomly-generated GUID.

## Mobile clients

As of April 2019, Microsoft Teams mobile clients support the `signin` action protocol (that is, mobile clients work the same way as the desktop/web clients). It does require an updated version of the [Microsoft Teams JavaScript library](https://www.npmjs.com/package/@microsoft/teams-js) (1.4.1 or later). The way it used to work is described [here](fallbackUrl.md).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
