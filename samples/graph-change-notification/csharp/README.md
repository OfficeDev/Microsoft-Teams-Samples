---
page_type: sample
description: This sample app demonstrates sending change notifications to user presence in Teams based on user presence status. The notifications are sent to user through bot in teams.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-graph-change-notification-csharp
---

# Change Notification sample

Bot Framework v4 ChangeNotification sample.

This sample app demonstrates sending change notifications to user presence in Teams based on user presence status.

## Included Features
* Bots
* Graph API
* Change Notifications

## Interact with app

![image](ChangeNotification/Images/Preview.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

## Setup
### Register you app with Azure AD.

  1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  2. Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the .env.
  4. Under **Manage**, select **Expose an API**. 
  5. Select the **Set** link to generate the Application ID URI in the form of `api://botid-{AppID}`. Insert your fully qualified domain name (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: `api://botid-{AppID}`
      * ex: `api://botid-00000000-0000-0000-0000-000000000000`.
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
          * `api://botid-00000000-0000-0000-0000-000000000000/access_as_user.
  11. In the **Authorized client applications** section, identify the applications that you want to authorize for your app’s web application. Each of the following IDs needs to be entered:
      * `1fec8e78-bce4-4aaf-ab1b-5451cc387264` (Teams mobile/desktop application)
      * `5e3ce6c0-2b1f-4285-8d4b-75ee78787346` (Teams web application)
  12. Navigate to **API Permissions**, and make sure to add the follow permissions:
  -   Select Add a permission
  -   Select Microsoft Graph -\> Delegated permissions.
      - `User.Read` (enabled by default)
      - `Presence.Read`
      - `Presence.Read.All`
  -   Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
   
   ![ApiPermission](ChangeNotification/Images/ApiPermission.png)

  13. Navigate to **Authentication**
      If an app hasn't been granted IT admin consent, users will have to provide consent the first time they use an app.
  - Set a redirect URI:
      * Select **Add a platform**.
      * Select **Web**.
      * Enter the **redirect URI** for the app in the following format: `https://token.botframework.com/.auth/web/redirect`.
  14.  Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description(Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the .env.

 2. Setup for Bot
    - In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
	**NOTE:** When you create app registration, you will create an App ID and App password - make sure you keep these for later.
    
### Instruction on setting connection string for bot authentication on the behalf of user
   ![image](ChangeNotification/Images/BotConnection.png)

   - Select Add OAuth Connection Settings.

   - Complete the form as follows.

    a. Enter a name for the connection. You'll use this name in your bot in the .env file. For example BotTeamsAuthADv1.

    b. Service Provider. Select **Azure Active Directory**.

    c. Client id. Enter the Application (client) ID that you recorded for your Azure identity provider app in the steps above.

    d. Client secret. Enter the secret that you recorded for your Azure identity provider app in the steps above.

    e. Grant Type. Enter authorization_code.

    f. Login URL. Enter https://login.microsoftonline.com.

    g. Tenant ID, enter the Directory (tenant) ID that you recorded earlier for your Azure identity app or common depending on the supported account type selected when you created the identity provider app.

    h. For Resource URL, enter https://graph.microsoft.com/
    
    i. Provide  Scopes like "Presence.Read, Presence.Read.All"

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3)  Open the code in Visual Studio

  - Launch Visual Studio code
  - File -> Open Folder
  - Navigate to `samples/graph-change-notification/csharp` folder
  - Select `ChangeNotification.sln` and open it in Visual Studio
   
4) Setup and run the bot from Visual Studio:
Modify the `appsettings.json` file with the following details:
    - Provide MicrosoftAppId and MicrosoftAppPassword in the appsetting that is created in Azure while doing Microsoft Entra ID app registration.
    - Provide ConnectionName in appsetting that is created in Azure wile creating connect for your Azure bot.
    - Provide the ngrok url as "BaseUrl" in appsetting on which application is running on like URL: https://xxxx.ngrok-free.app and if you are using dev tunnels, your URL will be like: https://12345.devtunnels.ms.
    - Press `F5` to run the project

5) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json`file contained in the `AppManifest` folder to replace your Microsoft App Id (that was created when you registered your Microsoft Entra ID app registration earlier) *everywhere* you see the place holder string `<<app id>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - `[Your tunnel Domain]` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `AppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-change-notification/csharp/ChangeNotification/AdapterWithErrorHandler.cs#L28) line and put your debugger for local debug.

## Running the sample
- After sucessfully installation of app you will get a sign in button. When sign in is complete then you get your current status in adapative card

![image0](ChangeNotification/Images/image0.png)

![image1](ChangeNotification/Images/image1.png)

![image2](ChangeNotification/Images/image2.png)

- After that when the user status chagnes you will get notify about their status: 
- Change user status from available to busy like

![image3](ChangeNotification/Images/image3.png)

![image4](ChangeNotification/Images/image4.png)

![image5](ChangeNotification/Images/image5.png)

![image6](ChangeNotification/Images/image6.png)

![image7](ChangeNotification/Images/image7.png)

![image8](ChangeNotification/Images/image8.png)
 
## Further reading
- [Bot Authentication](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=aadv2%2Ccsharp)
- [Change Notification](https://docs.microsoft.com/en-us/graph/api/resources/webhooks?view=graph-rest-beta)
- [App in Catalog](https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0)
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/graph-change-notification-csharp" />