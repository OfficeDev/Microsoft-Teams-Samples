# Meeting Apps

Meeting apps are Teams-aware webpages embedded in Microsoft Teams. They are scoped to group and teams. Add a meeting to an app from its installation page.

## Prerequisites
-  [NodeJS](https://nodejs.org/en/)

-  [ngrok](https://ngrok.com/)

-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

-  [Create an Azure AD App registration to support SSO and the User.Read Graph API](https://aka.ms/teams-toolkit-sso-appreg)

## ngrok

Teams needs to access your tab from a publically accessible URL. If you are running your app in localhost, you will need to use a tunneling service like ngrok. Run ngrok and point it to localhost.
  `ngrok http https://localhost:3000`

Note: It may be worth purchasing a basic subscription to ngrok so you can get a fixed subdomain ( see the --subdomain ngrok parameter)

**IMPORTANT**: If you don't have a paid subscription to ngrok, you will need to update your Azure AD app registration application ID URI and redirect URL ( See steps 5 and 13 [here](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso#steps) ) everytime you restart ngrok.

## Build and Run

In the root directory, execute:

`npm install`

`npm start`

## Deploy to Teams
* Update your backend service (api-server folder) first before trying to install the app

- Schedule a meeting.
- Edit the scheduled meeting.
- Press the '+' button, then select 'Manage apps'.
- Select the 'Upload custom app' from the bottom right corner.
- Choose the app package ( you can download from App Studio or build it yourself in the appPackage folder ).
- Once you see it in a meeting's list of managed apps, press the '+' again to add it to a meeting.
- Join the meeting an open the app to see it in action. 

### NOTE: First time debug step
On the first time running and debugging your app you need allow the localhost certificate.  After starting debugging when Chrome is launched and you have installed your app it will fail to load.

- Open a new tab `in the same browser window that was opened`
- Navigate to `https://localhost:3000/tab`
- Click the `Advanced` button
- Select the `Continue to localhost`

### NOTE: Debugging
Ensure you have the Debugger for Chrome/Edge extension installed for Visual Studio Code from the marketplace.