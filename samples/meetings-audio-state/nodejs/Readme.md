---
page_type: sample
description: This sample tab application demonstrates how to control meeting audio in Microsoft Teams by muting and unmuting using the Incoming Client Audio API.
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

This sample tab application showcases how to manage audio states in Microsoft Teams meetings by muting and unmuting using the Incoming Client Audio API. With features like meeting side panel integration, support for Teams themes, and RSC permissions, it provides a streamlined experience for controlling audio directly from the app.

## Included Features
* Meeting SidePanel
* RSC Permissions
* Teams Themes handler 

## Interaction with Tab
![mute-unmute](Images/mute-unmute-audiostate.gif)

## Prerequisites
- [nodejs](https://nodejs.org/en/)
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Teams Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.
> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

> Note these instructions are for running the sample on your local machine.

1. Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

2. Setup for Bot

 Register your application with Azure AD

- Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
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

- **Edit** the `manifest.json` contained in the  `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)

- **Edit** the `manifest.json` for `<<DOMAIN-NAME>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.

- **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)

- Add the app to personal/team/groupChat scope (Supported scopes). 

**Note:** RSC has been enabled post 1.11 hence this feature will not work below 1.12 (Manifest Version). 

## Running the sample

You can interact with Teams Tab meeting sidepanel.

1. **Select your app**
![Install](Images/1.Install.png)

Add your application to the `meetings` as its display as below.
![AddToMeeting](Images/2.AddToMeeting.png)

![Configure](Images/3.ConfigureTab.png)

2. **Toggle To Mute**
Toggle button to mute state its looks like below.
![Stage 2](Images/4.AudioStateTrue.png)

3. **Toggle To Unmute**
Toggle button to Unmute state it will unmute client audio.
![Stage 3](Images/5.AudioStateFalse.png)

4. **Dark Theme**
App theme changes to dark when Teams theme switch to dark .
![Dark](Images/6.DarkTheme.png)

5. **Light Theme**
App theme changes to light when Teams theme switch to light .
![Light](Images/4.AudioStateTrue.png)

6. **Contrast Theme**
App theme changes to contrast when Teams theme switch to contrast .
![Contrast](Images/7.Contrast.png)

## Further reading

- [Toggle incoming audio](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/api-references?tabs=dotnet#toggle-incoming-audio)
- [Get incoming audio speaker](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/api-references?tabs=dotnet#get-incoming-audio-speaker)
- [Handle theme change](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/access-teams-context?tabs=Json-v2%2Cteamsjs-v2%2Cdefault#handle-theme-change)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meetings-audio-state-nodejs" />