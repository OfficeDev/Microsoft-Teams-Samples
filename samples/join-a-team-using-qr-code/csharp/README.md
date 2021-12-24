﻿---
page_type: sample
description: This sample demos a feature where you can join a team using QR code containing the team's id.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
contentType: samples
createdDate: "24-12-2021 23:35:25"
---

# Join a team using QR code sample

This sample demos a feature where user can join a team using QR code containing the team's id.

User can generate a new QR code (contains team id information) and then scan the QR code to join the team.

`Currently, Microsoft Teams support for QR or barcode scanner capability is only supported for mobile clients`

- Type a message to get a card to generate the QR code.

 ![Card](JoinTeamUsingQR/Images/CardWithButtons.png)

- Select the team from dropdown list for which you want to generate the QR code and then click on      'Generate QR' button.

 ![QR Code](JoinTeamUsingQR/Images/QRCode.png)

- Scan the generated QR code to join the team.

 ![Join Team](JoinTeamUsingQR/Images/TeamQR.png)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [ngrok](https://ngrok.com/) or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay) 

## Setup

1. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2. In a terminal, navigate to `samples/join-a-team-using-qr-code/csharp`

3. Run ngrok - point to port 3978

   ```bash
   # ngrok http -host-header=rewrite 3978
   ```

4. Create a Bot Registration
   In Azure portal, create a [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

   **Note:** Once the Azure Bot is created, go to navigation pane and under Settings section select Channels and enable Microsoft Teams Channel.

    ![Configure Channel](JoinTeamUsingQR/Images/BotChannel.png)

   b) Add this permission to app registration

    ![Permissions](JoinTeamUsingQR/Images/Permission.png)

5. Modify the `manifest.json` in the `/AppManifest` folder and replace the `<<Microsoft-App-Id>>` with the id from step 2.

6. Zip the contents of `AppManifest` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams as in step 

7. Modify the `/appsettings.json` and fill in the `{{ Bot Id }}`,`{{ Bot Password }}`,`{{ Connection Name }}` with the id, password and   connection name from step 4.a and also your application base url `{{Appbase-url}}`.

8. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your tab is uploaded to Teams

## To try this sample

- In a terminal, navigate to `JoinTeamUsingQR`

    ```bash
    # change into project folder
    cd # JoinTeamUsingQR
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
  - Navigate to `samples/join-a-team-using-qr-code/csharp` folder
  - Select `JoinTeamUsingQR.csproj` file
  - Press `F5` to run the project

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.
