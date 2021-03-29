# API Server

The API server is used to exchange the access token provided by Teams to get a token for accessing graph resources that you need for your app.  This sample is requesting permission to read the user's profile to display the current logged in user's profile picture.

## Prerequisites
-  [NodeJS](https://nodejs.org/en/)

-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

-  [Create an Azure AD App registration to support SSO and the User.Read Graph API](https://aka.ms/teams-toolkit-sso-appreg)

## Update the env files

- In the *api-server* directory, open the *.env* file and update all the fields marked 'TODO':
  - CLIENT_ID, CLIENT_SECRET = Azure App Registration ID and secret
  - BotId = Bot Framework bot ID
  - BotPassword = Bot Framework bot password
  - REACT_APP_BASE_URL = The URL from which your app will be served
  - TEAMS_APP_ID = The Teams app ID in your manifest.

## Build and Run

In the root directory, execute:

`npm install`

`npm start`
