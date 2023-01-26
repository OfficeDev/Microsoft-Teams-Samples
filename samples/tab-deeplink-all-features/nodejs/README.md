---
page_type: sample
description: Microsoft Teams sample app for demonstrating deeplink all features using Tab
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "26/01/2023 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-deeplink-all-features-nodejs

---
 # DeepLink All Features

 This sample app for demonstrating for deeplink all features using Tab.

- **Interaction with bot**

 ![Deeplink-All-Features](Images/DeeplinkAllFeatures.gif)

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

    - In a terminal, navigate to `samples/tab-deeplink-all-features/nodejs`

        ```bash
        cd samples/tab-deeplink-all-features/nodejs
        ```

    - Install modules

        ```bash
        npm install
        ```

    - Start the bot

        ```bash
        npm start
        ```
    - If you are using Visual Studio code
     - Launch Visual Studio code
     - Folder -> Open -> Project/Solution
     - Navigate to ```samples/tab-deeplink-all-features/nodejs``` folder
     - Select ```nodejs``` Folder
     
     - To run the application required node modules. Please use this command to install modules `npm i`.

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appPackage` folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.
    - **Zip** up the contents of the `Manifest` folder to create a `Manifest.zip` or `Manifest_Hub` folder to create a `Manifest_Hub.zip`(Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    

 ## Running the sample

**Application Setup**
![Application ](DeeplinkAllFeatures/Images/1.App.png)

**DeeplinkTab**
![DeeplinkTab](DeepLinkAllFeatures/Images/3.DeeplinkTab.png)

**AppProfilePage**
![AppProfilePage](DeepLinkAllFeatures/Images/4.AppProfilePage.png)

**AudioCallSelectPeople**
![AudioCallSelectPeople](DeepLinkAllFeatures/Images/5.AudioCallSelectPeople.png)

**AudioCallStart**
![AudioCallStart](DeepLinkAllFeatures/Images/6.AudioCallStart.png)

**AudioCallStarted**
![AudioCallStarted](DeepLinkAllFeatures/Images/7.AudioCallStarted.png)

**ScheduleMeeting**
![ScheduleMeeting](DeepLinkAllFeatures/Images/8.ScheduleMeeting.png)

**ScheduleMeetingDeeplink**
![ScheduleMeetingDeeplink](DeepLinkAllFeatures/Images/9.ScheduleMeetingDeeplink.png)

**StartNewChat**
![StartNewChat](DeepLinkAllFeatures/Images/10.StartNewChat.png)

**VideoCall**
![VideoCall](DeepLinkAllFeatures/Images/12.VideoCall.png)

 ## Further reading

- [Deep link to an application](https://learn.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/deep-link-application?tabs=teamsjs-v2)

