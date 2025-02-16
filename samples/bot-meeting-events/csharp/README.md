# MeetingEventsCallingBot

## Summary
This sample Bot adds itself to the call when meeting "Start" event is received and drops/removes itself from the call when all the participants have left the call. Meeting "End" event is received when the bot drops the call. The Bot also tracks participants in the call.<br/>
`OnEventActivityAsync` is called on meeting start/end event.<br/>
Samples are generally not production-ready or an out-of-the-box solution but are intended to show developers specific patterns for use in their applications. The functionality is bare bone, all it does is tracks participants in the call once the bot is added to the meeting. 

## Prerequisites
- To test locally, you need [Ngrok](https://ngrok.com/download) installed on your local machine. Make sure you've downloaded and installed both on your local machine. ngrok will tunnel requests from the Internet to your local computer and terminate the SSL connection from Teams.<br/>
`ex: https://%subdomain%.ngrok.io` 

## Setup Guide

### Step #1: Create AAD App
- In Azure portal, go to Azure Active Directory -> App registrations -> New registration.
- Register an app.
	- Provide app Name and choose Multitenant. Leave the redirect URI for now.<br/>
	![](./Images/Setup_Step1_1.png)
	- Create Client Secret<br/>
	![](./Images/Setup_Step1_2.png)<br/>
	- Note the Application (Client) Id and Client Secret for further steps.

### Step #2: Register in Bot Service
- Create a Bot Channel Registration in Azure in different tab. Auto create App ID and password<br/>
![](./Images/Setup_Step2_1.png)<br/>
- Create new Microsoft App Id and password.<br/>
![](./Images/Setup_Step2_2.png)<br/>
Fill Microsoft App Id and Password as values of Application Id and Client Secret from Step #1.
- Set calling configuration in Teams channel. Add webhook for calling. `ex. https://%subdomain%.ngrok.io`<br/>
![](./Images/Setup_Step2_3.png)

### Step #3: Configure AAD App 
Configure the AAD app created in Step #1. 
- Add necessary API permissions to the app.<br/>
Go to API permissions -> Add a permission -> Microsoft Graph
![](./Images/Setup_Step3_1.png)
	- Add following Application permissions for Call
		- Calls.Initiate.All
		- Calls.InitiateGroupCalls.All
		- Calls.JoinGroupCalls.All
		- Calls.JoinGroupCallsasGuest.All
	- Add following Delegated permissions for User
		- User.Read<br/>
For more details on adding above graph permissions, checkout [Call Permissions](https://docs.microsoft.com/en-us/graph/api/call-answer?view=graph-rest-beta&tabs=http#permissions) and [User Permissions](https://docs.microsoft.com/en-us/graph/api/user-get?view=graph-rest-1.0&tabs=http#permissions)
- Add Redirect URI
	- Select Add a platform -> web
	- Enter the redirect URI for your app `ex. https://%subdomain%.ngrok.io/`
	- Next, Enable implicit grant by checking ID Token and Access Token
![](./Images/Setup_Step3_2.png)<br/>
- Consent the permissions
    - Go to "https://login.microsoftonline.com/common/adminconsent?client_id=<app_id>&state=<any_number>&redirect_uri=<app_redirect_url>"
    - Sign in with a tenant admin
    - Consent for the whole tenant.

### Step #4: Enable resource-specific consent (RSC) in your app
- Sign in to the Azure portal as tenant admin.
- Select Azure Active Directory -> Enterprise applications -> Consent and permissions -> User consent settings.
- Allow group owner consent for all group owners. For a team owner to install an app using RSC, group owner consent must be enabled for that user.
![](./Images/Setup_Step4_1.png)<br/>
- Allow user consent for apps. For a chat member to install an app using RSC, user consent must be enabled for that user.
![](./Images/Setup_Step4_2.png)<br/>
- To understand more about enabling RSC permissions, checkout [RSC](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent#enable-resource-specific-consent-in-your-application)

### Step #5: Run the app locally
- Clone the repo <br/>
`git@github.com:shsarda/MeetingEventsCallingBot.git`

- If you are using Visual Studio
	- Launch Visual Studio
	- File -> Open -> Project/Solution
	- Open MeetingEventsCallingBot -> MeetingEventsCallingBot.sln

- Edit appsettings.json file
	- Update MicrosoftAppId/AppId and MicrosoftAppId/AppSecret as values of Application Id and Client Secret from Step #1.
	- Update BotBaseURL `ex. https://%subdomain%.ngrok.io`

- Run ngrok using the port on which the project is running locally.

### Step #6: Packaging and installing your app to Teams

- Enabling "supportsCalling" in manifest
	- Add two additional settings supportsCalling and supportsVideo (already enabled in manifest for this sample)
	![](./Images/Setup_Step6_1.png)<br/>

- Enable RSC permission in manifest
	- Enable `OnlineMeeting.ReadBasic.Chat` in manifest (already enabled in manifest for this sample)
	![](./Images/Setup_Step6_2.png)<br/>

Make sure the required values (such as App id) are populated in the manifest, Zip the manifest with the profile images and install it in Teams.

### Step #7: Try out the app

- Enable developer preview in your debugging Teams client.
![](./Images/Setup_Step7_1.png)

- Add app to a scheduled meeting. Join the meeting to use the app.
![](./Images/Setup_Step7_2.png)

