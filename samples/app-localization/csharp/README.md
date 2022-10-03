---
page_type: sample
description: Microsoft Teams app localization using Bot and Tab
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:25 PM"
urlFragment: officedev-microsoft-teams-samples-app-localization-csharp
---

# Teams App Localization
This sample illustrates how to implement [Localization for Microsoft Teams apps](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/apps-localization).

## Prerequisites

Verify you have the right account for building Teams apps and install some recommended development tools.

- You need a Teams account that allows [custom app sideloading](https://docs.microsoft.com/en-us/microsoftteams/platform/build-your-first-app/build-first-app-overview#set-up-your-development-account).
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
   > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `app-localization\csharp` folder
  - Select `Localization.csproj` file

3) Run ngrok - point to port 3978

    ```bash
    ngrok http --host-header=localhost
    ```

4) This step is related to Microsoft Teams app manifest
    - **Edit** the `manifest.json` contained in the `Manifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Provide ngrok Url** (turnnelling Url) in `manifest.json` for contentUrl in case of tabs and for messaging endpoint in case of bots if enabled. 
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

5) Run your app, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

## Interacting with the app in Teams
In Teams, Once the app is successfully installed, you can interact with tab and bot in your preferred language.

#### To change language in Teams
To change the language in Microsoft Teams, please click your profile picture at the top of the app, then select Settings -> General and go to the Language section. Choose the preferred language and restart to apply the change. This sample supports en-US, fr-CA, hi-IN and es-MX.
1. **Installation**: You should see your app installation screen content in selected language. 
![image](https://user-images.githubusercontent.com/31851992/115969498-d68aea80-a55a-11eb-9cb4-23e8b7bcdda9.png)

1. **Bot**: send any message to see localized 
![image](https://user-images.githubusercontent.com/31851992/115969313-ec4be000-a559-11eb-814b-0a5b4e5f96ad.png)
1. **Tab**: click on tab to see localized info.  
![image](https://user-images.githubusercontent.com/31851992/115969438-8dd33180-a55a-11eb-9307-5287e626c215.png)

#### To Add more languages for localization in Teams through Code.
 
 Add Resource files for the respective languages, Check culture fallback behaviour and how to add other cultures refer [Globalization and localization Fundamentals](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-5.0). 


  

