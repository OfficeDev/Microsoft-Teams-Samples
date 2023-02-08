---
page_type: sample
description: This sample shows Anonymous Users Support In Meeting.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "08-02-2023 10:00:01"
urlFragment: officedev-microsoft-teams-samples-app-anonymous-users-csharp

---

### Anonymous User Support

This sample shows Anonymous Users Support In Meeting.

**Interaction with bot**
![appanonymoususersGif](Images/appanonymoususersGif.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Anonymous Users:** [Manifest](/samples/app-anonymous-users/csharp/demo-manifest/app-anonymous-users.zip)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup

> Note these instructions are for running the sample on your local machine.

1. Run ngrok - point to port 3978

   ```bash
     ngrok http -host-header=rewrite 3978
   ```  

2. Setup

   **Register your application with Azure AD:**

    - Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

    - Select **New Registration** and on the *register an application page*, set following values:
           * Set **name** to your app name.
           * Choose the **supported account types** (any account type will work)
           * Leave **Redirect URI** empty.
           * Choose **Register**.

    - On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.

    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

3. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
    
4. Run the app from a terminal or from Visual Studio, choose option A or B.

    A) From a terminal, navigate to `samples/app-anonymous-users/csharp`

    ```bash
    # run the app
    dotnet run
    ```
    B) Or from Visual Studio

    - Launch Visual Studio
    - File -> Open -> Project/Solution
    - Navigate to `samples/app-anonymous-users/csharp` folder
    - Select `AnonymousUsers.sln` file
    - Press `F5` to run the project

5. In a terminal, navigate to `samples/app-anonymous-users/csharp/ClientApp`

    - Inside ClientApp folder execute the below command.

        ```bash
        # npm install
        ```

 6) Modify the `/appsettings.json` and fill in the following details:
  - `{{ MicrosoftAppType }}` - **Allowed values are: MultiTenant(default), SingleTenant, UserAssignedMSI**
  - `{{ MicrosoftAppId }}` - Generated from Step 2 is the application app id
  - `{{ MicrosoftAppPassword }}` - Generated from Step 2, also referred to as Client secret
  - `{{ MicrosoftAppTenantId }}` - Generated from Step 2 is the tenantId id

 7. __*This step is specific to Teams.*__

- **Edit** the `manifest.json` contained in the  `TeamsAppManifest` folder to replace your Microsoft App Id `<<YOUR-MICROSOFT-APP-ID>>` (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)

- **Edit** the `manifest.json` for `{{domain-name}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.

- **Zip** up the contents of the `TeamsAppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)

- Add the app to team/groupChat scope (Supported scopes). 

**Note:**
-   If you are facing any issue in your app,  [please uncomment this line](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/7336b195da6ea77299d220612817943551065adb/samples/app-anonymous-users/csharp/AdapterWithErrorHandler.cs#L27) and put your debugger for local debug.

## Running the sample

You can interact with Teams Tab meeting sidepanel.

**Install app:**

![InstallApp ](Images/InstallApp.png)

**Add to a meeting:**

![AddMeetingInstall ](Images/AddMeetingInstall.png)

**Select meeting:**

![AddToMeeting ](Images/AddToMeeting.png)

**Add app in a meeting tab:**

![MeetingTab ](Images/MeetingTab.png)

**Select vote:**

![Vote0 ](Images/Vote0.png)

![VotePage1 ](Images/VotePage1.png)

**Submit vote:**

![SubmitVote2 ](Images/SubmitVote2.png)

**Vote successfully:**

![SuccessVote3 ](Images/SuccessVote3.png)

**Select CreateConversation:**

![CreateConver4 ](Images/CreateConver4.png)

**All message have been send:**

![SendConver7 ](Images/SendConver7.png)

**Send user conversation mmessage:**

![SendAllUserConver5 ](Images/SendAllUserConver5.png)

**Send next user conversation mmessage:**

![NextuserSendConver6 ](Images/NextuserSendConver6.png)

**Join meeting and Click share invite:**

![CopyMeetingLink8 ](Images/CopyMeetingLink8.png)

**Paste the URL and create a guest user:**

![CreateGuestUser9 ](Images/CreateGuestUser9.png)

**Accept guest user:**

![AccepetGuestUser10 ](Images/AccepetGuestUser10.png)

**CreateConversation guest user:**

![CreateConverAnoUser11 ](Images/CreateConverAnoUser11.png)

**Add app:**

![AddApp12 ](Images/AddApp12.png)

**Add app in a meeting tab:**

![MeetingTab13 ](Images/MeetingTab13.png)

**Share to stage view:**

![SharePage14 ](Images/SharePage14.png)

**Click share to stage view:**

![UserOneSharing15 ](Images/UserOneSharing15.png)

**Screen visible anonymous users:**

![AnoUserSharing16 ](Images/AnoUserSharing16.png)

**Tenant user submit vote:**

![SubmitVote17 ](Images/SubmitVote17.png)

**Anonymous users screen cout auto update:**

![AnoUserSubmitVote18 ](Images/AnoUserSubmitVote18.png)

**Anonymous user submit vote:**

![AnoCoutCheck19 ](Images/AnoCoutCheck19.png)

**Tenant users screen cout auto update:**

![UserCountCheck20 ](Images/UserCountCheck20.png)

**Remove guest user:**

![RemoveGuestuser21 ](Images/RemoveGuestuser21.png)

**Confirm message:**

![RemovePop22 ](Images/RemovePop22.png)

**The anonymous user was removed from team:**

![SuccessRemovedMessage23 ](Images/SuccessRemovedMessage23.png)

**Remove tenant user:**

![RemoveNormalUser24 ](Images/RemoveNormalUser24.png)

**The tenant user was removed from team:**

![UserRemoverSuccess25 ](Images/UserRemoverSuccess25.png)

**Add users:**

![Adduser26 ](Images/Adduser26.png)

**Welcome to the team:**

![SuccessAddUser27 ](Images/SuccessAddUser27.png)


## Further reading

- [Build apps for anonymous users](https://learn.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/build-apps-for-anonymous-user?branch=pr-en-us-7318&tabs=javascript)


