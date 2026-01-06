---
page_type: sample
description: This sample bot demonstrates how to use targeted messaging in Microsoft Teams to send private messages to specific users within channels and group chats. It includes a personal reminder bot that showcases targeted message delivery.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "01/05/2026 05:00:17 PM"
urlFragment: officedev-microsoft-teams-samples-targeted-messages-csharp

---
## Targeted Messages in Microsoft Teams

This sample demonstrates how to use targeted messaging in Microsoft Teams. Targeted messages are private messages that are only visible to a specific user within a channel or group chat conversation. The sample implements a personal reminder bot that sends reminders as targeted messages.

## Included Features
* Bots
* Targeted Messaging
* Adaptive Cards

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account).

- [.NET SDK](https://dotnet.microsoft.com/download) version 10.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version
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
</br>![image](https://raw.githubusercontent.com/OfficeDev/TeamsFx/dev/docs/images/visualstudio/debug/debug-button.png)
1. In the opened web browser, select Add button to install the app in Teams
> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

1. Register a new application in the [Microsoft Entra ID � App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.


    1) Select **New Registration** and on the *register an application page*, set following values:
        * Set **name** to your app name.
        * Choose the **supported account types** (any account type will work)
        * Leave **Redirect URI** empty.
        * Choose **Register**.

    2) On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.

    3) Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select "Never" for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json.

2. Setup for Bot
    - In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).
   - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
   - While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

3. Run ngrok - point to port 5130

   ```bash
   ngrok http 5130 --host-header="localhost:5130"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 5130 --allow-anonymous
   ```

4. Setup for code  
   - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
    Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `TargetedMessage` folder

     ```bash
     # run the bot
     dotnet run
     ```
     
  B) Or from Visual Studio

    - Launch Visual Studio
    - File -> Open -> Project/Solution
    - Navigate to `TargetedMessage` folder
    - Select `TargetedMessage.csproj` file
    - Press `F5` to run the project   
  
- Update the `appsettings.json` configuration file and replace with placeholder `ClientId` and `ClientSecret`. 

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your ClientId (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `TEAMS_APP_ID` and `BOT_ID` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using devtunnel it would be `https://1234.devtunnel.ms` then your domain-name will be `1234.devtunnel.ms` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

## Running the sample

**Using the Reminder Bot:**

Once installed, you can use the following commands in any channel or group chat:

**Set a Reminder:**
- `remind me in 5 minutes to check email`
- `remind me in 1 hour meeting starts`
- `remind me in 30 seconds test`
- `remind @John in 10 minutes review PR`

**Supported Time Formats:**
- Seconds: `30 seconds`, `30 secs`, `30s`
- Minutes: `5 minutes`, `5 mins`, `5m`
- Hours: `1 hour`, `2 hrs`, `1h`

**Manage Reminders:**
- `my-reminders` - View your active reminders
- `cancel-reminder [id]` - Cancel a specific reminder
- `reminder-help` - Show help information

**How Targeted Messaging Works:**

The bot uses the Teams SDK to send targeted messages. Key points:
- Set `isTargeted: true` when sending the message
- Set the `Recipient` property to specify who should see the message
- Works in both channels and group chats
- The message appears in the shared conversation but is only visible to the recipient

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading
- [Targeted Messages in Teams](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages)
- [Send and receive messages with a bot](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-messages)
- [Adaptive Cards](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card)
- [Microsoft Teams SDK for .NET](https://microsoft.github.io/teams-sdk/welcome)

- 

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-targeted-messages-csharp" />