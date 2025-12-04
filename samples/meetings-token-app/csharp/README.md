---
page_type: sample
description: The Meeting Token Generator app for Microsoft Teams allows participants to request and manage interaction tokens, ensuring equitable participation during meetings like scrums and Q&A sessions.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
  contentType: samples
  createdDate: "07-07-2021 13:38:27"
urlFragment: officedev-microsoft-teams-samples-meetings-token-app-csharp
---

# Meeting Token Generator

The Meeting Token Generator is a sample application designed to extend Microsoft Teams meetings by enabling participants to request interaction tokens. This app utilizes the Live Share SDK for real-time updates and includes features like meeting chat and a side panel for enhanced collaboration. The demo manifest is available for testing in your Microsoft Teams client.

- [Live-share-sdk-overview](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-live-share-overview)
- [Build tabs for meeting](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/build-tabs-for-meeting?tabs=desktop)

## Included Features
* Meeting Chat
* Meeting Details
* Meeting SidePanel
* Live Share SDK
* RSC Permissions
* Theme Handler

**NOTE: This capability is currently available in developer preview only.**

## Interaction with app
 
![Preview](Images/Preview.gif)

## Interaction with app theme

![Preview Image](Images/app-theme-tokenapp.gif)


## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app manifest (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Meetings Token App:** [Manifest](/samples/meetings-token-app/csharp/demo-manifest/meetings-token-app.zip)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [Ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

-[Microsoft 365 Agents Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

### Technologies

We assume working knowledge of the following technologies to gain full understanding of the app
- [C#](https://docs.microsoft.com/dotnet/csharp/tutorials/)
- [ECMAScript6](http://es6-features.org/)
- [Asp.NET core](https://docs.microsoft.com/aspnet/core/?view=aspnetcore-3.1) version 3.1
- [React.JS](https://reactjs.org/tutorial/tutorial.html) version 16+ 

The app uses the Teams extensibility features described on the following pages:
- [Apps in Teams meetings](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings)
- [Create apps for Teams meetings](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/create-apps-for-teams-meetings?tabs=json)
- [Tab single sign-on](https://docs.microsoft.com/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso) to get the identity of the user accessing the tab, in a way that can be verified in the server APIs

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.

1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio Microsoft 365 Agents Toolkit extension
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. In the debug dropdown menu of Visual Studio, select default startup project > Microsoft Teams (browser)
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`.
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
    </br>![image](https://raw.githubusercontent.com/OfficeDev/TeamsFx/dev/docs/images/visualstudio/debug/debug-button.png)
1. In the opened web browser, select Add button to install the app in Teams

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.


## Setup
**This app will work in developer preview only**

1. Setup for code

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

4. Setup for Bot
  - Register a AAD aap registration in Azure portal.
  - Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
  - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
  - While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

5. Setup NGROK
- Run ngrok - point to port 3978

```bash
# ngrok http 3978 --host-header="localhost:3978"
```

6. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
- Set up the appsettings.json with the following keys:
    - `"MicrosoftAppId"`: Application (client) ID of the bot's Azure AD application
    - `"MicrosoftAppPassword"`: client secret of the bot's Azure AD application
    - `"AzureAd"."TenantId"`: Tenant ID of the tenant where the app will be used. Note that the sample will only work in this tenant.
    - `"AzureAd"."ApplicationIdURI "`: Set to the same value as `api://[WebAppDomain]/MicrosoftAppId`.
    - `"ContentBubbleUrl "`: Content bubble iframe url (default. `https://[WebAppDomain]/contentBubble.html`). Remember that `[WebAppDomain]` will be your tunnel domain, so the content bubble URL will be similar to `https://f631****.ngrok-free.app/contentBubble.html`.
 
 - Build and run the service
      You can build and run the project from the command line or an IDE:

      A) From a command line:
      
        ```bash
        # run the server
        cd TokenApp
        dotnet run
        ```

      B) From an IDE:
      1. Launch Visual Studio
      2. File > Open > Project/Solution
      3. Navigate to the `TokenApp` folder
      4. Select `TokenApp.csproj` file
      5. Press `F5` to run the project

      1. Set `manifestVersion` to "devPreview"
      1. Add your bot configuration, with the app id of the bot generated from the previous steps
      1. Fill-in the following `webApplicationInfo` section, using `MicrosoftAppId` and `WebAppDomain` values from the previous section.
          
          ```json
          "webApplicationInfo": {  
            "id": "[MicrosoftAppId]",  
            "resource": "api://[WebAppDomainName]/[MicrosoftAppId]"  
          }
        ``` 
7. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
   
- Enable developer preview in your desktop Teams client
Follow [these instructions](https://docs.microsoft.com/microsoftteams/platform/resources/dev-preview/developer-preview-intro#enable-developer-preview) to enable developer preview. Note that Developer preview mode must be enabled on each Teams client app or browser.

Note: Open the meeting chat section and type @MeetingTokenApp Hello (It will send back the required information to you).

> In-meeting tabs are only available in the Teams desktop client. They will not be visible when you run Teams in a web browser.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/meetings-token-app/csharp/AdapterWithErrorHandler.cs#L32) line and put your debugger for local debug.
  
-- Upload the app in a Teams desktop client
    1. Create a meeting with few test participants, ideally with a mix of Presenters and Attendees.
    1. Once meeting is created, go to the meeting details page and click on the "Add tab" (+) button.
    1. In the pop-up that opens, click on "Manage apps".
    1. Click on "Upload a custom app" and upload the .zip file that was created in the previous steps. This adds the app to the meeting.
    1. Click on the "Add tab" button again. Now in the app selection page, the app should be visible as a "Meeting optimized tab".
    1. Select the Meeting Token app.
    1. Now the app will be visible in the meeting chat.
    1. Start the meeting and the icon should be visible in the meeting control bar.

## Running the sample
 - Display the current token that is being serviced in the meeting
 - Display the user list sorted by the token number in ascending order
 - Generate a token for the user upon request
 - Display the current user's token number
 - Mark a token as done by the user
 - Skip the current token for the organizer of the meeting

 ![config_page](Images/config_page.png)

 ![chat_page](Images/chat_meeting_tab.png)

 ![pre_meeting_tab](Images/pre_meeting_tab.png)

 ![side_panel_tab](Images/side_panel_tab.png)

 ### Interactions with app theme
 ![Preview Image](Images/light.PNG)

 ![Preview Image](Images/dark.PNG)

 ![Preview Image](Images/contrast.PNG)
  
### User interactions
- **Token** - Requests a token for the user
- **Done** - Acknowledges that the user is done with the token
- **Skip** - Skips the current user and moves on to the next person in line for a token. This button is only shown to users with the meeting Organizer role.

## Troubleshooting
The sample app uses an in-memory store to maintain token information and the service URL for the tenant. If you restart the project, you must run the following command to recapture the service URL: `@[BotName] reset`

In your own projects, please use a durable storage mechanism to store the service URL for the tenant.

## Further reading

- [Meeting apps APIs](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet)
- [Build tabs for meeting](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/build-tabs-for-meeting?tabs=desktop)
- [Teams Tabs experience](https://docs.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
- [Tabs SSO](https://docs.microsoft.com/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Azure Portal](https://portal.azure.com)
- [Handle theme change](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/access-teams-context?tabs=Json-v2%2Cteamsjs-v2%2Cdefault#handle-theme-change)

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meetings-token-app-csharp" />