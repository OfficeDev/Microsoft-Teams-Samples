---
page_type: sample
description: This is an sample application which showcases Microsoft Entra authentication in channel tab.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-channel-group-sso-quickstart-csharp_dotnetcore
---

# Teams Tab SSO Authentication Sample C#

## Interaction with app.

![setupteams](Images/TabChannelGroupSsoQuickStartModule.gif)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [Ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Run the app (Using Teams Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.10 Preview 4  or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Teams Toolkit for Visual Studio [Teams Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select default startup project > **Microsoft Teams (browser)**
1. In Visual Studio, right-click your **TeamsApp** project and **Select Teams Toolkit > Prepare Teams App Dependencies**
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps.
1. Select **Debug > Start Debugging** or **F5** to run the menu in Visual Studio.
1. In the browser that launches, select the **Add** button to install the app to Teams.
> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup
### Register your Teams Auth SSO with Azure AD

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  - Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
  - On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.
  - Under **Manage**, select **Expose an API**. 
  - Select the **Set** link to generate the Application ID URI in the form of `api://{AppID}`. Insert your fully qualified domain name (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: `api://fully-qualified-domain-name/{AppID}`
    * ex: `api://%ngrokDomain%.ngrok-free.app/00000000-0000-0000-0000-000000000000`.
  - Select the **Add a scope** button. In the panel that opens, enter `access_as_user` as the **Scope name**.
  - Set **Who can consent?** to `Admins and users`
  - Fill in the fields for configuring the admin and user consent prompts with values that are appropriate for the `access_as_user` scope:
     * **Admin consent title:** Teams can access the user’s profile.
     * **Admin consent description**: Allows Teams to call the app’s web APIs as the current user.
     * **User consent title**: Teams can access the user profile and make requests on the user's behalf.
     * **User consent description:** Enable Teams to call this app’s APIs with the same rights as the user.
 - Ensure that **State** is set to **Enabled**
 - Select **Add scope**
    * The domain part of the **Scope name** displayed just below the text field should automatically match the **Application ID** URI set in the previous step, with `/access_as_user` appended to the end:
        * `api://[ngrokDomain].ngrok-free.app/00000000-0000-0000-0000-000000000000/access_as_user.
-  In the **Authorized client applications** section, identify the applications that you want to authorize for your app’s web application. Each of the following IDs needs to be entered:
     * `1fec8e78-bce4-4aaf-ab1b-5451cc387264` (Teams mobile/desktop application)
     * `5e3ce6c0-2b1f-4285-8d4b-75ee78787346` (Teams web application)
 - Navigate to **API Permissions**, and make sure to add the follow permissions:
  -   Select Add a permission
  -   Select Microsoft Graph -\> Delegated permissions.
     * User.Read (enabled by default)
     * email
     * offline_access
     * OpenId
     * profile
 -  Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
 - Navigate to **Authentication**
    If an app hasn't been granted IT admin consent, users will have to provide consent the first time they use an app.
    Set a redirect URI:
     * Select **Add a platform**.
     * Select **web**.
     * Enter the **redirect URI** for the app in the following format: `https://<your_tunnel_domain>/Auth/End`. This will be the page where a successful implicit grant flow will redirect the user.
    
    Enable implicit grant by checking the following boxes:  
    ✔ ID Token  
    ✔ Access Token  
 -  Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description      (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the `appsettings.json`.

2. Setup NGROK
 - Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Modify the `/appsettings.json` and fill in the following details:
  - `{{YOUR-TENANT-ID}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal Directory (tenant) ID.
  - `{{YOUR-MICROSOFT-APP-ID}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal Application (client) ID.
  - `{{ YOUR-CLIENT-SECRET}}` - Generated from Step 1, also referred to as Client secret
  - `{{ ApplicationIdURI}}` - Your application's ApplicationIdURI. E.g. api://%ngrokDomain%.ngrok-free.app/00000000-0000-0000-0000-000000000000`

-  If you are using Visual Studio
   - Launch Visual Studio
   - File -> Open -> Project/Solution
   - Navigate to `samples/tab-personal-sso-quickstart/csharp_dotnetcore` folder
   - Select `PersonalTabSSO.csproj` file

- Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

4. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    -  **Edit** the `manifest.json` for `webApplicationInfo` resource `"api://<<your_tunnel_domain>>/<<YOUR-MICROSOFT-APP-ID>>""` with MicrosoftAppId. E.g. `"api://ngrok-free.app/0000000000-0000000000-0000000"`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

## Running the sample.

This sample illustrates how to implement SSO authentication for Teams Tab.

![setupteams](Images/setupteams.png)

![setuptab](Images/setuptab.png)

![tabsso](Images/tabsso.png)

![configuretab](Images/configuretab.png)

## Further Reading.
[Tab-channel-group-SSO-QuickStart](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/create-channel-group-tab?pivots=node-java-script)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-channel-group-sso-quickstart-csharp_dotnetcore" />