---
page_type: sample
description: Sample app which demonstrates how to use live share SDK inside meeting side panel.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-meetings-sidepanel-csharp
---

# Meetings SidePanel

This sample illustrates how to implement [Side Panel](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/create-apps-for-teams-meetings?view=msteams-client-js-latest&tabs=dotnet#notificationsignal-api) In-Meeting Experience and uses [Live Share SDK](https://aka.ms/livesharedocs) to share data in realtime.

## Interaction with app.

![Customform](SidePanel/Images/SidePanelModule.gif)

  
### User interactions(Meeting Organizer)
- **Add New Agenda Item** - Gives provision to add new Agenda point.
- **Add** - Adds the agenda from Textinput to the SidePanel agenda list.
- **Publish Agenda** - Sends the agenda list to the meeting chat.



## Prerequisites

### Tools

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
  ```bash
  # determine dotnet version
  dotnet --version
  ```
## Setup.

  - [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  ```bash
  # run ngrok locally
  ngrok http -host-header=localhost 3001
  ```

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```
2. If you are using Visual Studio
- Launch Visual Studio
- File -> Open -> Project/Solution
- Navigate to ```samples\meetings-sidepanel\csharp``` folder
- Select ```SidePanel.sln``` file
3. Run ngrok - point to port 3001
   ```ngrok http -host-header=rewrite 3978```
4. Create a new Bot by following steps mentioned in [Build a bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots?view=msteams-client-js-latest#build--a-bot-for-teams-with-the-microsoft-bot-framework) documentation.
5. Go to appsettings.json and add ```MicrosoftAppId```, ```MicrosoftAppPassword``` and ```BaseUrl``` information.
6. Update the manifest.json file with ```Microsoft-App-ID``` and ```BaseUrl``` value.
7. Run your app, either from Visual Studio with ```F5``` or using ```dotnet run``` in the appropriate folder.
8. Navigate to ```samples\meetings-sidepanel\csharp\ClientApp``` folder and execute the below command.

    ```bash
    # npx @fluidframework/azure-local-service@latest
    ```
9. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.


## Running the sample

1. Welcome image to added side panel.
![Customform](SidePanel/Images/welcome.png)

2. Screen ready to added the agenda.
![AddNewAgenda](SidePanel/Images/addnewagenda.png)

3. On click of "Add" button, agenda point will be added to the agenda list.
![AgendaSubmit](SidePanel/Images/addagendabutton.png)

4. On click of "Publish Agenda", the agenda list will be sent to the meeting chat.
![AgendaCard](SidePanel/Images/agendacard.png)

## Deploy the bot to Azure

-  To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further Reading.

- [Meeting Live Caption](https://learn.microsoft.com/en-us/microsoftteams/platform/sbs-meetings-sidepanel?tabs=vs)

