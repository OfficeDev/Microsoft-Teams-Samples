---
page_type: sample
description: This sample demonstrates how to install a Microsoft Teams app using QR code scanning, allowing users to generate a QR code with the app ID for seamless installation.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "10/11/2021 23:35:25 PM"
urlFragment: officedev-microsoft-teams-samples-app-installation-using-qr-code-csharp
---

# Install app using barcode sample

This C# sample illustrates the process of installing Microsoft Teams apps via QR code scanning. Users can generate a QR code containing the app ID, making installation straightforward while leveraging features such as bot interaction, Teams SSO, adaptive cards, task modules, and media permissions.

`Currently, Microsoft Teams support for QR or barcode scanner capability is only available for mobile clients`.

## Included Features
* Bot
* Teams SSO (bots)
* Adaptive Cards
* Task Modules
* Device Permissions (media)
* Graph API

## Interaction with bot - Desktop View

![App Installation Using QRCodeDesktopGif](QRAppInstallation/Images/AppInstallationUsingQRCodeDesktop.gif)

## Interaction with bot - Mobile View

![App Installation Using QRCodeGif](QRAppInstallation/Images/AppInstallationUsingQRCode.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Install app using barcode sample:** [Manifest](/samples/app-installation-using-qr-code/csharp/demo-manifest/App-Installation-Using-QR.zip)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup

1) Setup for Bot SSO
- Refer to [Bot SSO Setup document](../BotSSOSetup.md)

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

- While registering the Azure bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
    
    > NOTE: When you create your app registration in Azure portal, you will create an App ID and App password - make sure you keep these for later.

2) Setup NGROK
-  Run ngrok - point to port 3978

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
   ```
   
- In a terminal, navigate to `samples/app-installation-using-qr-code/csharp`

    change into project folder
    ```bash
    cd # QRAppInstallation
    ```
 
- Modify the `/appsettings.json` and fill in the following details:
  - `{{Microsoft-App-Id}}` - Generated from Step 1 from Microsoft Entra ID app registration in Azure portal
  - `{{Microsoft-App-Password}}` - Generated from Step 1, also referred to as Client secret
  - `{{ Application Base Url }}` - Your application's base url. E.g. https://12345.ngrok-free.app if you are using ngrok and if you are using dev tunnels, your URL will be https://12345.devtunnels.ms.
  - `{{ Auth Connection Name }}` - The OAuthConnection setting from step 1, from Azure Bot SSO setup

The `Connection Name` referred to is the name that we provide while adding OAuth connection setting in the Bot channel registration.
Please follow link [Add authentication to your bot](https://docs.microsoft.com/microsoftteams/platform/bots/how-to/authentication/add-authentication?tabs=dotnet%2Cdotnet-sample#azure-ad-v2) to see how we can add the setting.
 
- Run the bot from a terminal or from Visual Studio, choose option A or B.
 
   A) From a terminal
     ```bash
     # run the bot
     dotnet run
     ```

   B) Or from Visual Studio
     - Launch Visual Studio
     - File -> Open -> Project/Solution
     - Navigate to `QRAppInstallation` folder
     - Select `QRAppInstallation.csproj` file
     - Press `F5` to run the project 

- Modify the `manifest.json` in the `/AppManifest` folder and replace the following details:
  - `{{Microsoft-App-Id}}` with Microsoft Entra ID app registration Application id, generated from Step 1
  - `{{domain-name}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.

- Zip the contents of `AppManifest` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams.

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppManifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

 - **Note**
 Kindly add the app/bot in personal scope and login there, afterwards add the app/bot in any Teams channel.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/app-installation-using-qr-code/csharp/QRAppInstallation/AdapterWithErrorHandler.cs#L30) line and put your debugger for local debug.

## Running the sample

- **Desktop View**
**Card with actions Generate QR code and Install App:**

![Card](QRAppInstallation/Images/CardWithButtons.png)

**Generate QR code is used to generate a QR code by selecting the app:**

![QR Code](QRAppInstallation/Images/QRCode.png)

**Install App is used to Scan the QR code and it then installs the app:**

![Install App](QRAppInstallation/Images/AppInstallation.png)

-  **Mobile View**
**Hey command interaction:**

![CardWithButtonsMobile](QRAppInstallation/Images/CardWithButtonsMobile.png)

**Permission UI:**

![Permission](QRAppInstallation/Images/Permission.png)

**QR Code:**

![QRCodeMobile](QRAppInstallation/Images/QRCodeMobile.png)

**App added:**

![AppAddedMobile](QRAppInstallation/Images/AppAddedMobile.png)

**Polly App Install:**

![AppInstallationMobile](QRAppInstallation/Images/AppInstallationMobile.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Integrate media Capabilities inside your app](https://learn.microsoft.com/microsoftteams/platform/concepts/device-capabilities/media-capabilities?tabs=mobile)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/app-installation-using-qr-code-csharp" />