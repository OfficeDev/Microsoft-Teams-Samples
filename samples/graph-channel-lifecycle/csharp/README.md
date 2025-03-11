---
page_type: sample
description: This sample application demonstrates how to manage the channel lifecycle—create, update, and delete channels—using the Microsoft Graph API through a Teams tab.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-graph-channel-lifecycle-csharp
---

# Channel life cycle using C#

This sample application illustrates how to effectively manage the lifecycle of channels in Microsoft Teams using the Microsoft Graph API. It enables users to create, update, and delete channels directly through a Teams tab and is built with C# to demonstrate integration with the Teams Toolkit, complete with setup instructions for registration, tunneling, and deployment.

## Included Features
* Tabs
* Graph API
* RSC Permissions

## Interaction with app

 ![ChannelLifeCycleModule](ChannelLifecycle/Images/ChannelLifeCycleModule.gif)

## Prerequisites
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [Ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

- [Teams Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

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
1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

> NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

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
  - `{{ ClientId}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal.
  - `{{ ClientSecret}}` - Generated from Step 1, also referred to as Client secret
  - `{{ BaseUri }}` - Your application's base url. E.g. https://12345.ngrok-free.app if you are using ngrok and if you are using dev tunnels, your URL will be like: https://12345.devtunnels.ms.

- Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `samples/graph-channel-lifecycle/csharp`

  ```bash
  # run the bot
  dotnet run
  ```
  B) Or from Visual Studio
    - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `ChannelLifecycle` folder
  - Select `ChannelLifecycle.csproj` file
  - Press `F5` to run the project

 4. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
- [Upload app manifest file](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/apps-upload#load-your-package-into-teams) (zip file) to your team

## Running the sample.

  ![](ChannelLifecycle/Images/Image0.png)

  ![](ChannelLifecycle/Images/Image1.PNG)

  ![](ChannelLifecycle/Images/Image2.PNG)

  ![](ChannelLifecycle/Images/Image3.PNG)

  ![](ChannelLifecycle/Images/Image4.png)

  ![](ChannelLifecycle/Images/Image5.png)

  ![](ChannelLifecycle/Images/Image6.png)
  
  [Grpah-Channel-lifecycel](https://learn.microsoft.com/en-us/microsoftteams/plan-teams-lifecycle)
  
## Further Reading.

 [sample feature](https://docs.microsoft.com/en-us/graph/api/resources/channel?view=graph-rest-1.0) life cycle which includes create, update delete a channel
  
 



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/graph-channel-lifecycle-csharp" />