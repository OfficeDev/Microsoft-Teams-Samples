---
page_type: sample
description: This sample app demonstrates how a Bot can create a call, join a meeting and transfer the call
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:25 PM"
urlFragment: officedev-microsoft-teams-samples-bot-calling-meeting-csharp
---
# Calling and Meeting Bot Sample V4

## Summary

Calling and Meeting Bot provides basic functionality like Create Call, Join a call, Transfer/Redirect a call, Join a scheduled meeting and invite the participants by integrating cloud communications API Graph API.
- **Interaction with bot**
![bot-calling-meeting ](Images/bot-calling-meeting.gif)
## Frameworks

![drop](https://img.shields.io/badge/.NET&nbsp;Core-6.0-green.svg)
![drop](https://img.shields.io/badge/Bot&nbsp;Framework-3.0-green.svg)

## Prerequisites

* [Office 365 tenant](https://developer.microsoft.com/en-us/microsoft-365/dev-program)

* Microsoft Teams is installed and you have an account
* [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0
* [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## Setup

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
2) If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/bot-calling-meeting/csharp` folder
  - Select `CallingBotSample.csproj` file

3) Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    ```

## Register Azure AD application
Register one Azure AD application in your tenant's directory for the bot and tab app authentication.

1.  Log in to the Azure portal from your subscription, and go to the "App registrations" blade  [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps). Ensure that you use a tenant where admin consent for API permissions can be provided.

2.  Click on "New registration", and create an Azure AD application.

3.  **Name:**  The name of your Teams app - if you are following the template for a default deployment, we recommend "App catalog lifecycle".

4.  **Supported account types:**  Select "Accounts in any organizational directory"

5.  Leave the "Redirect URL" field blank.   

6.  Click on the "Register" button.

7.  When the app is registered, you'll be taken to the app's "Overview" page. Copy the  **Application (client) ID**; we will need it later. Verify that the "Supported account types" is set to **Multiple organizations**.

8.  On the side rail in the Manage section, navigate to the "Certificates & secrets" section. In the Client secrets section, click on "+ New client secret". Add a description for the secret and select Expires as "Never". Click "Add".

9.  Once the client secret is created, copy its **Value**, please take a note of the secret as it will be required later.


At this point you have 3 unique values:
-   Application (client) ID which will be later used during Azure bot creation
-   Client secret for the bot which will be later used during Azure bot creation
-   Directory (tenant) ID


We recommend that you copy these values into a text file, using an application like Notepad. We will need these values later.

10.  Under left menu, navigate to **API Permissions**, and make sure to add the following permissions of Microsoft Graph API > Application permissions:
- `Calls.AccessMedia.All`
- `Calls.Initiate.All`
- `Calls.InitiateGroupCall.All`
- `Calls.JoinGroupCall.All`
- `Calls.JoinGroupCallAsGuest.All`
- `OnlineMeetings.ReadWrite.All`

Click on Add Permissions to commit your changes.

11.  If you are logged in as the Global Administrator, click on the "Grant admin consent for <%tenant-name%>" button to grant admin consent else, inform your admin to do the same through the portal or follow the steps provided here to create a link and send it to your admin for consent.
    
12.  Global Administrator can grant consent using following link:  [https://login.microsoftonline.com/common/adminconsent?client_id=](https://login.microsoftonline.com/common/adminconsent?client_id=)<%appId%> 
13. Create a policy for a demo tenant user for creating the online meeting on behalf of that user using the following PowerShell script

```
	Import-Module MicrosoftTeams
	# Calling Connect-MicrosoftTeams using no parameters will open a window allowing for MFA accounts to authenticate
	Connect-MicrosoftTeams

	New-CsApplicationAccessPolicy -Identity “<<policy-identity/policy-name>>” -AppIds "<<microsoft-app-id>>" -Description "<<policy-description>>"
	Grant-CsApplicationAccessPolicy -PolicyName “<<policy-identity/policy-name>>” -Identity "<<object-id-of-the-user-to-whom-policy-need-to-be-granted>>"

  eg:
  Import-Module MicrosoftTeams
	Connect-MicrosoftTeams

	New-CsApplicationAccessPolicy -Identity Meeting-policy-dev -AppIds "d0bdaa0f-8be2-4e85-9e0d-2e446676b88c" -Description "Online meeting policy - contoso town"
	Grant-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Identity "782f076f-f6f9-4bff-9673-ea1997283e9c"
```
![PolicySetup](Images/PolicySetup.PNG)

14. Update `PolicyName`, `microsoft-app-id`, `policy-description`, `object-id-of-the-user-to-whom-policy-need-to-be-granted` in powershell script.
15. Run `Windows Powershell PSI` as an administrator and execute above script.
16. Run following command to verify policy is create successfully or not
`Get-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Identity "<<microsoft-app-id>>"`
## Setup Bot Service

1. In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration)
2. Select Type of App as "Multi Tenant"
3.  Select Creation type as "Use existing app registration"
4. Use the copied App Id and Client secret from above step and fill in App Id and App secret respectively.
5. Click on 'Create' on the Azure bot.   
6. Go to the created resource, ensure that you've [enabled the Teams Channel](https://learn.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
7. In Settings/Configuration/Messaging endpoint, enter the current `https` URL you have given by running ngrok. Append with the path `/api/messages`     
8. Select the Calling tab on the Teams channel page. Select Enable calling, and then update Webhook (for calling) with your HTTPS URL (`https://yourNgrok/callback`) where you receive incoming notifications.
For example `https://contoso.com/teamsapp/callback`
![EnableCallingEndpoint ](Images/EnableCallingEndpoint.PNG)
9. Save your changes.

### Configuring the sample:
1. __*Update appsettings.json for calling Bot*__
````
{
  "MicrosoftAppId": "<<microsoft-app-id>>",
  "MicrosoftAppPassword": "<<microsoft-app-client-secret>>",
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<<tenant-Id>>",
    "ClientId": "<<microsoft-app-id>>",
    "ClientSecret": "<<microsoft-app-client-secret>>"
  },
  "Bot": {
    "AppId": "<<microsoft-app-id>>",
    "AppSecret": "<<microsoft-app-client-secret>>",
    "PlaceCallEndpointUrl": "https://graph.microsoft.com/v1.0",
    "BotBaseUrl": "https://<<subdomain>>.ngrok-free.app",
    "GraphApiResourceUrl": "https://graph.microsoft.com",
    "MicrosoftLoginUrl": "https://login.microsoftonline.com/",
    "RecordingDownloadDirectory": "temp",
    "CatalogAppId": "<<microsoft-app-id>>"
  },
  "CognitiveServices": {
    "Enabled": false,
    "SpeechKey": "<<cognitive-speech-key>>",
    "SpeechRegion": "<<cognitive-speech-region>>",
    "SpeechRecognitionLanguage": "<<cognitive-speech-language>>" 
  },
  "Users": {
    "UserIdWithAssignedOnlineMeetingPolicy": "<<object-id-of-the-user-to-whom-online-meeting-policy-has-been-granted>>"
  }
}
````
- Update `microsoft-app-id`, `tenant-Id`, `microsoft-app-client-secret` with your app's client id and client secret registered in demo tenant.
- Update `BotBaseUrl` with your `ngrok` URL.
- Update `object-id-of-the-user-to-whom-online-meeting-policy-has-been-granted` with the ID of the user who has had the policy assigned to them above

**Create a Cognitive Services resource using the Azure portal:**
 [Create Cognitive Services resource ](https://learn.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account?tabs=multiservice%2Canomaly-detector%2Clanguage-service%2Ccomputer-vision%2Cwindows)
- Update `cognitive-speech-key` replace `your-key `with one of the keys for your resource.
- Update `cognitive-speech-key` replace `your-region `with one of the regions for your resource.
- Update `cognitive-speech-language` replace `your-language `with one of the language for your resource.

2. __*This step is specific to Teams*__
    - **Edit** the `manifest.json` contained in the  `TeamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
   - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `TeamsAppManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal/team/groupChat scope (Supported scopes)

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/bot-calling-meeting/csharp/Source/CallingBotSample/AdapterWithErrorHandler.cs#L22) line and put your debugger for local debug.

 ## Running the sample

* Install app in personal teams.
![CallingBotInstallation ](Images/BotCallingMeeting1.png)

* Select welcome card. 
![BotCallingMeeting2 ](Images/BotCallingMeeting2.png)

* Bot will send adaptive card as mentioned below. 
![BotCallingMeeting3 ](Images/BotCallingMeeting3.png)

* Select 'Create Call'. 
![BotCallingMeeting4 ](Images/BotCallingMeeting4.png)

* 'Calling Bot' selected user. 
![BotCallingMeeting5 ](Images/BotCallingMeeting5.png)

* User join call. 
![BotCallingMeeting55 ](Images/BotCallingMeeting55.png)

* Successfully call . 
![BotCallingMeeting6 ](Images/BotCallingMeeting6.png)

* Control this meeting. 
![BotCallingMeeting7 ](Images/BotCallingMeeting7.png)

* Create Incident. 
![BotCallingMeeting8 ](Images/BotCallingMeeting8.png)

* Transfer call. 
![BotCallingMeeting9 ](Images/BotCallingMeeting9.png)

* 'Calling Bot' selected user. 
![BotCallingMeeting10 ](Images/BotCallingMeeting10.png)

* Play record prompt. 
![BotCallingMeeting11 ](Images/BotCallingMeeting11.png)

* Hang up. 
![BotCallingMeeting12 ](Images/BotCallingMeeting12.png)

* Install app in team.
![BotCallingMeeting13 ](Images/BotCallingMeeting13.png)

* Add calling bot to a team. 
![BotCallingMeeting14 ](Images/BotCallingMeeting14.png)

* Bot will send adaptive card as mentioned below. 
![BotCallingMeeting16 ](Images/BotCallingMeeting16.png)

* Select 'Create Call'. 
![BotCallingMeeting17 ](Images/BotCallingMeeting17.png)

* User join call. 
![BotCallingMeeting18 ](Images/BotCallingMeeting18.png)

* Control this meeting. 
![BotCallingMeeting19 ](Images/BotCallingMeeting19.png)

* Install app in meeting.
![BotCallingMeeting20 ](Images/BotCallingMeeting20.png)

* Add calling bot to a meeting. 
![BotCallingMeeting21 ](Images/BotCallingMeeting21.png)

* Bot will send adaptive card as mentioned below. 
![BotCallingMeeting22 ](Images/BotCallingMeeting22.png)

* Select 'Create Call'. 
![BotCallingMeeting23 ](Images/BotCallingMeeting23.png)

* User join call.  
![BotCallingMeeting24 ](Images/BotCallingMeeting24.png)

* Control this meeting. 
![BotCallingMeeting25 ](Images/BotCallingMeeting25.png)

* Join scheduled meeting. 
![BotCallingMeeting26 ](Images/BotCallingMeeting26.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**

## Further reading
- [Register a calling Bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/calls-and-meetings/registering-calling-bot#create-new-bot-or-add-calling-capabilities)
- [Cloud Communications API](https://docs.microsoft.com/en-us/graph/api/resources/call?view=graph-rest-1.0)
- [Recognize and convert speech to text](https://learn.microsoft.com/en-us/azure/cognitive-services/speech-service/get-started-speech-to-text?pivots=programming-language-csharp&tabs=windows%2Cterminal)
