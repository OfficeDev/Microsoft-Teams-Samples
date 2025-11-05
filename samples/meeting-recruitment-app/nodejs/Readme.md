---
page_type: sample
description: "This sample illustrates a meeting experience for recruitment scenario using Apps In Meetings. This app also uses bot for sending notifications."
products:
- office-teams
- office
- office-365
languages:
- nodejs
- javascript
extensions:
  contentType: samples
  createdDate: "10/01/2021 02:36:57 PM"
urlFragment: officedev-microsoft-teams-samples-meeting-recruitment-app-nodejs
---

# Recruitment App Sample using Apps in Meetings

This sample app demonstrates how to enhance recruitment meetings in Microsoft Teams using bots to send notifications and provide in-meeting functionality. It includes features such as managing candidate information, adding interview questions, sharing assets, and capturing feedback in the meeting side panel.

## Included Features
* Bots
* Meeting Chat 
* Meeting Sidepanel 
* Meeting Details

## Interact with app

![Details](Images/meetingrecruitment.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Recruitment App Sample:** [Manifest](/samples/meeting-recruitment-app/csharp/demo-manifest/Meeting-Recruitment-App.zip)

## Prerequisites

- [NodeJS](https://nodejs.org/en/) must be installed on your development machine (version 16.14.2  or higher).
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
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

## Setup

 1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal. 
  
         Go to App registrations and create a new app registration in a different tab.
      Register an application.
      Fill out name and select third option for supported account type and click "Register".

      ![AppRegistration](Images/AppRegistration.png)

      * Copy and paste the App Id and Tenant ID somewhere safe. You will need it in a future step.

      - Create Client Secret.
         * Navigate to the "Certificates & secrets" blade and add a client secret by clicking "New Client Secret".

      ![ClientSecret](Images/clientsecret.png) 

      * Copy and paste the secret somewhere safe. You will need it in a future step.

       Navigate to **API Permissions**, and make sure to add the follow permissions:
    * Select Add a permission
    * Select Microsoft Graph -> Delegated permissions.
    * `User.Read` (enabled by default)
    * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.


 
 2. Setup for Bot
    - In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.
    **NOTE:** When you create app registration, you will create an App ID and App password - make sure you keep these for later.
    
 3. Create a Azure Storage account(This is needed to store/retrieve data that's used in the app) 
  [Create storage account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal)

   This step will create a storage account. You will require storage account name and keys in next steps.
  
   Please follow [View account keys](https://docs.microsoft.com/azure/storage/common/storage-account-keys-manage?tabs=azure-portal#view-account-access-keys) to see the   
   keys info.

4. Setup NGROK
 - Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

5. Setup for code

  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
   
   -Modify the `keys.js` file in the location `samples/meeting-app/nodejs/api/server` and fill in the `[STORAGE ACCOUNT NAME]` and `[ACCESS KEY]` for azure table storage.
 

  - We have two different solutions to run so follow below steps:
 
    A) In a terminal, navigate to `samples/meeting-recruitment-app/nodejs/api`

    B) In a different terminal, navigate to `samples/meeting-recruitment-app/nodejs/clientapp`

     * In both the terminal run 
   
       npm install

       npm start

6. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appManifest folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `<<APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `<<BASE-URL>>` and replace `<<BASE-URL>>` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)    
    
- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appManifest folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. **Your app is uploaded to Teams**.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/meeting-recruitment-app/nodejs/api/server/index.js#L55) line and put your debugger for local debug.

## Running the sample

1) Details page:
   The details page shows basic information of the candidate, timeline, Questions (that can be added for meeting), Notes (provided by peers)

   ![Details](Images/details.png)

2) Action on Questions:
   
   - The interviewer can Add/Edit or Delete question.

   ![Add Question](Images/add_question.png)

   - Add Questions Task Module
   
   ![Add Question Task](Images/add_task.png)

   ![Edit Delete Question](Images/edit_questions.png)

   - Edit Question Task Module
   
   ![Edit Task](Images/edit_task.png)

3) Add Notes:
   
   The interviewer can add notes that will appear to other peers.

   ![Add Notes](Images/add_note.png)

   Add Note Task Module
  
   ![Add Notes](Images/add_note_task.png)

4) Sidepanel:
    
    The in-meeting side panel shows two sections as follows:
    
    A) Overview: Shows the basic details of the candidate.
    
    B) Questions: The questions set in the details page appear here. The interviewer can use this to provide rating and submit final feedback.

    ![Sidepanel Overview](Images/sidepanel_overview.png)

    ![Sidepanel Questions](Images/sidepanel_questions.png)

5) Share assets:

   This is used to share assets to the candidate.
   
   ![Share Assets](Images/share_assets.png)

6) Mobile view: Details tab

   ![Details tab](Images/details_tab_mobile.png)

   ![Note](Images/Note_mobile.png)

   ![Share Doc](Images/ShareDoc_mobile.png)
   
   - Sidepanel view
   
   ![Sidepanel Overview mobile](Images/sidepanel_mobile.png)

   ![Sidepanel Question mobile](Images/question_mobile.png)

## Deploy to Azure

Deploy your project to Azure by following these steps:

| From Visual Studio Code                                                                                                                                                                                                                                                                                                                                                  | From TeamsFx CLI                                                                                                                                                                                                                    |
| :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| <ul><li>Open Microsoft 365 Agents Toolkit, and sign into Azure by clicking the `Sign in to Azure` under the `ACCOUNTS` section from sidebar.</li> <li>After you signed in, select a subscription under your account.</li><li>Open the Microsoft 365 Agents Toolkit and click `Provision` from DEPLOYMENT section or open the command palette and select: `Teams: Provision`.</li><li>Open the Microsoft 365 Agents Toolkit and click `Deploy` or open the command palette and select: `Teams: Deploy`.</li></ul> | <ul> <li>Run command `teamsfx account login azure`.</li> <li>Run command `teamsfx provision --env dev`.</li> <li>Run command: `teamsfx deploy --env dev`. </li></ul> |

> Note: Provisioning and deployment may incur charges to your Azure Subscription.


## Further reading

- [Meeting apps APIs](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet)
- [Install the App in Teams Meeting](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meeting-recruitment-app-nodejs" />
