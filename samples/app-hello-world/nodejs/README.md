# Microsoft Teams Hello World Sample App

Discover a simple Microsoft Teams sample app built with Node.js, designed to showcase core features such as tabs, bots, and messaging extensions.

## Features Included

- **Tabs**: Customizable web interfaces in Teams.
- **Bots**: Interactive AI-driven chat capabilities.
- **Messaging Extensions**: In-context interaction with users directly from within Teams messages.

## Interaction with the App

![HelloWorldGif](Images/AppHelloWorldGif.gif)

Explore the app's functionality through visual interaction shown in the gif above.

## Experience the App in Microsoft Teams

Try the demo app by uploading the app package to your Teams client. You need to enable sideloading in your tenant (see [instructions here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

- **App Package**: [Download Manifest](/samples/app-hello-world/csharp/demo-manifest/app-hello-world.zip)

## Prerequisites

1. **Microsoft Teams**: Installed with an active account (not a guest account).
2. **Node.js**: Version 16.14.2 or higher. [Download it here](https://nodejs.org/en/download/).
3. **Tunneling Solution**: Latest version of [dev tunnels](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/).
4. **M365 Developer Account**: [Set up your account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant).
5. **Teams Toolkit for VS Code or TeamsFx CLI**: [Visual Studio Code Extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one).

## Running the App Using Teams Toolkit for Visual Studio Code

1. Install [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview).
2. Add the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension).
3. Open the sample's directory in VS Code.
4. Sign in with your Microsoft 365 account via the extension.
5. Launch the app by selecting **Debug > Start Debugging** or press **F5**.

> If sideloading is not permitted, create a Microsoft 365 Developer Program account for a free sandbox environment.

## Manually Upload the App to Teams

### 1. Bot Setup in Azure
  
- Register a [Azure Bot](https://docs.microsoft.com/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2) and enable the [Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0).
- Set messaging endpoint to `https://<your_tunnel_domain>/api/messages`.

### 2. Set Up NGROK

- Run the command:

  ```bash
  ngrok http 3333 --host-header="localhost:3333"
  ```

- Alternatively, use dev tunnels:

  ```bash
  devtunnel host -p 3333 --allow-anonymous
  ```

### 3. Code Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

2. Navigate to `samples/app-hello-world/nodejs` and install modules:

   ```bash
   npm install
   ```

3. Configure `custom-environment-variables` and `default` with your bot credentials and base url.

4. Start your app:

   ```bash
   npm start
   ```

### 4. Teams Manifest Preparation

- Modify `manifest.json` in `app-hello-world/nodejs/appManifest` to include your App Id where indicated.
- Adjust `configurationUrl`, `staticTabs`, and `validDomains` accordingly as per your tunneling domain.
- Zip the manifest folder contents and upload it to Teams.

### Default Landing Configuration

- **Bot as Default**:

  ```json
  "staticTabs": [
    {
      "entityId": "conversations",
      "scopes": ["personal"]
    },
    {
      "entityId": "com.contoso.helloworld.hellotab",
      "name": "Hello Tab",
      "contentUrl": "https://${{BOT_DOMAIN}}/hello",
      "scopes": ["personal"]
    }
  ],
  ```

- **Tab as Default**:

  ```json
  "staticTabs": [
    {
      "entityId": "com.contoso.helloworld.hellotab",
      "name": "Hello Tab",
      "contentUrl": "https://${{BOT_DOMAIN}}/hello",
      "scopes": ["personal"]
    },
    {
      "entityId": "conversations",
      "scopes": ["personal"]
    }
  ],
  ```

## Running the Sample

- **Install the App**: Launch and install it within Teams.

### Additional Scenarios

#### Outlook on the Web

- Access your app via [Outlook](https://outlook.office.com/mail/) and find it under 'More Apps'.

#### Office on the Web

- Log into office.com, find your app under 'Apps' to test its integration.

## Azure Deployment

- For comprehensive deployment instructions to Azure, refer to [this guide](https://aka.ms/azuredeployment).

## Further Reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Overview](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Extending Teams Apps Across Microsoft 365](https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/overview)

![Telemetry Image](https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/app-hello-world-nodejs)