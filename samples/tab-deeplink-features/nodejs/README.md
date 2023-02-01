---
page_type: sample
description: Microsoft Teams sample app for demonstrating deep link features using tab
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "26/01/2023 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-deeplink-features-nodejs

---
 ## Deep Links Features
 This sample app for demonstrating for deep links features using tab.

- **Interaction with App**

 ![Deeplink-Features](Images/DeeplinkFeatures.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Deep-Link Features:** [Manifest](/samples/tab-deeplink-features/nodejs/demo-manifest/tab-deeplink-features.zip)

## Prerequisites
- Microsoft Teams is installed and you have an account (not a guest account)
- To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 18x  or higher)
- [ngrok](https://ngrok.com/download) or equivalent tunneling solution
- [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the 

## Setup.

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal. 
    
**NOTE:** When you create app registration, you will create an App ID and App password - make sure you keep these for later.

3. Setup NGROK
   - Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```
4. Setup for code

  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

    - In a terminal, navigate to `samples/tab-deeplink-features/nodejs`

        ```bash
        cd samples/tab-deeplink-features/nodejs
        ```

    - If you are using Visual Studio code
     - Launch Visual Studio code
     - Folder -> Open -> Project/Solution
     - Navigate to ```samples/tab-deeplink-features/nodejs``` folder
     - Select ```nodejs``` Folder

    - Install modules

        ```bash
        npm install
        ```
    - Navigate to `env.js` file and update MicrosoftAppID at placeholer `<<Microsoft-App-ID>>` which you have generated in Step 1 (App Registration creation) and update your AppId at placeholder `<<App-ID>>` (You can get it manually from [teams admin portal](https://admin.teams.microsoft.com/).
    - [TeamsAdminPortal-AppID](Images/11.TeamsAdminPortal.png)

    - Run the App

        ```bash
        npm start
        ```

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appPackage` folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.
    - **Zip** up the contents of the `Manifest` folder to create a `Manifest.zip` or `Manifest_Hub` folder to create a `Manifest_Hub.zip`(Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)


 ## Running the sample

**Application Setup**
![Application ](Images/1.App.png)

**DeeplinkTab**
![DeeplinkTab](Images/3.DeeplinkTab.png)

**AppProfilePage**
![AppProfilePage](Images/4.AppProfilePage.png)

**AudioCallSelectPeople**
![AudioCallSelectPeople](Images/5.AudioCallSelectPeople.png)

**AudioCallStart**
![AudioCallStart](Images/6.AudioCallStart.png)

**AudioCallStarted**
![AudioCallStarted](Images/7.AudioCallStarted.png)

**ScheduleMeeting**
![ScheduleMeeting](Images/8.ScheduleMeeting.png)

**ScheduleMeetingDeeplink**
![ScheduleMeetingDeeplink](Images/9.ScheduleMeetingDeeplink.png)

**StartNewChat**
![StartNewChat](Images/10.StartNewChat.png)

**VideoCall**
![VideoCall](Images/12.VideoCall.png)

 ## Further reading

- [Create Deep link](https://learn.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/deep-links)
