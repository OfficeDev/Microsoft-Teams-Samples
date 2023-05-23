---
page_type: sample
description: Sample app showing custom personal Tab with ASP. NET Core
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:28 PM"
urlFragment: officedev-microsoft-teams-samples-tab-personal-razor-csharp
---

# Personal Tab with ASP.NET Core

In this quickstart we'll walk-through creating a custom personal tab with C# and ASP.Net Core Razor pages. We'll also use App Studio for Microsoft Teams to finalize your app manifest and deploy your tab to Teams.

 ## Included Features
* Tabs

## Interaction with app

![personaltabmodule](Images/PersonalTabModule.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Personal Tab:** [Manifest](/samples/tab-personal/mvc-csharp/demo-manifest/tab-personal.zip)


## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup

1. Run ngrok - point to port 3978
   ```ngrok http 3978 --host-header="localhost:3978"```

2. Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

3. If you are using Visual Studio
 - Launch Visual Studio
 - File -> Open -> Project/Solution
 - Navigate to ```samples\tab-personal\razor-csharp``` folder
 - Select ```PersonalTab.sln``` file and open the solution

4. Modify the `manifest.json` in the `/AppManifest` folder and replace the following details:
   - <<Guid>> with any random GUID.
   - `<<Base-url>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
   - `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.

5. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppManifest folder, select the zip folder, and choose Open.

## Running the sample

![personaltabinstllation](Images/installation.png)

![personaltab](Images/personaltab.png)

## Further Reading

[Tab-personal](https://learn.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
[Create a Custom Personal Tab with ASP.NET Core](https://learn.microsoft.com/microsoftteams/platform/tabs/how-to/create-personal-tab?pivots=mvc-csharp)




<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-personal-razor-csharp" />