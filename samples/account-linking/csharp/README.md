---
page_type: sample
description: This sample demos linking user's Azure AD id with Facebook and google account of user from bot, ME and tab.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
contentType: samples
createdDate: "22-03-2022 00:00:00"
---

# Overview of External OAuth identity linking in Teams Apps

If you are familiar with the context for this sample, please feel free to jump the section on [Technical Implementation](#technical-implementation). 

## About the sample
This sample demos linking a user's Azure AD ID to another app's user ID for use with Teams SSO. This sample demo uses GitHub as the 3P app.  The code generically handles account linking for OAuth2.0. and the only GitHub specific related code is for calling the GitHub API.

This sample also has a [Code Tour](#code-tours) which explains the sample step-by-step. 

To review, this sample will:
* Show you the architecture to build out more seamless account linking with SSO in Teams
* Walk you through the code to implement this in detail
* Give you a jumpstart on account linking if you build in C#

### Scope
Teams SSO is enabled across multiple surface areas. This solution works across all those areas:
* Tabs
* Bots
* Messaging extensions

This sample does not cover:
* Microsoft Graph tokens and Microsoft Graph calls. These tokens are received by exchanging user access tokens on the server-side.
* All languages, it is only built in C#
* Specific auth systems on your (partner) side

### Context
#### Problem Overview
Azure AD SSO is important for three parties:
* End user: Reduces the number of sign ins and allows for a single sign-in for all devices
* Developer: Less dev time for authentication
* IT Admin: Protect user privacy and security through conditional access policies
Often, you (a partner) have your own Identity Provider (IDP) systems. For you to build out Azure AD SSO inside of Teams, you need to associate the users’ accounts with Azure AD SSO accounts. 

#### SSO Overview
Teams enacts Azure AD SSO in the following manner:
1.  In the tab, call getAuthToken() using the Teams JavaScript SDK. This tells Teams to obtain an auth token.
2.  If this is the first time the current user has used your tab application, the user will be prompted to consent.
3.  Microsoft Teams requests a token for your tab application from the Azure AD endpoint for the current user.
4.  Azure AD sends the tab application token back to Teams.
5.  Microsoft Teams then sends the token back to your tab application.
6.  JavaScript in yours tab application can parse the token and extract the information it needs, such as the user's email address. The tab app can optionally exchange the token server-side for further Graph permissions.

### Problem Definition
[Teams SSO](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-Azure AD-sso?tabs=dotnet) offers a seamless experience for partners to build auth inside of Teams. Achieving that level of seamlessness when your app is using Azure AD as its identity system is relatively straightforward. Achieving that level when your app is not using Azure AD is a lot less straightforward, partly because it can be difficult to link user accounts between Teams and your app's identity systems. 

To get around this, we typically see partners try to use email addresses to match up user identities in their different systems. However, that generates several problems. 

Even if the emails are the same, there are a few problems. First, emails and User Principal Names (UPNs) can sometimes change. Second, most likely, the identity framework you've chosen won't accept a simple email mapping and provide the user's access tokens in their system. These access tokens are required because they are used to get permissions on the user's behalf and are extremely important to keep secure. This path would also require you to change your own security architecture to allow special requests from your Teams apps. 

This is especially difficult if the user's email in Teams is different than their email in your system. For example, the user’s Azure AD email could be: john@contoso.com, but the email that is registered with your is john@gmail.com or even johnmatthews@contoso.com. In both of these examples, the emails don’t match up. The personal email address issue occurs frequently with apps that exist outside of enterprise scope. 

There's an elegant solution to this problem, which this sample demonstrates. 

## Solution
### Solution Description
1. Get the user's Azure AD access token - which can be done with a simple call (the beauty of SSO - review [SSO Overview](#sso-overview) if this is unclear)
2. Ask the user to sign into your chosen system or Identity Provider (IDP)
3. Retrive the access that your system or the Identity Provider provides
4. Store that access token in a table associated with the user's Azure AD identity

Therefore, there are four parts to this solution:
* Logic to get a user's Teams identity
* UI to prompt user to login to your app
* Logic to connect the user information from your IDP system to a user's identity in Teams
* Table that contains: User's Teams identity (GUID/UUID) and the user's access token from your IDP

This sample does not store user information from your auth system. Replicating user info can cause a myriad of consistency and data inventory issues. 

Now, whenever your Teams app needs to make a call, it can:
1. View the user's identity
2. Check the table for the user's IDP access token
3. Call your app using that token

There are both in-memory and Azure example implementations of the token persistence in this sample. 

This sample shows this flow using Github as the 3P app. It works out-of-box with zero onus on GitHub to change its concept of "identity" (no change to security model) and requires no carve-out in the existing APIs to enable the integration.


### Flows
From a user perspective, there are a few additional steps at the start. After the user installs the app, they will be prompted when using any of the capabilities (tab / conversational bot / messaging extension) to log your app. After logging in once, they won't need to log in again across the other capabilities or on other devices. To review, the flows are:

Tab:
1.  User clicks on tab
2.  User presented with partner auth popup

Bot:
1.  Bot sends SignIn Card 
2.  Partner auth 

Message Extension: 
1.  Users clicks on message extension, which shows SignIn response
2.  Partner auth

# Technical Implementation

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
  run ngrok locally
  ```bash
  ngrok http https://localhost:5001
  ```

## Setup
### 1. Getting the domain name
If you are running this app locally, you will probably be using some form of tunnel to your local machine. For this sample we use [ngrok](https://ngrok.com)

```bash
ngrok http https://localhost:5001
```

```bash
Session Status                online
Account                       {{REDACTED}}
Version                       2.3.40
Region                        United States (us)
Web Interface                 http://127.0.0.1:4040
Forwarding                    http://590a2d6f8b31.ngrok.io -> https://localhost:5001
Forwarding                    https://590a2d6f8b31.ngrok.io -> https://localhost:5001
Connections                   ttl     opn     rt1     rt5     p50     p90
                              0       0       0.00    0.00    0.00    0.00                
```

For this example, we'll use the `590a2d6f8b31.ngrok.io` as our domain name.

### 2. Provision an Azure AD application for Tab SSO
Please follow the instructions on [creating an Azure AD application with Tab SSO](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-Azure AD-sso?tabs=dotnet#1-create-your-azure-ad-application). The fully qualified domain name will be the ngrok url from before.

#### 2.1 Configure the app for v2 tokens
**IMPORTANT** Please ensure the `accessTokenAcceptedVersion` in the `Manifest` blade is set to `2`.

Please save for a future step
1. The `Application (client) id` from the "Overview" blade for the app
2. A client secret from the "Certificates & secrets" page

### 3. Deploy the bot to Azure
Create a new [Azure Bot Registration]( https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-4.0) and [connect it to Microsoft Teams](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

Please save  
1. The `Application (client) id` from the "Overview" blade for the bot (called the bot id going forward)
2. A client secret from the "Certificates & secrets" page (called the bot secret going forward)

### 4. Provision a GitHub app
Please follow the instructions on [creating a GitHub App](https://docs.github.com/en/developers/apps/building-github-apps/creating-a-github-app).

The "callback url" will be `https://{{your-domain}}/oauth/end`, e.g. `https://590a2d6f8b31.ngrok.io/oauth/end`.

Please save
1. The `Client ID`
2. A `client secret` 

### 5. Filling in the Manifest file
Please fill in the following values into the `Manifest/Manifest.json` file (called out using the `{{ }}` fences)

| Parameter  | Value |
|---|---|
| Bot Id  | The "Application (client) id" from step 3. |
| Tab domain | The domain from step 1 |
| Azure Ad Application Id | the "Application (client) id" from step 2| 

### 6. Filling in the app settings
Please copy the `Source/appsettings.json` into a new file `appsettings.development.json` and fill in the following parameters. 

| Parameter | Value |
| --------- | ----- |
| AzureAd:ClientId | The client id from step 2 |
| AzureAd:ClientSecret | The client secret from step 2 |
| AzureAd:TenantId | `common` | 
| Bot:MicrosoftAppId | The client id from step 3 | 
| Bot:MicrosoftAppPassword | The client secret from step 3 | 
| OAuth:ClientId | The client id from step 4 | 
| OAuth:ClientSecret | the client secret from step 4 |

Please note the `StateReplay`, `TokenStorage` and `Keyring` sections are unused unless the `UseAzure` setting is `true`.

### 7. Running the app
```bash
cd /Source
dotnet run
```

### 8. Installing the app
Please follow the documentation on [creating a Microsoft Teams app package](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/apps-package) and [sideloading your app in Teams](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/apps-package). 

The "manifest" in question is the `Manifest` directory filled out in step 5.

### 9. (Optional) Enable the Azure Ad version of the integration
If you want to try out the implementation using Azure you will need to provision a few resources first

1. [Storage account (with table storage)](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview)
2. [KeyVault](https://azure.microsoft.com/en-us/services/key-vault/)

The `StateReplay:Endpoint`, `TokenStorage:Endpoint` values and `KeyRing` section will need to be filled out with the values from the above.
## Code Tours
This repository uses VSCode [Code Tours](https://marketplace.visualstudio.com/items?itemName=vsls-contrib.codetour#:~:text=A%20%22code%20tour%22%20is%20simply%20a%20series%20of,CONTRIBUTING.md%20file%20and%2For%20rely%20on%20help%20from%20others.) to explain _how_ the code works. 

The tour files can be found in the `.tours` directory.

## OAuth Issues

If you encounter issues with your OAuth2.0 provider missing parameters it should be small changes to the query string parameters involved in the flow. 

### Authorization missing parameters
Add the parameters to the [OAuthController](./Source/Controllers/OAuthController.cs):58

### Access token request missing parameters
Add the parameters to the 
- [OAuthServiceClient](./Source/Services/OAuth/OAuthServiceClient.cs):41 for code flow
- [OAuthServiceClient](./Source/Services/OAuth/OAuthServiceClient.cs):55 for refresh token flow

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Authentication basics](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/authentication/authentication)


