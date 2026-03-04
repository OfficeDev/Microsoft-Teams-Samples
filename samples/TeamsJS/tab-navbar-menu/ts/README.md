---
page_type: sample
description: This sample application illustrates the use of a navbar menu in personal tabs within Microsoft Teams, enabling users to access multiple actions seamlessly. Designed for mobile clients, it includes features like an overflow menu for additional actions, enhancing user navigation and experience.
products:
- office-teams
- office
- office-365
languages:
- typescript
- nodejs
extensions:
 contentType: samples
 createdDate: "29/12/2022 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-navbar-menu-ts
---

# Teams Personal Tab Navbar-Menu Sample TS

This Microsoft Teams sample application showcases the implementation of a navbar menu in personal tabs, providing users with quick access to multiple actions and an overflow menu for additional options. Tailored for mobile clients, it enhances usability and improves navigation within the app, making it easier to manage tasks and access features efficiently.

**Note:** NaveBar menu is only supported in Mobile Clients.

 ## Included Features
* Tabs
* NavBar Menu

## Interaction with app - Mobile

![NavBarGif](Images/MenuGif.gif)

 ## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  [NodeJS](https://nodejs.org/en/)
-  [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) latest version or equivalent tunneling solution
-  [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

**Mobile app test:**
> If you test this sample via toolkit, once the toolkit runs successfully, go to appManifest -> build -> appManifest.local, unzip it, and change 'https://localhost:3978' to your ngrok URL.

## Setup

1. Setup NGROK

 - Run ngrok - point to port 3978

   ```bash
   ngrok http https://localhost:3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

2. App Registration

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

3. Setup for code
   - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

  - In a terminal, navigate to `samples/tab-navbar-menu/ts`
   
  - Install modules

      `npm install`

      `npm start`
      
   - The client will start running on 3978 port 

4. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appManifest folder to replace your GUID and you see the place holder string `{{GUID}}` in the `manifest.json`
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appManifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
   **Note** The navbar menu app is supported only personal scopes.
## Running the sample

**Install App:**

![InstallApp](Images/InstallApp.png)

**MainPage UI:**

![MainPage](Images/MainPage.png)

**Clik 3 small dots (Includes other app options and information):**

![Manu1](Images/Menu1.png)

**Click About Menu:**

![Menu2](Images/Menu2.png)

**Clik 3 small dots (Includes other app options and information):**

![Menu3](Images/Menu3.png)

**Click Contact Menu:**

![Menu4](Images/Menu4.png)

**Clik 3 small dots (Includes other app options and information):**

![Menu5](Images/Menu5.png)


## Further Reading
[Configure and add multiple actions in NavBar](https://learn.microsoft.com/en-us/microsoftteams/platform/concepts/design/personal-apps?view=msteams-client-js-1.12.1#configure-and-add-multiple-actions-in-navbar)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-navbar-menu-ts" />