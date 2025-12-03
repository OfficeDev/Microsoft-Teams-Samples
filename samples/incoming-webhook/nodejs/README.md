---
page_type: ssample
description: "This sample demonstrates sending message cards through Incoming Webhooks in Microsoft Teams and includes an HttpPOST action in the card."
products:
- office-teams
- office
- office-365
languages:
- nodejs
- javascript
extensions:
  contentType: samples
  createdDate: "07/01/2022 02:36:57 PM"
urlFragment: officedev-microsoft-teams-samples-incoming-webhook-nodejs
---
> [!IMPORTANT]
>
> The existing Microsoft 365 (previously called Office 365) connectors across all cloud platforms are nearing deprecation, and the creation of new Microsoft 365 connectors will soon be blocked. For more information on the schedule and how the Workflows app provides a more flexible and secure experience, see [retirement of Microsoft 365 connectors within Microsoft Teams](https://devblogs.microsoft.com/microsoft365dev/retirement-of-office-365-connectors-within-microsoft-teams/).

# Incoming webhook

This sample demonstrates how to use Incoming Webhooks in Microsoft Teams to send message cards. The UI allows users to input the Incoming Webhook URL and message card payload, which are sent to a team, showcasing the card's HttpPOST action for dynamic interaction. It includes setup instructions for tunneling solutions, API, and client app configuration, as well as integration across Microsoft 365 hubs like Outlook and Office.com.

## Included Features
* Tabs
* Incoming Webhooks

## Interaction with tab
![webhook-tab ](Images/webhook-app.gif)

## Prerequisites

- [NodeJS](https://nodejs.org/en/)
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account


## Setup

### Register your app with Azure AD.

  1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  2. Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
  4. Navigate to **API Permissions**, and make sure to add the follow permissions:
   Select Add a permission
      * Select Add a permission
      * Select Microsoft Graph -\> Delegated permissions.
      * `User.Read` (enabled by default)
      * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.


### 1. Setup for incoming webhook

1) Create a incoming webhook. [Create incoming webhooks](https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook). Keep this webhook URL handy while running the sample.

### 2. Setup NGROK
1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

### 3. Setup for code

1) Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```
1) Install node modules

   Inside node js folder,  navigate to `samples/incoming-webhoook/nodejs/api` open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

   - Repeat the same step in folder `samples/incoming-webhoook/nodejs/ClientApp`

    ```bash
    npm install
    ```

    If you face any dependency error while installing node modules, try using below command

    ```bash
    npm install --legacy-peer-deps
    ```

1) We have two different solutions to run so follow below steps:
 
   A) In a terminal, navigate to `samples/incoming-webhoook/nodejs/api`

   B) In a different terminal, navigate to `samples/incoming-webhoook/nodejs/ClientApp`

1) Run both solutions i.e. `samples/incoming-webhoook/nodejs/api` and `samples/incoming-webhoook/nodejs/ClientApp`
    ```
    npm start
    ```

### 4. Setup Manifest for Teams
1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `appManifest` folder to replace `{{Manifest-id}}` with any `GUID` ID.
    - **Edit** the `manifest.json` for `contentUrl`, `websiteUrl` inside `staticTabs` section. Replace `<<Domain-name>>` with app's base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `incoming-webhook\nodejs\appManifest_Hub` folder with the required values.
    - **Zip** up the contents of the `appManifest` folder to create a `Manifest.zip` or `appManifest_Hub` folder to create a `appManifest_Hub.zip`(Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal scope.

## Running the sample
- Open Incoming webhook tab.
![webhook-tab ](Images/incoming-webhook-page.png)

- Click on send button. You will get a card from webhook in the team where incoming webhook is added.
![webhook-tab ](Images/incoming-webhook-card.png)

## Outlook on the web

- To view your app in Outlook on the web.

- Go to [Outlook on the web](https://outlook.office.com/mail/)and sign in using your dev tenant account.

**On the side bar, select More Apps. Your uploaded app title appears among your installed apps**

![InstallOutlook](Images/InstallOutlook.png)

**Select your app icon to launch and preview your app running in Outlook on the web**

![AppOutlook](Images/AppOutlook.png)

**Note:** Similarly, you can test your application in the Outlook desktop app as well.

## Office on the web

- To preview your app running in Office on the web.

- Log into office.com with test tenant credentials

**Select the Apps icon on the side bar. Your uploaded app title appears among your installed apps**

![InstallOffice](Images/InstallOffice.png)

**Select your app icon to launch your app in Office on the web**

![AppOffice](Images/AppOffice.png) 

**Note:** Similarly, you can test your application in the Office 365 desktop app as well.

## Further reading

- [Build webhooks and connectors ](https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)
- [Extend Teams apps across Microsoft 365](https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/overview)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/incoming-webhook-nodejs" />
