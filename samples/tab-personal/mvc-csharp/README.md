---
page_type: sample
description: This sample showcases the development of a custom personal tab for Microsoft Teams, utilizing ASP.NET Core MVC to enhance user interaction. 
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-personal-mvc-csharp
---

# Personal Tab with ASP. NET Core MVC

This sample application illustrates the process of building a custom personal tab in Microsoft Teams using ASP.NET Core MVC. It provides a comprehensive guide on setting up the environment, configuring the app manifest, and deploying the app across Teams and Office platforms, enabling seamless integration and enhanced user experience.

 ## Included Features
* Tabs

## Interaction with app

![personaltab](Images/PersonalTabModule.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Personal Tab:** [Manifest](/samples/tab-personal/mvc-csharp/demo-manifest/tab-personal.zip)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [Ngrok](https://ngrok.com/download) (For local environment testing) latest version (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

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

1. Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

2) App Registration

### Register your application with Azure AD

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
4. Navigate to **API Permissions**, and make sure to add the follow permissions:
    * Select Add a permission
    * Select Microsoft Graph -> Delegated permissions.
    * `User.Read` (enabled by default)
    * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

3. Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

4. If you are using Visual Studio
 - Launch Visual Studio
 - File -> Open -> Project/Solution
 - Navigate to ```samples\tab-personal\mvc-csharp``` folder
 - Select ```PersonalTabMVC.sln``` file and open the solution

5. Modify the `manifest.json` in the `/appPackage` or `/Manifest_Hub` folder and replace the following details:
   - <<Guid>> with any random GUID.
   - `<<Base-url>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
   - `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.

6. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage or ./Manifest_Hub folder, select the zip folder, and choose Open.

  **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `/Manifest_Hub` folder with the required values.

## Running the sample

![personaltab](Images/personaltab.png)

![Greytab](Images/Greytab.png)

![tab](Images/Redtab.png)


## Outlook on the web

- To view your app in Outlook on the web.

- Go to [Outlook on the web](https://outlook.office.com/mail/)and sign in using your dev tenant account.

**On the side bar, select More Apps. Your uploaded app title appears among your installed apps**

![InstallOutlook](Images/InstallOutlook.png)

**Select your app icon to launch and preview your app running in Outlook on the web**

![AppOutlook](Images/AppOutlook.png)

**Click on 'Select Gray' button application will perform like below **

![AppOutlook_Gray.png](Images/AppOutlook_Gray.png.png)

**Click on 'Select Red' button application will perform like below **

![AppOutlook_Red.png](Images/AppOutlook_Red.png.png)

**Note:** Similarly, you can test your application in the Outlook desktop app as well.

## Office on the web

- To preview your app running in Office on the web.

- Log into office.com with test tenant credentials

**Select the Apps icon on the side bar. Your uploaded app title appears among your installed apps**

![InstallOffice](Images/InstallOffice.png)

**Select your app icon to launch your app in Office on the web**

![AppOffice](Images/AppOffice.png) 

**Click on 'Select Gray' button application will perform like below **

![AppOffice_Gray.png](Images/AppOffice_Gray.png.png)

**Click on 'Select Red' button application will perform like below **

![AppOffice_Red.png](Images/AppOffice_Red.png.png)

**Note:** Similarly, you can test your application in the Office 365 desktop app as well.

## Fruther Reading
[Tab-personal](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/what-are-tabs)
[Create a Custom Personal Tab with ASP. NET Core MVC](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/create-personal-tab?pivots=mvc-csharp)


[Tab-personal](https://learn.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
[Create a Custom Personal Tab with ASP. NET Core MVC](https://learn.microsoft.com/microsoftteams/platform/tabs/how-to/create-personal-tab?pivots=mvc-csharp)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-personal-mvc-csharp" />