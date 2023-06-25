---
page_type: sample
description: This is an sample tab (side panel) application which shows how to mute/unmute Teams meeting audio using toggle Incoming Client Audio API.
products:
- office-teams
- office
- office-365
languages:
- js
extensions:
 contentType: samples
 createdDate: "08-08-2022 10:00:01"
urlFragment: officedev-microsoft-teams-samples-meeting-audio-state-nodejs

---

# Meeting Audio State Sample

This sample shows how to mute/unmute Teams meeting audio using toggle Incoming Client Audio API.

## Included Features
* Meeting SidePanel
* RSC Permissions

## Interaction with bot
![mute-unmute](Images/mute-unmute-audiostate.gif)

## Prerequisites

- [nodejs](https://nodejs.org/en/)
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## Setup

> Note these instructions are for running the sample on your local machine.

1. Run ngrok - point to port 3978

   ```bash
     ngrok http 3978 --host-header="localhost:3978"
   ```  

2. Setup for Bot

 Register your application with Azure AD

- Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
- On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.
- Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the .env file.

- In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

3. Clone the repository
    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

4. In the folder where repository is cloned navigate to `samples/meetings-audio-state/nodejs`

5. Install node modules

   Inside node js folder, open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

    ```bash
    npm install
    ```

6. Update the `.env` with `PORT=3978`

7. Run your app

    ```bash
    npm start
    ```

 8. __*This step is specific to Teams.*__

- **Edit** the `manifest.json` contained in the  `TeamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)

- **Edit** the `manifest.json` for `<<DOMAIN-NAME>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.

- **Zip** up the contents of the `TeamsAppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)

- Add the app to personal/team/groupChat scope (Supported scopes). 

**Note:** RSC has been enabled post 1.11 hence this feature will not work below 1.12 (Manifest Version). 

## Running the sample

You can interact with Teams Tab meeting sidepanel.

1. **Select your app**
Add your application to the `meetings` as its display as below.
![Stage 1](Images/joinTheCall.png)

2. **Toggle To Mute**
Toggle button to mute state its looks like below.
![Stage 2](Images/ToggleMute.png)

3. **Toggle To Unmute**
Toggle button to Unmute state it will unmute client audio.
![Stage 3](Images/ToggleUnMute.png)


## Further reading

- [Toggle incoming audio](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/api-references?tabs=dotnet#toggle-incoming-audio)
- [Get incoming audio speaker](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/api-references?tabs=dotnet#get-incoming-audio-speaker)



<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meetings-audio-state-nodejs" />