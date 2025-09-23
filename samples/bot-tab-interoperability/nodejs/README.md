---
page_type: sample
description: Microsoft Teams personal app unified communication between bot and tab
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "20-05-2024 13:38:27"
urlFragment: officedev-microsoft-teams-samples-bot-tab-interoperability-nodejs
---

# Personal Bot Tab

This sample illustrates how to implement interoperability between bot and tab.

## Included Features
* Interoperability bot and tab

## Interaction with app

![Preview Image](Images/)

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version
    ```
    
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
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
- Register an Microsoft Entra ID app in Azure portal and also register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

- While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. Run ngrok - point to port 3978 (pointing to ClientApp)

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```


2. Clone the repository
      ```bash
      git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
      ```

3. Open .env file from this path folder (samples/bot-tab-interoperabilityl/nodejs/server) and update ```MicrosoftAppId```,  ```MicrosoftAppPassword``` information with values generated values while doing Microsoft Entra ID App Registration.
- Update ```BaseURL``` with your application domain URL like ngrok URL: https://xxxx.ngrok-free.app and if you are using dev tunnels, your URL will be like: https://12345.devtunnels.ms.

4. Install node modules

   Inside node js folder,  navigate to `samples/bot-tab-interoperability/nodejs/server` open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

   - Repeat the same step in folder `samples/bot-tab-interoperability/nodejs/ClientApp`

    ```bash
    npm install
    ```

5. We have two different solutions to run, so follow below steps:
 
- In a terminal, navigate to `samples/bot-tab-interoperability/nodejs/server` folder, Open your local terminal and run the below command to install node modules. You can do the same in Visual studio code terminal by opening the project in Visual studio code
```bash
npm install
```

```bash
npm start
```

If you face any dependency error while installing node modules, try using below command

```bash
npm install --legacy-peer-deps
```

- In a different terminal, navigate to `samples/bot-tab-interoperability/nodejs/ClientApp` folder, Open your local terminal and run the below command to install node modules. You can do the same in Visual studio code terminal by opening the project in Visual studio code 
```bash
cd client
npm install
```

```bash
npm start
```

If you face any dependency error while installing node modules, try using below command

```bash
npm install --legacy-peer-deps
```

6. Run your app, either from Visual Studio code  with ``` npm start``` or using ``` Run``` in the Terminal.

7) Setup Manifest for Teams (__*This step is specific to Teams.*__)
    - **Edit** the `manifest.json` contained in the `appManifest` folder and replace your Microsoft App Id (that was created when you registered your app earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `configurationUrl` inside `configurableTabs` . Replace `{{BASE-URL}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Update** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Update** the `manifest.json` for `<<Manifest-id>>` with any GUID or with your MicrosoftAppId, generated during App registration in Azure portal.

    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)

**Note**: If you are facing any issue in your app, [please uncomment this line](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-tab-interoperability/nodejs/server/index.js#L48) and put your debugger for local debug.

## Running the sample

**Interacting with the app in Teams Personal Scope**

Interact with app by installing into personal scope.

![Install]()

2. --.

![Image1]()

![Image1]()

3. -----.

![Image1]()


## Further reading.

- [Unified communication between bot and tab](https://review.learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/teams%20conversational%20ai/bots-and-tabs?branch=pr-en-us-10829&tabs=update-the-tab-through-bot-message%2Ccsharp%2Cjavascript1)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-tab-interoperability-nodejs" />