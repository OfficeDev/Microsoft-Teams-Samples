---
page_type: sample
description: "This is an sample tab application which shows the contents of tab context object in a private and shared channel."
products:
- office-teams
- office
- office-365
languages:
- nodejs
- javascript
extensions:
  contentType: samples
  createdDate: "17/06/2022 12:26:57 PM"
urlFragment: officedev-microsoft-teams-samples-tab-channel-context-nodejs
---
## Prerequisites

- [NodeJS](https://nodejs.org/en/)
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
- [Create a private/shared channel](https://support.microsoft.com/en-gb/office/create-a-standard-or-private-channel-in-teams-fda0b75e-5b90-4fb8-8857-7e102b014525) Priave/Shared channel in team

# Tab context in channel
This sample shows the contents of tab context object in a private and shared channel.

## Included Features
* Tabs

## Interaction with app

![Preview Image](Images/Preview.gif)

## Setup

1) Setup NGROK
Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    
    ```

2) Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

3) Install node modules

   Inside node js folder,  navigate to `samples/tab-channel-context/nodejs/ClientApp` open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

    ```bash
    npm install

4) Run the solution from the same path terminal using below command.

    ```
    npm start
    ```

5) Modify the `manifest.json` in the `/AppPackage` folder and replace the following details
   - `{{Manifest-id}}` with some unique GUID. Refer [GUID generator](https://guidgenerator.com/)
   - `{{base-url}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
   - `<<Domain Name>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.

6) Zip the contents of `AppPackage` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams.

7) Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Apps -> Manage your apps -> Upload an app.
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
   
8) Add the tab in private/shared channel.

 **NOTE:** Only accounts with admin access can create private/shared channels in team.

# Running the sample
1) Configure in Private channel

![tab context in private channel](Images/privateChannel.png)

2) Configure in Public channel

![tab context in private channel](Images/regularChannel.png)

**Features of the sample**

- Add the tab in private/shared channel.
- The details of the context object will be shown on tab based on channel type.
- You can expand/reduce the JSON for the context object and can also copy a particular object slice.

## Further reading

- [Tab Basics](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/create-channel-group-tab?pivots=node-java-script)
- [Azure Portal](https://portal.azure.com)
- [Get-context-for-tabs](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/access-teams-context#retrieve-context-in-private-channels)
- [Shared Channels](https://docs.microsoft.com/en-us/MicrosoftTeams/shared-channels)
- [Private Channels](https://docs.microsoft.com/en-us/MicrosoftTeams/private-channels)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-channel-context-nodejs" />