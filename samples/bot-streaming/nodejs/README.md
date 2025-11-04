---
page_type: sample
description: This sample app can be use to streaming scenarios in Teams using Azure Open AI and Bot Framework v4 for personal scope.
products:
- office-teams
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "11/18/2024"
urlFragment: officedev-microsoft-teams-samples-bot-streaming-nodejs
---

# Teams Streaming Bot Sample

This bot has been created using [Bot Framework](https://dev.botframework.com) and [Azure Open AI](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal) as a secondary/alternative option to using [Teams AI SDK](https://github.com/microsoft/teams-ai/tree/main/js/samples/04.ai-apps/i.teamsChefBot-streaming). 

Its main purpose is to demonstrate how to build a bot connected to an LLM and send messages through Teams.

## Included Features
* Bots
* Azure Open AI
* Streaming
* Custom Engine Agent - Copilot

> [!IMPORTANT]
> This bot doesn't save any context calls. Therefore, each interaction is individual and unique.

## Interaction with bot
![StreamingBot](Images/bot-streaming.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [NodeJS](https://nodejs.org/en/)
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Create an Azure Open AI service

- In Azure portal, create an [Azure Open AI service](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).
- **Deploy Azure Open AI model:** Deploy the `gpt-35-turbo` model in your created Azure Open AI service for the application to perform translation.
- Collect `AzureOpenAIEndpoint`, `AzureOpenAIKey`, `AzureOpenAIDeployment` values and save these values to update in `.env` file later.

### Register your app with Azure AD.

  1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
  2. Select **New Registration** and on the *register an application page*, set following values:
      * Set **name** to your app name.
      * Choose the **supported account types** (any account type will work)
      * Leave **Redirect URI** empty.
      * Choose **Register**.
  3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
  4. Navigate to **API Permissions**, and make sure to add the following permissions:
   Select Add a permission
      * Select Add a permission
      * Select Microsoft Graph -\> Delegated permissions.
      * `User.Read` (enabled by default)
      * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

## Setup for Bot

   In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration).
    - For bot handle, make up a name.
    - Select "Use existing app registration" (Create the app registration in Microsoft Entra ID beforehand.)
    - __*If you don't have an Azure account*__ create an [Azure free account here](https://azure.microsoft.com/free/)
    
   In the new Azure Bot resource in the Portal, 
    - Ensure that you've [enabled the Teams Channel](https://learn.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - In Settings/Configuration/Messaging endpoint, enter the current `https` URL you were given by running the tunneling application. Append with the path `/api/messages`

## Setup

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```
1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

1) In a terminal, navigate to `samples/bot-streaming/nodejs`

1) Install modules

    ```bash
    npm install
    ```

1) Update the `.env` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.) `MicrosoftAppTenantId` will be the id for the tenant where application is registered.
 - Also, set MicrosoftAppType in the `.env`. (**Allowed values are: MultiTenant(default), SingleTenant, UserAssignedMSI**)

1) Run your bot at the command line:

    ```bash
    npm start
    ```

1) __*This step is specific to Teams.*__
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal scope (Supported scopes)

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-streaming/nodejs/index.js#L46) line and put your debugger for local debug.

## Running the sample

**Install App in Teams:**
![InstallApp ](Images/1.InstallApp.png)

**Welcome Streaming Card Displayed in Teams:**
![2.WelcomeStreaming ](Images/2.WelcomeStreaming.png)

**User Asking a Question to the Bot:**
![3.AskQuestion ](Images/3.AskQuestion.png)

**Streaming Results from the Bot in Teams:**
![4.AskQuestion1 ](Images/4.AskQuestion1.png)

**Getting the information:**
![4.GettingInformation ](Images/4.GettingInformation.png)

**Bot's Response to the User's Question:**
![5.AskQuestionResults ](Images/5.AskQuestionResults.png)

## Custom Engine Agent - Copilot
**Installation of Copilot App**  
![Installation of Copilot App](Images/install1.png)  
**Opening the Daily Task Reminder in Copilot**  
![Opening the Daily Task Reminder in Copilot](Images/opening1.png)  
**Asking a question**  
![Creating a new reminder in Copilot](Images/running1.png)  
**Scheduling a task reminder**  
![Scheduling a task reminder](Images/showresult1.png)  

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Stream message through REST API](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/streaming-ux?branch=pr-en-us-10850&tabs=csharp#stream-message-through-rest-api) 
- [Custom Engine Agent-Copilot](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/overview-custom-engine-agent?utm_source=chatgpt.com)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-streaming-nodejs" />