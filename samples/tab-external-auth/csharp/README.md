---
page_type: sample
description: Microsoft Teams sample app which showcases Google authentication using external auth providers.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "24/08/2023 11:20:17 AM"
urlFragment: officedev-microsoft-teams-samples-tab-external-auth-csharp
---
# Tab external auth - C#

This sample illustrates how to implement Google auth using external auth providers.

## Included Features
* External Auth (Google Oauth2)
* Tabs

## Interaction with app

![tab-external-auth](Images/tab-external-auth-app.gif)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [devtunnel](https://aka.ms/TunnelsCliDownload/win-x64) or [ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup

1. Create Google Ouath app [Google API Console](https://console.developers.google.com/)
 - Select Oauth client Id for app creation
 ![oauthapp1](Images/oauthapp1.png)

 - Select application type as `Web application` and give a suitable app name
 ![oauthapp3](Images/oauthapp3.png)

 - For authorized javascript url, give your app's base url
 ![oauthapp3](Images/oauthapp3.png)

 - For redirect url, give url in below format `https://<<base-url>>/Auth/GoogleEnd` where `base-url` is your application's base url. For eg,
 ![oauthapp4](Images/oauthapp4.png)

 - Once the app is created, copy the client id and client seret
 ![oauthapp2](Images/oauthapp2.png)

 - Enable access to the [Google People API](https://developers.google.com/people/).
 ![people-api](Images/peopleapi1.png)

  ![people-api2](Images/peopleapi2.png)

2. Setup NGROK
- Run ngrok - point to port 3978

```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnel`. Please follow [Create and host a Dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Modify the `/appsettings.json` and fill in the following details:
  - `{{GoogleAppId}}` - Generated from Step 1, while registrating google oauth app.
  - `{{GoogleAppPassword}}` - Generated from Step 1, while registrating google oauth app.
  - `{{ApplicationBaseUrl}}` - Your application's base url. For eg `https://123.ngrok.io`.


 - If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `/samples/tab-external-auth/csharp/TabExternalAuth` folder
  - Select `TabExternalAuth.csproj` file


4. Setup Manifest for App
    - **Edit** the `manifest.json` contained in the ./AppManifest folder to replace placeholder `{{GUID-ID}}` with any guid id.
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`. And if you are using dev tunnel, your URL will be https://12345.devtunnels.ms.
    - **Zip** up the contents of the `Manifest` folder to create a `Manifest.zip`  (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./Manifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
    
## Running the sample

### Google OAuth 2.0

![tab-page](Images/tab.png)

![redirect-page](Images/redirect-page.png)

![tab-auth-page](Images/tab1.png)

## Further Reading.
[External-auth](https://learn.microsoft.com/microsoftteams/platform/tabs/how-to/authentication/auth-oauth-provider#add-authentication-to-external-browsers)



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-external-auth-csharp" />