---
page_type: sample
description: Configurable Tab using AAD and Silent Authentication
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "20-03-2021 13:38:27"
urlFragment: officedev-microsoft-teams-samples-tab-channel-group-config-page-auth-csharp
---

# Config Tab Authentication

## Summary

There are many services that you may wish to consume inside your Teams app, and most of those services require authentication and authorization to get access to the service. Services include Facebook, Twitter, and of course Teams. Users of Teams have user profile information stored in Azure Active Directory (Azure AD) using Microsoft Graph and this article will focus on authentication using Azure AD to get access to this information.

OAuth 2.0 is an open standard for authentication used by Azure AD and many other service providers. Understanding OAuth 2.0 is a prerequisite for working with authentication in Teams and Azure AD. The examples below use the OAuth 2.0 Implicit Grant flow with the goal of eventually reading the user's profile information from Azure AD and Microsoft Graph.

## Interaction with app

![Initial Config Page](ConfigTabAuthentication/Images/Configtabauthenticatonmodule.gif)

## Initiate Silent and Simple Authentication ConfigurableTab using AAD

Authentication flow should be triggered by a user action. You should not open the authentication pop-up automatically because this is likely to trigger the browser's pop-up blocker as well as confuse the user.

Add a button to your [configuration](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/create-tab-pages/configuration-page) or [content](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/create-tab-pages/content-page) page to enable the user to sign in when needed. This can be done in the tab configuration page or any content page.

Azure AD, like most identity providers, does not allow its content to be placed in an iframe. This means that you will need to add a pop-up page to host the identity provider. In the following example this page is /tab-auth/simple-start. Use the microsoftTeams.authenticate() function of the Microsoft Teams client SDK to launch this page when your button is selected.


## Pre-requisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup.

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  - Your tab needs to run as a registered Azure AD application in order to obtain an access token from Azure AD. In this step you'll register the app in your tenant and give Teams   permission to obtain access tokens on its behalf.

  -Create an [AAD application](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso#1-create-your-aad-application-in-azure) in           Azure. You can do this by visiting the "Azure AD app registration" portal in Azure.

 - Set your application URI to the same URI you've created in Ngrok.
   - Ex: api://contoso.ngrok.io/{appId} using the application ID that was assigned to your app
                    
 - Setup a client secret. You will need this when you exchange the token for more API permissions from your backend.
   - Visit Manage > Certificates & secrets
   - Create a new client secret.
          
- Setup your API permissions. This is what your application is allowed to request permission to access.
   - Visit Manage > API Permissions
   - Make sure you have the following Graph permissions enabled: email, offline_access, openid, profile, and User.Read.

- Set Redirect URIs. Navigate to Authentication from left pane.
    - Click on Add Platform select *Web*.
    - Add URI as https://<<BASE-URI>>/SilentAuthEnd it will look like https://contoso.ngrok.io/SilentAuthEnd
    - Make sure to check *Access tokens* and *ID tokens* checkbox
    - Add one more URI as https://<<BASE-URI>>/SimpleAuthEnd
    - Again, Click on Add Platform and this time select *Single-page application*
    - Enter URI as https://<<BASE-URI>>/AuthEnd

![Authentication Azure AD](ConfigTabAuthentication/Images/authentication_azure_ad.png)
 
2. Setup for Bot
- Register a AAD aap registration in Azure portal.
- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK
- Run ngrok - point to port 3978

```bash
# ngrok http -host-header=rewrite 3978
```

4. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
 - Update the `appsettings.json` configuration for the tab to use the <<YOUR-MICROSOFT-APP-ID>> get from the step 1 Mircosoft App Id in TabAuthentication folder update
 
- If you are using Visual Studio
    - Launch Visual Studio
    - File -> Open -> Project/Solution
    - Navigate to `tab-channel-group-config-page-auth` folder
    - Select `TabAuthentication.csproj` file
    - Press `F5` to run the project


5.  __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your tab earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` and set your Base URI to the same URI you've created in Ngrok *everywhere* you see the place holder string `<<BASE-URI>>` (depending on the scenario the Microsoft App Id, Base URI may occur multiple times in the `manifest.json`)
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

7) Run your tab, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

## Running the sample.

![Initial Config Page](ConfigTabAuthentication/Images/config_page.png)

![Simple SignIn](ConfigTabAuthentication/Images/simple_signin.png)

![Silent SignIn](ConfigTabAuthentication/Images/silent_signin.png)

![Channel Tab](ConfigTabAuthentication/Images/channel_tab.png)

## Further Reading.

[Tab-Channle-Group-config-auth](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/create-channel-group-tab?pivots=node-java-script)


 


