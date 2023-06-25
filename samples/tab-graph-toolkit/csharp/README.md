---
page_type: sample
description: Microsoft Teams tab sample app for demonstrating graph toolkit component
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "11/11/2021 11:30:17 PM"
urlFragment: officedev-microsoft-teams-samples-tab-graph-toolkit-csharp
---

# Teams tab with microsoft graph toolkit

This is the demo app for [Teams tab using miscrosoft graph toolkit](https://docs.microsoft.com/en-us/graph/toolkit/get-started/build-a-microsoft-teams-tab?tabs=unpkg%2Cjs)

 ## Included Features
* Tabs
* Graph Toolkit

## Interaction with app

![Module](TabGraphToolkit/Images/TabGraphToolKit.gif)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
> NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.

  - Register your app with Microsoft identity platform via the Azure AD portal
  - Your app must be registered in the Azure AD portal to integrate with the Microsoft identity platform. See [Register an application with the Microsoft identity platform](https://docs.microsoft.com/en-us/graph/auth-register-app-v2).
  - Click on Add a Platform in redirect URI section.
  - Select Single Page Application and add following URL `<<base-url>>/tabauth`
  - Save and register.
  - Once App is registerd copy the `client_Id` for your app and update in the app.

2. Setup NGROK
 - Run ngrok - point to port 3978

  ```bash
   ngrok http 3978 --host-header="localhost:3978"
  ```

3. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Modify Update `client_Id` copied from step 1 in index.tsx file(samples/tab-graph-toolkit/csharp/TabGraphToolkit/clientapp/src)

  - In a terminal, navigate to `samples/tab-graph-toolkit/csharp`

    change into project folder
    ```bash
    cd # TabGraphToolkit
    ```
 - Install modules
   navigate to `samples/tab-graph-toolkit/csharp/TabGraphToolkit/ClientApp`

    ```bash
    npm install
    ```
   A) From a terminal
     ```bash
     # run the bot
     dotnet run
     ```

   B) Or from Visual Studio
     - Launch Visual Studio
     - File -> Open -> Project/Solution
     - Navigate to `TabGraphToolkit` folder
     - Select `TabGraphToolkit.csproj` file
     - Press `F5` to run the project 
     
4. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./AppPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `tab-graph-toolkit\csharp\TabGraphToolkit` folder with the required values.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` or `Manifest_Hub` folder into a `manifest.zip`.(Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package) 
 - Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams. 
   
## Running the sample
Once you access the Tab within your app you will be able to see following microsoft-graph-toolkit component. 

- `<mgt-login>`

![Login](TabGraphToolkit/Images/login.png)

- `<mgt-agenda>`

![agenda](TabGraphToolkit/Images/agenda.png)

- `<mgt-people-picker>`

![people-picker](TabGraphToolkit/Images/people-picker.png)

- `<mgt-tasks>`

![tasks](TabGraphToolkit/Images/tasks.png)

- `<mgt-todo>`

![todo](TabGraphToolkit/Images/todo.png)

- `<mgt-person-card>`

![person-card](TabGraphToolkit/Images/person-card.png)

- `<mgt-person>`

![person](TabGraphToolkit/Images/person.png)

## Outlook on the web

- To view your app in Outlook on the web.

- Go to [Outlook on the web](https://outlook.office.com/mail/)and sign in using your dev tenant account.

**On the side bar, select More Apps. Your sideloaded app title appears among your installed apps**

![InstallOutlook](TabGraphToolkit/Images/InstallOutlook.png)

**Select your app icon to launch and preview your app running in Outlook on the web**

![AppOutlookLogin](TabGraphToolkit/Images/AppOutlookLogin.png)

![AppOutlook](TabGraphToolkit/Images/AppOutlook.png)

![AppOutlookDetails](TabGraphToolkit/Images/AppOutlookDetails.png)

**Note:** Similarly, you can test your application in the Outlook desktop app as well.

## Office on the web

- To preview your app running in Office on the web.

- Log into office.com with test tenant credentials

**Select the Apps icon on the side bar. Your sideloaded app title appears among your installed apps**

![InstallOffice](TabGraphToolkit/Images/InstallOffice.png)

**Select your app icon to launch your app in Office on the web**

![AppOfficeLogin](TabGraphToolkit/Images/AppOfficeLogin.png) 

![AppOffice](TabGraphToolkit/Images/AppOffice.png) 

![AppOfficeDetails](TabGraphToolkit/Images/AppOfficeDetails.png) 

**Note:** Similarly, you can test your application in the Office 365 desktop app as well.

## Further Reading
[Tab-graph-toolkit](https://learn.microsoft.com/en-us/graph/toolkit/get-started/build-a-microsoft-teams-tab?tabs=unpkg%2Chtml)
[Extend Teams apps across Microsoft 365](https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/overview)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-graph-toolkit-csharp" />