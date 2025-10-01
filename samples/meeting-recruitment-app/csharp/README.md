---
page_type: sample
description: "This sample illustrates a meeting experience for recruitment scenario using Apps In Meetings. This app also uses bot for sending notifications."
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
  contentType: samples
  createdDate: "10/01/2021 02:36:57 PM"
urlFragment: officedev-microsoft-teams-samples-meeting-recruitment-app-csharp
---

# Recruitment App Sample using Apps in Meetings

This sample illustrates a meeting experience for recruitment.

It has meeting details and in-meeting app that helps in the interview process.

## Included Features
* Bots
* Meeting Chat 
* Meeting Sidepanel 
* Meeting Details

## Interaction with app

![Details](MeetingApp/Images/meetingrecruitment.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app manifest (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Recruitment App Sample:** [Manifest](/samples/meeting-recruitment-app/csharp/demo-manifest/Meeting-Recruitment-App.zip)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
-[Microsoft 365 Agents Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

##Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio.

1. Install Visual Studio 2022 **Version 17.14 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Microsoft 365 Agents Toolkit for Visual Studio Microsoft 365 Agents Toolkit extension
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an     existing public dev tunnel.
1. Create a Azure Storage account(This is needed to store/retrieve data that's used in the app) 
     [Create storage account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal)

      This step will create a storage account. You will require storage account name and keys in next steps.
  
      Please follow [View account keys](https://docs.microsoft.com/azure/storage/common/storage-account-keys-manage?tabs=azure-portal#view-account-access-keys)  to see the keys info. 
      Update {{ StorageConnectionString }} in `/appsettings.json`.
1. In the debug dropdown menu of Visual Studio, select default startup project > Microsoft Teams (browser)
1. Right-click the 'M365Agent' project in Solution Explorer and select **Microsoft 365 Agents Toolkit > Select Microsoft 365 Account**
1. Sign in to Microsoft 365 Agents Toolkit with a **Microsoft 365 work or school account**
1. Set `Startup Item` as `Microsoft Teams (browser)`.
1. Press F5, or select Debug > Start Debugging menu in Visual Studio to start your app
    </br>![image](https://raw.githubusercontent.com/OfficeDev/TeamsFx/dev/docs/images/visualstudio/debug/debug-button.png)
1. In the opened web browser, select Add button to install the app in Teams
> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

1.Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
   Fill out name and select third option for supported account type and click "Register".

   ![AppRegistration](MeetingApp/Images/AppRegistration.png)

   * Copy and paste the App Id and Tenant ID somewhere safe. You will need it in a future step.

  - Create Client Secret.
   * Navigate to the "Certificates & secrets" blade and add a client secret by clicking "New Client Secret".

   ![ClientSecret](MeetingApp/Images/clientsecret.png) 

   * Copy and paste the secret somewhere safe. You will need it in a future step.

   Navigate to **API Permissions**, and make sure to add the follow permissions:
    * Select Add a permission
    * Select Microsoft Graph -> Delegated permissions.
    * `User.Read` (enabled by default)
    * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

   
2. Setup for Bot

    - Also, register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
  - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
  - While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

      > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.
      
3. Create a Azure Storage account(This is needed to store/retrieve data that's used in the app) 
     [Create storage account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal)

      This step will create a storage account. You will require storage account name and keys in next steps.
  
      Please follow [View account keys](https://docs.microsoft.com/azure/storage/common/storage-account-keys-manage?tabs=azure-portal#view-account-access-keys)  to see the keys info. 
     

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

- Modify the `/appsettings.json` and fill in the following details:
    - `{{MicrosoftAppId}}` - Generated from Step 1 while doing Microsoft Entra ID app registration in Azure portal.
    - `{{ MicrosoftAppPassword}}` - Generated from Step 1, also referred to as Client secret
    - `{{ StorageConnectionString }}` - Generated from Step 3,Create a Azure Storage accoun

 - Run the app from a terminal or from Visual Studio, choose option A or B.

     A) From a terminal

    ```bash
    # run the app
    dotnet run
    ```

    B) Or from Visual Studio

    - Launch Visual Studio
    - File -> Open -> Project/Solution
    - Navigate to `MeetingApp` folder
    - Select `MeetingApp.csproj` file
    - Press `F5` to run the project

6. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `<<APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `<<BASE-URL>>` and replace `<<BASE-URL>>` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)    
    
- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/meeting-recruitment-app/csharp/MeetingApp/AdapterWithErrorHandler.cs#L25) line and put your debugger for local debug.

## Running the sample

1) Details page:
   The details page shows basic information of the candidate, timeline, Questions (that can be added for meeting), Notes (provided by peers)

   ![Details](MeetingApp/Images/details.png)

2) Action on Questions:
   
   - The interviewer can Add/Edit or Delete question.

   ![Add Question](MeetingApp/Images/add_question.png)

   - Add Questions Task Module
   
   ![Add Question Task](MeetingApp/Images/add_task.png)

   ![Edit Delete Question](MeetingApp/Images/edit_questions.png)

   - Edit Question Task Module
   
   ![Edit Task](MeetingApp/Images/edit_task.png)

3) Add Notes:
   
   The interviewer can add notes that will appear to other peers.

   ![Add Notes](MeetingApp/Images/add_note.png)

   Add Note Task Module
  
   ![Add Notes](MeetingApp/Images/add_note_task.png)

4) Sidepanel:
    
    The in-meeting side panel shows two sections as follows:
    
    A) Overview: Shows the basic details of the candidate.
    
    B) Questions: The questions set in the details page appear here. The interviewer can use this to provide rating and submit final feedback.

    ![Sidepanel Overview](MeetingApp/Images/sidepanel_overview.png)

    ![Sidepanel Questions](MeetingApp/Images/sidepanel_questions.png)

5) Share assets:

   This is used to share assets to the candidate.
   
   ![Share Assets](MeetingApp/Images/share_assets.png)

6) Mobile view: Details tab

   ![Details tab](MeetingApp/Images/details_tab_mobile.png)

   ![Note](MeetingApp/Images/Note_mobile.png)

   ![Share Doc](MeetingApp/Images/ShareDoc_mobile.png)
   
   - Sidepanel view
   
   ![Sidepanel Overview mobile](MeetingApp/Images/sidepanel_mobile.png)

   ![Sidepanel Question mobile](MeetingApp/Images/question_mobile.png)


## Further reading

- [Meeting apps APIs](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/meeting-apps-apis?tabs=dotnet)
- [Install the App in Teams Meeting](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/meeting-recruitment-app-csharp" />