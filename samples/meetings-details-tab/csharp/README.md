---
page_type: sample
description: Microsoft Teams meeting extensibility sample for iteracting with Details Tab in-meeting
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-meetings-details-tab-csharp
---

# Meetings Details Tab

This sample shows creating poll in meeting , where memebers of the meeting can answer poll question and can see the results.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## To try this sample

- Register your app with Microsoft identity platform via the Azure AD portal
  - Your app must be registered in the Azure AD portal to integrate with the Microsoft identity platform and call Microsoft Graph APIs. See [Register an application with the Microsoft identity platform](https://docs.microsoft.com/en-us/graph/auth-register-app-v2).

  - Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.
  
- Clone the repository 
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

- Build your solution

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/meetings-details-tab/csharp` folder
  - Select `DetailsTab.csproj` file
  - Press `F5` to run the project

- Setup ngrok
  ```bash
  ngrok http -host-header=rewrite 3978
  ```
- Go to appsettings.json and add ```MicrosoftAppId```, ```MicrosoftAppPassword``` and ```BaseUrl``` information.
- Update the manifest.json file with ```Microsoft-App-ID``` and ```BaseUrl``` value.
- Run your app, either from Visual Studio with ```F5``` or using ```dotnet run``` in the appropriate folder.
- [Install the App in Teams Meeting](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)

## Interacting with the app in Teams Meeting
Interact with Details Tab in Meeting.
1. Install the Details Tab manifest in meeting chat.
2. Add the Details Tab in Meeting
3. Click on `Add Agenda`
4. Newly added agenda will be added to Tab.
![image](https://user-images.githubusercontent.com/50989436/120268903-5af02c00-c2c4-11eb-9061-c8af7436715e.png)
5. Click on Send button in Agenda from Tab.
6. An Adaptive Card will be posted in meeting chat for feedback
![image](https://user-images.githubusercontent.com/50989436/120431715-7c214d00-c396-11eb-8919-0dbb6192ce22.png)

7. Participants in meeting can submit their response in adaptive card
8. Response will be recorded and Bot will send an new adaptive card with response.
![image](https://user-images.githubusercontent.com/50989436/120431763-92c7a400-c396-11eb-8daf-dce922b380ad.png)
9. Participants in meeting can view the results from meeting chat or Tab itself.
 
  
 
  
  
 
