---
page_type: sample
description: This sample application illustrates how to create channel and group tabs in Microsoft Teams using C#/.NET, providing collaborative spaces for web content. It includes detailed setup instructions, prerequisites, and guidance for deploying the app within your Teams environment.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "01/07/2025 10:02:21 PM"
urlFragment: officedev-microsoft-teams-samples-tab-channel-group-quickstart-csharp
---

# Tabs quick start

Explore this simple hello world app that showcases channel and group tabs in Microsoft Teams using C#/.NET, designed to enhance collaboration around web-based content. This sample includes comprehensive setup steps, requirements, and deployment instructions, allowing you to easily integrate and run the app in your Teams environment.

## Included Features
* Tabs

## Interaction with app
![HelloTabGif](Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web/Images/tab_channel_group_module.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Tab Channel quick start:** [Manifest](/samples/tab-channel-group-quickstart/csharp/demo-manifest/tab-channel-group-quickstart.zip)

## Prerequisites
- Microsoft Teams is installed and you have an account (not a guest account)
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunneling solution
- [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions for uploading custom apps
- [Microsoft 365 Agents Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio [Microsoft 365 Agents Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`.
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
1. In the opened web browser, select Add button to install the app in Teams
> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
**NOTE:** When you create app registration, you will create an App ID and App password - make sure you keep these for later.

2. Setup NGROK
 - Run ngrok - point to port 5000

    ```bash
    ngrok http 5000 --host-header="localhost:5000"
    ```
   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 5000 --allow-anonymous
   ```

3. Setup for code
- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
- In a terminal, navigate to `samples/tab-channel-group-quickstart/csharp`

- Run the app from a terminal or from Visual Studio:

  A) From a terminal, navigate to `Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web`

  ```bash
  # run the app
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web` folder
  - Select `Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web.csproj` file
  - Press `F5` to run the project   

4. Setup Manifest for Teams
- **This step is specific to Teams.**
    - Edit the `manifest.json` contained in the `appPackage/` folder to replace with your MicrosoftAppId (that was created in step1 and is the same value of MicrosoftAppId) *everywhere* you see the place holder string `{{MicrosoftAppId}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - Zip up the contents of the `appPackage/` folder to create a `manifest.zip`
    - Upload the `manifest.zip` to Teams (in the left-bottom *Apps* view, click "Upload a custom app")

  **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `/AppManifest_Hub` folder with the required values.

- Modify the `/appsettings.json` and fill in the following details:
  - `{{MicrosoftAppId}}` - Generated from Step 1 is the application app id
  - `{{MicrosoftAppPassword}}` - Generated from Step 1, also referred to as Client secret
  - `{{MicrosoftAppTenantId}}` - Generated from Step 1, also referred to as Directory (tenant) ID

## Deploy to Teams
Start debugging the project by hitting the `F5` key or click the debug icon in Visual Studio and click the `Start Debugging` green arrow button.

### NOTE: First time debug step
On the first time running and debugging your app you need allow the localhost certificate. After starting debugging when the browser is launched and you have installed your app it will fail to load.

- Open a new tab `in the same browser window that was opened`
- Navigate to `https://localhost:5000/tab` (or your configured port)
- Click the `Advanced` button
- Select the `Continue to localhost`

### NOTE: Debugging
Ensure you have the appropriate debugging tools configured in Visual Studio for .NET development.


## Running the sample

**Install App:**

![InstallApp](Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web/Images/install.png)

**Set Up Tab:**

![SetupTab](Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web/Images/setup_tab.png)

**Configure Tab:**

![TabConfigure](Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web/Images/tab_configure.png)

**Hello World UI:**

![HelloWorld](Microsoft.Teams.Samples.TabChannelGroupQuickstart.Web/Images/hello_world.png)

## Deploy the app to Azure

To learn more about deploying an app to Azure, see [Deploy your app to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Create a group tab](https://learn.microsoft.com/microsoftteams/platform/tabs/how-to/create-channel-group-tab?pivots=node-java-script)
- [Extend Teams apps across Microsoft 365](https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/overview)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-channel-group-quickstart-csharp" />