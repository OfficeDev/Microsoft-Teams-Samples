### API Server
The API server is used to exchange the access token provided by Teams to get a token for accessing graph resources that you need for your app. This sample is requesting permission to read the user's profile to display the current logged in user's profile picture.

### Prerequisites
- NodeJS
- M365 developer account or access to a Teams account with the appropriate permissions to install an app.
- Create an Azure AD App registration to support SSO and the User.Read Graph API

### Update the env files
In the api-server directory, open the .env file and update the CLIENT_ID and CLIENT_SECRET variables with the client ID and secret from your Azure AD app registration. If you requested additional Graph permissions from the default User.Read, append them, space separated, to the GRAPH_SCOPES key.

### Build and Run
In the root directory, execute:

`npm install`

`npm start`
