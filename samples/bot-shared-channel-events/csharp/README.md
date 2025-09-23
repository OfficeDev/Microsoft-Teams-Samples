---
page_type: sample
description: Microsoft Teams bot can receive transitive member add and remove events in shared channels.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "09/10/2025 23:35:25 PM"
urlFragment: officedev-microsoft-teams-samples-bot-shared-channel-events-csharp
---

# Bot updates for handling transitive member add/remove events in Microsoft Teams shared channels

This sample shows how to build a Microsoft Teams bot using the Bot Framework SDK that responds to transitive member changes in shared channels. When a member is added to or removed from a parent team that shares a channel, Teams automatically updates the membership of the shared channel. The bot receives these transitive member add and remove events and can process them to track membership changes, maintain rosters, or trigger custom workflows. This enables developers to extend shared channel scenarios by keeping their applications and services in sync with the latest membership state across teams and channels.

The feature shown in this sample is currently available in public developer preview only.

## Included Features
* Bots
* Adaptive Cards
* RSC Permissions

## Interaction with app

![Meetings Events](SharedChannelEvents/Images/SharedChannelEvents.gif)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay) 
- [Microsoft 365 Agents Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio [Microsoft 365 Agents Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. In the debug dropdown menu of Visual Studio, select default startup project > **Microsoft Teams (browser)**
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`.
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
    </br>![image](https://raw.githubusercontent.com/OfficeDev/TeamsFx/dev/docs/images/visualstudio/debug/debug-button.png)
1. In the opened web browser, select Add button to install the app in Teams
> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup
> NOTE: The free ngrok plan will generate a new URL every time you run it, which requires you to update your Azure AD registration, the Teams app manifest, and the project configuration. A paid account with a permanent ngrok URL is recommended.

1) Setup for Bot
   - Register Azure AD application resource in Azure portal
   - In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

   - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
   - While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

## Configure Delegated Permissions in Azure AD App Registration

  1. Navigate to **API Permissions** in your app registration.  
  2. Select **Microsoft Graph** → **Delegated permissions**.  
  3. Add the following permission:  
     - `User.Read`  
  4. Grant **Admin Consent** for the added permission.  

  **NOTE:** When you create your bot you will create an App ID and App password - make sure you keep these for later.

2) Setup NGROK  
   Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3) Setup for code   
- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git

- Navigate to `samples/meetings-events/csharp` 
    - Modify the `/appsettings.json` and fill in the `{{ MicrosoftAppId }}`,`{{ MicrosoftAppPassword }}` with the values received while doing Microsoft Entra ID app registration in step 1.

- Run the app from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the app
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `SharedChannelEvents` folder
  - Select `SharedChannelEvents.csproj` file
  - Press `F5` to run the project

4) Setup Manifest for Teams

Modify the `manifest.json` in the `/appPackage` folder and replace the following details

   - `<<App-ID>>` with your Microsoft Entra ID app registration id   
   - `<<VALID DOMAIN>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
   - Zip the contents of `appPackage` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store
   - - **Upload** the `manifest.zip` to Teams
         - Select **Apps** from the left panel.
         - Then select **Upload a custom app** from the lower right corner.
         - Then select the `manifest.zip` file from `appPackage`.
         - Install the App in Teams Channels.

## Running the sample

**Shared Channel Events:**   

![Shared Channel Events](SharedChannelEvents/Images/1.Install.png)

![Shared Channel Events](SharedChannelEvents/Images/2.Select_Teams.png)

![Shared Channel Events](SharedChannelEvents/Images/3.Add_Channel.png)

![Shared Channel Events](SharedChannelEvents/Images/4.Create_Channel.png)

![Shared Channel Events](SharedChannelEvents/Images/5.SelectMember_For_ShareChannel.png)

![Shared Channel Events](SharedChannelEvents/Images/6.Manage_Created_Shared_Channel.png)

![Shared Channel Events](SharedChannelEvents/Images/7.Select_Apps_Tab.png)

![Shared Channel Events](SharedChannelEvents/Images/8.Select_App_Add_To_Channel.png)

![Shared Channel Events](SharedChannelEvents/Images/9.Add_Member_To_Channel.png)

![Shared Channel Events](SharedChannelEvents/Images/10.Member_Added.png)

![Shared Channel Events](SharedChannelEvents/Images/11.Removing_Member_From_Channel.png)

![Shared Channel Events](SharedChannelEvents/Images/12.Member_Removed.png)

![Shared Channel Events](SharedChannelEvents/Images/13.Share_Channel_Team.png)

![Shared Channel Events](SharedChannelEvents/Images/14.Select_Team_For_Sharing.png)

![Shared Channel Events](SharedChannelEvents/Images/15.Channel_Shared_With_Team.png)

![Shared Channel Events](SharedChannelEvents/Images/16.Channel_UnShare_With_Team.png)

![Shared Channel Events](SharedChannelEvents/Images/17.Removing_Channel_From_Team.png)

![Shared Channel Events](SharedChannelEvents/Images/18.Channel_UnShared_Notification.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- To be included once the release is completed.

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-shared-channel-events/csharp" />