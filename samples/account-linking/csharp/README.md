---
page_type: sample
description: This sample demos linking user's AAD id with Facebook and google account of user from bot, ME and tab.
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

# External OAuth identity linking in Teams Apps.

This sample demos linking user's AAD id with their GitHub identity. 

The code generically handles account linking for OAuth2.0, the only GitHub specifics are related to calling the GitHub API. 

Please see the [Code Tours](#code-tours) section for in-depth explanation of the sample. 

## About the sample
    After the user installs the app, they will be prompted when using any of the capabilities (tab / conversational bot / messaging extension) to log into GitHub. After logging in the app will store the users access and refresh tokens so that they won't need to log in again across the other capabilities or on other devices. 

    There are example in-memory and Azure implementations of the token persistance. 
    
You will find the implementation details below. 

This sample will help you
•	Show you the architecture to build out SSO in Teams
•	Use this code (if you build in C#) 

### Scope
SSO is a problem across multiple surface areas and this sample can be used to build solutions in three surface areas:
•	Tabs
•	Bots
•	Messaging extensions
This sample does not need to cover:
•	It does need to cover Graph tokens. These tokens are received by exchanging user access tokens on the server-side.
•	It also does not need to be implemented in all languages, it can be implemented in one language to show the basic framework
•	Specific auth system systems on the partner side -> the partner can form these connections youselves

### Context
#### Problem Overview
AAD SSO is important for three parties:
•	End user: Skip signing into apps in Teams
•	Developer: Simpler development when it comes to authentication
•	IT Admin: Protect user privacy and security through conditional access policies
Often, you (a partner) have your own Identity and Access Management systems and your own user account information. In order for you to build out AAD SSO inside of Teams, you need to associate the users’ accounts with AAD SSO accounts. 

#### SSO Overview
Teams enacts AADD SSO in the following manner:
1.	In the tab, call getAuthToken() using the Teams JavaScript SDK. This tells Teams to obtain an auth token.
2.	If this is the first time the current user has used your tab application, you will be prompted to consent.
3.	Microsoft Teams requests a token for the tab application from the Azure AD endpoint for the current user.
4.	Azure AD sends the tab application token back to Teams.
5.	Microsoft Teams then sends the token back to the tab application.
6.	JavaScript in the tab application can parse the token and extract the information it needs, such as the user's email address. The tab app can optionally exchange the token server-side for further Graph permissions.

### Problem Definition
You have two approaches to link AAD accounts with existing customer accounts. These two scenarios are:
1.	the user exists in the third-party app with the same email address
2.	the user exists in the third-party app with a different email address
3.	the user logs  in with Microsoft SSO and then provides consent for AAD to access partner’s account information

#### Scenario #1
If the email addresses are same, then you can be linked right after the user is signed in with AAD. This is relatively simple for you to implement as you merely have to search their own auth system for the user’s email and associated the access token with that email. You need to create a separate attribute or separate table to match a user’s email address  with the AAD access token (and token refresh). 
To add to this difficulty, AzureAD also does not advise email mapping. Emails and User Principal Names (UPNs) can sometimes change.
 
#### Scenario #2
If the email addresses are different, you must do the following:
1.	Ask the user to consent/sign with SSO
2.	Ask the user to sign in with the email you use with their 3P app
3.	Associate their Teams identity with their app’s identity in a table (or as an attribute on their database)
Partners have been having some trouble with both of these scenarios. Scenario #2 is especially problematic for several partners. The underlying difficulty is associating a user’s AAD email and associated access token with a different email. For example, the user’s AAD email could be: john@contoso.com, but the email that is registered with the partner is john@gmail.com or even johnmatthews@contoso.com. In both of these examples, the emails don’t match up. The personal email address issue occurs frequently with apps that exist outside of enterprise scope (e.g., Starbucks, Uber, Coda, Observable). This is especially difficult to process as emails/UPNs can change as well for the same individual. 

**As the solution for Scenario #2 will solve Scenario #1 as well, we will focus on solving Scenario #2. 

## Solution
### Solution Description
There is one refresh token associated per user, with a refresh every ~12 hours (time is variable/to be determined).
As covered earlier, the solution will be targeted to solve Scenario #2 . There are four parts to this solution:
•	UI to prompt user to login to/accept SSO 
•	UI to prompt user to login to/accept 3P app
•	Logic to connect user information from the partner’s auth system to SSO
•	Table that contains: AzureAD identity (GUID/UUID) – not the AAD access token 

This sample does not store user information from your auth system . Replicating user info can cause a myriad of consistency and data inventory issues. 

Example
For example, let’s say your auth system uses personal email addresses and its users have no associated Teams identity.
For this, you would have to build out this entire flow:
1.	Ask the user to sign in to the app
2.	Ask you to connect their Teams identity with their app’s identity by going through the SSO flow
3.	Associate their Teams identity with their app’s identity in a table (or as an attribute on their database
Which would mean building out three components:
•	UI to prompt user to login to their Teams SSO after logging in with their Uber credentials
•	Logic to connect user information from their auth system to SSO
•	Table that contains: AzureAD identity (GUID/UUID) – not the AAD access token 


### Flows
The flows are:
Tab:
1.	User consents to Teams SSO
2.	User presented with partner auth popup
3.	Azure AD consent
Bot:
1.	SignIn Card 
2.	Auth popup
3.	Partner auth 
4.	Azure AD consent
Message Extension: 
1.	SignIn response
2.	Auth popup
3.	Partner auth
4.	Azure AD consent


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
Please follow the instructions on [creating an Azure AD application with Tab SSO](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso?tabs=dotnet#1-create-your-azure-ad-application). The fully qualified domain name will be the ngrok url from before.

#### 2.1 Configure the app for v2 tokens
**IMPORTANT** Please ensure the `accessTokenAcceptedVersion` in the `Manifest` blade is set to `2`.


Please save for a future step
1. The `Application (client) id` from the "Overview" blade for the app
2. A client secret from the "Certificates & secrets" page

### 3. Deploy the bot to Azure
Create a new [Azure Bot Registration]() and [connect it to Microsoft Teams](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

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
