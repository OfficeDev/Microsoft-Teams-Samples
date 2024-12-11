---
page_type: sample
products:
- office-365
languages:
- javascript
title: Microsoft Teams python Helloworld Sample
description: Explore a comprehensive Microsoft Teams hello world sample app built with Node.js, demonstrating key features such as tabs, bots, and messaging extensions.
extensions:
  contentType: samples
  createdDate: 10/19/2022 10:02:21 PM
urlFragment: officedev-microsoft-teams-samples-app-hello-world-python
---

# Microsoft Teams hello world sample app.

- This Hello World sample app showcases the fundamental features of Microsoft Teams, including tabs, bots, and messaging extensions, all built with Node.js.

## Included Features
* Tabs
* Bots
* Messaging Extensions

## Interaction with app

![HelloWorldGif](Images/AppHelloWorldGif.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams hello world sample app:** [Manifest](/samples/app-hello-world/csharp/demo-manifest/app-hello-world.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [Python SDK](https://www.python.org/downloads/) min version 3.8
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
-  [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.
- [Teams Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. Remember to select `requirements.txt` as dependencies to install when creating the virtual environment.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3) Create [Azure Bot resource resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Azure free account here](https://azure.microsoft.com/free/)

4) In a terminal, go to `samples\app-hello-world`

5) Activate your desired virtual environment

6) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

7) Update the `config.py` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

8) Run your app with `python app.py`

### 4. Setup Manifest for Teams

 - **This step is specific to Teams.**

    - **Edit** the `manifest.json` contained in the `app-hello-world/python/appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<Your Microsoft App Id>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `configurationUrl` inside `configurableTabs` and `validDomains`. Replace `{{domain-name}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    
    **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `app-hello-world/python/appManifest_Hub` folder with the required values.

    - **Zip** up the contents of the `app-hello-world/python/appManifest` folder to create a `manifest.zip`(Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal/team/groupChat scope (Supported scopes)

This app has a default landing capability that determines whether the opening scope is set to the Bot or a static tab. Without configuring this, Microsoft Teams defaults to landing on the bot in desktop clients and tab in mobile clients.

To set the **Bot as the default landing capability**, configure the 'staticTabs' section in the manifest as follows:
```bash
"staticTabs": [
  {
    "entityId": "conversations",
    "scopes": [
      "personal"
    ]
  },
  {
    "entityId": "com.contoso.helloworld.hellotab",
    "name": "Hello Tab",
    "contentUrl": "https://${{BOT_DOMAIN}}/hello",
    "scopes": [
      "personal"
    ]
  }
],
```

To set the **Tab as the default landing capability**, configure the 'staticTabs' section in the manifest as follows:
```bash
"staticTabs": [
  {
    "entityId": "com.contoso.helloworld.hellotab",
    "name": "Hello Tab",
    "contentUrl": "https://${{BOT_DOMAIN}}/hello",
    "scopes": [
      "personal"
    ]
  },
  {
    "entityId": "conversations",
    "scopes": [
      "personal"
    ]
  }
],
```

## Running the sample

**Install App:**

![Personal Scope Bot](Images/1.PersonalScope_Bot.png)

**Hello World Tab:**

![HelloWorld](Images/2.PersonalScope_StaticTab.png)

**ME:**

![Hello World App](Images/3.PersonalScope_MsgExtSearchQuery.png)

![Hello World App](Images/4.PersonalScope_MsgExtQuery2.png)

**Teams Scope**

![Hello World App](Images/TeamsScope_Bot.png)

![Hello World App](Images/TeamsScope_ComposeExt_1.png)

![Hello World App](Images/TeamsScope_ComposeExt_2.png)

![Hello World App](Images/TeamsScope_ConfigTab.png)

![Hello World App](Images/TeamsScope_FirstTab.png)

![Hello World App](Images/TeamsScope_SecondTab.png)

## Outlook on the web

- To view your app in Outlook on the web.

- Go to [Outlook on the web](https://outlook.office.com/mail/)and sign in using your dev tenant account.

**On the side bar, select More Apps. Your sideloaded app title appears among your installed apps**

![InstallOutlook](Images/Outlook_SelectApp.png)

**Select your app icon to launch and preview your app running in Outlook on the web**

![AppOutlook](Images/Outlook_StaticTab.png)

**Note:** Similarly, you can test your application in the Outlook desktop app as well.

## Office on the web

- To preview your app running in Office on the web.

- Log into office.com with test tenant credentials

**Select the Apps icon on the side bar. Your sideloaded app title appears among your installed apps**

![InstallOffice](Images/Office_M365.png)

**Select your app icon to launch your app in Office on the web**

![AppOffice](Images/Office_M365_App.png)

**Note:** Similarly, you can test your application in the Office 365 desktop app as well.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Extend Teams apps across Microsoft 365](https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/overview)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/app-hello-world-python" />