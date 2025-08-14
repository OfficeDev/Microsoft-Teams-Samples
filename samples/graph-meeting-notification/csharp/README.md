---
page_type: sample
description: This sample application demonstrates how to use an online meeting subscription to send notifications in Microsoft Teams chat when users join, leave, or when meetings start and end.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "10/08/2022 12:16:45 AM"
urlFragment: officedev-microsoft-teams-samples-graph-meeting-notification-csharp
---

# Online meeting subscription

This sample application illustrates the use of online meeting subscriptions in Microsoft Teams, allowing for real-time notifications in chat when users join or leave meetings and when meetings start or end. Developed in C#, it includes features like bot integration, change notifications, and adaptive cards, along with a comprehensive setup guide involving Azure Bot Service, Microsoft Graph API, and self-signed certificates for secure communication.

## Concepts introduced in this sample
- After sucessfully installation of bot in meeting you will get a welcome card and the subscription will be created for meeting it is installed in.

## Included Features
* Bots
* Change Notifications
* Adaptive Cards
* Graph API

## Interaction with app

![Feature](MeetingNotification/Images/MeetingNotificaionAppModule.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  .[NET 6.0](https://dotnet.microsoft.com/en-us/download) SDK.
    ```bash
        # determine dotnet version
        dotnet --version
    ```
-  [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) latest version or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

## Setup

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

   - Navigate to **API Permissions**, and make sure to add the follow permissions:
   - Select Add a permission
   - Select Microsoft Graph -> Application permissions.
   - `OnlineMeetings.Read.All`
   - Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
   - Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json file.

2. Setup for Bot
	
	- Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
	- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
	- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

### Create and install Self-Signed certificate

 To include resource data of online meeting, this Graph API require self-signed certificate. Follow the below steps to create and manage certificate.

 - You can self-sign the certificate, since Microsoft Graph does not verify the certificate issuer, and uses the public key for only encryption.

 - Use [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-whatis) as the solution to create, rotate, and securely manage certificates. Make sure the keys satisfy the following criteria:

    - The key must be of type `RSA`
    - The key size must be between 2048 and 4096 bits

 - Follow this documentation for the steps - [**Create and install Self-Signed certificate**](MeetingNotification/CertificateDocumentation/README.md)
 
3. Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

4.  Setup for code.

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
    
- Modify the `/appsettings.json` and fill in the following details:
  - `{{MicrosoftAppId}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal.
  - `{{ MicrosoftAppPassword}}` - Generated from Step 1, also referred to as Client secret
  - `{{ MicrosoftAppTenantId }}`- Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal Directory (tenant) ID.
  - `{{BaseUrl}}` -  Your application's base url. E.g. https://12345.ngrok-free.app if you are using ngrok and if you are using dev tunnels, your URL will be like: https://12345.devtunnels.ms.
  - `{{ Base64EncodedCertificate}}` - value from *Create and install Self-Signed certificate* step 2
  - `{{ CertificateThumbprint }}` - value from *Create and install Self-Signed certificate* step 2
    
 - Run from Visual Studio code

  - Launch Visual Studio code
  - File -> Open Folder
  - Navigate to `samples/graph-meeting-notification/csharp` folder
  - Select `MeetingNotification` folder
  - Press `F5` to run the project

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./AppManifest folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    
    - **Zip** up the contents of the `AppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppManifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
   
Follow this documentation to get more information on custom apps and uploading them into Teams - [Manage custom apps](https://docs.microsoft.com/en-us/microsoftteams/custom-app-overview) and [Upload an app package](https://docs.microsoft.com/en-us/microsoftteams/upload-custom-apps)

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-meeting-notification/csharp/MeetingNotification/AdapterWithErrorHandler.cs#L27) line and put your debugger for local debug.

## Running the sample
![Welcome Card](MeetingNotification/Images/WelcomeCard.png)

- After that when the metting gets started or user joins the meeting, notifications will be sent in chat:

![Meeting Started](MeetingNotification/Images/MeetingStartedCard.png)

- Notifications will also be triggered when someone leaves that meeting or when meeting gets end:

![Meeting Ended](MeetingNotification/Images/MeetingEndedCard.png)
 
## Further reading
- [Change notifications for Microsoft Teams meeting](https://docs.microsoft.com/en-us/graph/changenotifications-for-onlinemeeting)
- [Set up change notifications that include resource data](https://docs.microsoft.com/en-us/graph/webhooks-with-resource-data)



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/graph-meeting-notification-csharp" />