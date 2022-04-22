---
page_type: sample
description: This sample demos a bot with capability to upload files to SharePoint site and same files can be viewed in Teams file viewer.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "16-11-2021 00:15:13"
---

# Bot with SharePoint file to view in Teams file viewer

Using this C# sample, a bot with capability to upload files to SharePoint site and same files can be viewed in Teams file viewer

## Key features

![upload file card](BotWithSharePointFileViewer/Images/uploadFileCard.png)

![Upload file](BotWithSharePointFileViewer/Images/uploadFile.png)

![View file card](BotWithSharePointFileViewer/Images/viewFileCard.png)

![view file in teams](BotWithSharePointFileViewer/Images/fileViewer.png)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [ngrok](https://ngrok.com/) or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay) 

## Setup

1 Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2 In a terminal, navigate to `samples/bot-sharepoint-file-viewer/csharp`

3 Run ngrok - point to port 3978

```bash
# ngrok http -host-header=rewrite 3978
```

4. Create a Bot Registration
   In Azure portal, create a [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

   - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

   Add this permission to app registration

![Permissions](BotWithSharePointFileViewer/Images/permissions.png)

5. SharePoint site configuration
   - Login to [sharepoint](https://www.office.com/launch/sharepoint?auth=2)
   - Click on `Create site` and select `Team site`
   
   ![Team Site](BotWithSharePointFileViewer/Images/teamSite.png)
   
   - Enter site name and description of site.
   
   ![Site name](BotWithSharePointFileViewer/Images/siteName.png).
   
   - From site address eg: 'https://m365x357260.sharepoint.com/sites/SharePointTestSite'
      `m365x357260.sharepoint.com` - value is sharepoint tenant name.
	  
   - Click on next. (optional step)Add aditional owner and member.
   - Click on Finish.

6. Modify the `manifest.json` in the `/AppManifest` folder and replace the `<<Microsoft-App-Id>>` with the id from step 2.

7. Zip the contents of `AppManifest` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams as in step 6.

8. Modify the `/appsettings.json` and fill in the `{{ Bot Id }}`,`{{ Bot Password }}`,`{{ Connection Name }}` with the id from step 2 and `{{Sharepoint tenant name}}`,`{{Sharepoint site name}}` from step 5 and `{{Appbase-url}}`.

9. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your tab is uploaded to Teams

## To try this sample

- In a terminal, navigate to `BotWithSharePointFileViewer`

    ```bash
    # change into project folder
    cd # BotWithSharePointFileViewer
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/bot-sharepoint-file-viewer/csharp` folder
  - Select `BotWithSharePointFileViewer.csproj` file
  - Press `F5` to run the project

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.
