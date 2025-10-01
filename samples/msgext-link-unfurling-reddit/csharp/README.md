---
page_type: sample
description: This sample demonstrates a C# Messaging Extension that implements link unfurling for Reddit links in Microsoft Teams.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-msgext-link-unfurling-reddit-csharp
---

# Link Unfurling for Reddit Links

![Preview Image](doc/images/Preview.gif)
This comprehensive C# sample illustrates how to implement a Messaging Extension for [Reddit](https://reddit.com) links in Microsoft Teams, featuring robust  [link unfurling](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling?tabs=dotnet) capabilities. With built-in user authentication and configurable settings, the extension streamlines the process of sharing and interacting with Reddit content seamlessly in Teams.


This sample demonstrates the following concepts: 
- Link Unfurling
- Bot Token Service & User Authentication
- Message Extension settings page
- Message Extension logout

## Setup
You will need to complete the following before running the code

### Create Messaging Extension
Follow the directions for [creating a messaging extension](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/create-messaging-extension).
- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. You __must__ use the 'Bot Channel Registration' so Bot Framework token service can be registered to manage tokens. 
2. The `reddit.com` and `www.reddit.com` domains should be registered in the 'messageHandlers' for the Teams App. If these are not included, the extension will not trigger for reddit links!

Make sure to note the app id and password for later. 

2) App Registration

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

### Configure Reddit App
Go To The [Reddit App Preferences](https://www.reddit.com/prefs/apps/) and register a new app for Reddit using the following parameters. 

| Parameter        | Value                      |
|------------------|:---------------------------|
| __Type__         | `web app`                  |
| __redirect uri__ | Not required               |
| Description      | Your own description       |
| About Url        | Url to your own about page |

Afterwards be sure to save the `client id` and the `secret` for the next step. 

### Configure Bot Framework Authentication
In the Azure portal, navigate to the Bot Channels Registration for the app and select the `Settings` menu under `Bot management`

At the bottom of the blade press `Add Setting`, this will open the `New Connection Setting` Blade. 

| Parameter         | Value                                                               |
|-------------------|---------------------------------------------------------------------|
| Name              | `reddit`                                                            |
| Service Provider  | `Generic Oauth 2`                                                   |
| Client Id         | Reddit Client Id from [Configure Reddit App](#configure-reddit-app) |
| Client Secret     | Reddit Secret from [Configure Reddit App](#configure-reddit-app)    |
| Authorization Url | `https://www.reddit.com/api/v1/authorize.compact`                   |
| Refresh Url       | `https://www.reddit.com/api/v1/access_token`                        |
| Scopes            | `read`                                                              |


<!-- ## Architecture -->
<!--insert an architecture diagram -->

## Directory Structure
- `/dotnet` the ASP.NET Core implementation which uses 2 legged OAuth for Reddit API calls
- `/dotnet_user_auth` the ASP.NET Core implementation which uses the 3 legged OAuth for Reddit API calls on behalf of the user.

## References

- [Link Unfurling BotBuilder Sample](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/55.teams-link-unfurling)




<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/msgext-link-unfurling-reddit-csharp" />