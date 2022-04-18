---
page_type: sample
description: This sample app demonstarte how an Bot can create a call, join a meeting and transfer the call
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:25"
---
# Calling and Meeting Bot Sample V4

## Summary

Calling and Meeting Bot provides basic functionality like Create Call, Join a call, Transfer/Redirect a call, Join a scheduled meeting and invite the participants by integrating cloud communications API Graph API.

## Frameworks

![drop](https://img.shields.io/badge/.NET&nbsp;Core-3.1-green.svg)
![drop](https://img.shields.io/badge/Bot&nbsp;Framework-3.0-green.svg)

## Prerequisites
* [Office 365 tenant](https://developer.microsoft.com/en-us/microsoft-365/dev-program)

* Microsoft Teams is installed and you have an account
* [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
* [ngrok](https://ngrok.com/) or equivalent tunnelling solution

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

1) If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/bot-calling-meeting/csharp` folder
  - Select `CallingBotSample.csproj` file

1) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```
    
## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**

---    

## Setup Calling and Meeting Bot

* [Register an App](https://docs.microsoft.com/en-us/graph/auth-register-app-v2) in Azure using demo tenant 
* Create `client_secret` for your app
* Copy `client_Id` `client_secret` for your app in Notepad.
 - Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

* Create a policy for a demo tenant user for creating the online meeting on behalf of that user using the following powershell script

```
	Import-Module MicrosoftTeams
	$userCredential = Get-Credential
	Connect-MicrosoftTeams -Credential $userCredential

	New-CsApplicationAccessPolicy -Identity “<<policy-identity/policy-name>>” -AppIds "<<azure-client-id>>" -Description "<<Policy-description>>"
	Grant-CsApplicationAccessPolicy -PolicyName “<<policy-identity/policy-name>>” -Identity "<<object-id-of-the-user-to-whom-policy-need-to-be-granted >>"

  ex:
  Import-Module MicrosoftTeams
	$userCredential = Get-Credential
	Connect-MicrosoftTeams -Credential $userCredential

	New-CsApplicationAccessPolicy -Identity Meeting-policy-dev -AppIds "d0bdaa0f-8be2-4e85-9e0d-2e446676b88c" -Description "Online meeting policy - contoso town"
	Grant-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Identity "782f076f-f6f9-4bff-9673-ea1997283e9c"
	
```
* Update `PolicyName`, `azure-client-id`, `policy-description`, `object-id for user` in powershell script.
* Run `Windows Powershell PSI` as an administrator and execute above script.
* Run following command to verify policy is create successfully or not
`Get-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Identity "<<azure-client-Id>>"
	`
* Add following Graph API Applications permissions to your Azure App.
- `Calls.AccessMedia.All`
- `Calls.Initiate.All`
- `Calls.InitiateGroupCall.All`
- `Calls.JoinGroupCall.All`
- `Calls.JoinGroupCallAsGuest.All`
- `OnlineMeetings.ReadWrite.All`

* Grant Admin consent for the above permissions

* Create `Bot Channel Registeration` in Azure account which have subscription enabled.
* Provide `App-Name`, `Resource Group` and other required information
* Update messgaing endpoint `https://{yourngrok}/api/messages` 
* Click on Create AppId and Secret
* Enter `Client_Id and Client_Secret` of your azure app registered in demo tenant
* Add the Teams channel.
* Select the Calling tab on the Teams channel page. Select Enable calling, and then update Webhook (for calling) with your HTTPS URL (`https://yourNgrok/callback`) where you receive incoming notifications, for example `https://contoso.com/teamsapp/callback`.
![image](https://user-images.githubusercontent.com/50989436/122867490-375e5580-d347-11eb-8447-7e417947bf1f.png)
* Save your changes.

## Update appsetting.json for calling Bot
````
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "BotBaseUrl": "https://{yourngrok}.ngrok.io",
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": ""
  },
  "Bot": {
    "AppId": "",
    "AppSecret": "",
    "PlaceCallEndpointUrl": "https://graph.microsoft.com/v1.0",
    "BotBaseUrl": "https://{yourngrok}.ngrok.io",
    "GraphApiResourceUrl": "https://graph.microsoft.com",
    "MicrosoftLoginUrl": "https://login.microsoftonline.com/"
  },
  "UserId": "",
  "Users": [
    {
      "DisplayName": "",
      "Id": ""
    },
    {
      "DisplayName": "",
      "Id": ""
    },
    {
      "DisplayName": "",
      "Id": ""
    }
  ]
}
````
- Update `MicrosoftAppId, MicrosoftAppPassword, AppId, AppSecret` with your client_id and client_secret app registered in demo tenant.
- Update `BotBaseUrl` with your `ngrok` URL.
- Update `UserId` and `DisplayName` of the users from where you want to initiate the call and to whom you want to redirect or transfer the call

## Create Teams App Package and upload it for organisation
- Open your project in Visual Studio
- Go to `Manifest folder`
- Update your Bot Id with client_Id and base URL with `ngrok URL`.
- Zip your manifest along with two icons
- upload your manifest in your demo tenant for organisation in run your solution in Visual studio.

## Interaction with Calling and Meeting Bot

* Install Calling Bot in Teams
![image](https://user-images.githubusercontent.com/50989436/122866700-0c273680-d346-11eb-9c30-83ff23f019e0.png)

* Bot will throw welcome Text with Adaptive Card as mentioned below
![image](https://user-images.githubusercontent.com/50989436/122866848-4395e300-d346-11eb-8e20-d43629d22aaa.png)

* User can ask Bot to Create a call, Transfer a call
![image](https://user-images.githubusercontent.com/50989436/122867719-92904800-d347-11eb-87a6-3d61c24c6451.png)

* User can ask Bot to schedule a Meeting and invite the participants
![image](https://user-images.githubusercontent.com/50989436/122867010-848df780-d346-11eb-9129-4447e39d35f5.png)

## Further reading
- [Register a calling Bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/calls-and-meetings/registering-calling-bot#create-new-bot-or-add-calling-capabilities)
- [Cloud Communications API](https://docs.microsoft.com/en-us/graph/api/resources/call?view=graph-rest-1.0)


