# SidePanel Sample

This sample illustrates how to implement [Side Panel](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/create-apps-for-teams-meetings?view=msteams-client-js-latest&tabs=dotnet#notificationsignal-api) In-Meeting Experience.
  
### User interactions(Meeting Organizer)
- **Add New Agenda Item** - Gives provision to add new Agenda point.
- **Add** - Adds the agenda from Textinput to the SidePanel agenda list.
- **Publish Agenda** - Sends the agenda list to the meeting chat.

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version
    ```
- [socket.io](https://www.npmjs.com/package/socket.io) to update data in real time.
      
 - [Ngrok](https://ngrok.com/download) (Only for devbox testing) Latest (any other tunneling      software       can also be used)
    ```bash

     # run ngrok locally
    ngrok http -host-header=localhost 3978
    ```
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

1. Clone the repository
    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- In a terminal, navigate to `samples/javascript_nodejs/SidePanelnode`

    ```bash
    cd samples/javascript_nodejs/SidePanelnode
    ```

- Install modules

    ```bash
    npm install
    ```

- Start the bot

    ```bash
    npm start
    ```
2. If you are using Visual Studio code
- Launch Visual Studio code
- Folder -> Open -> Project/Solution
- Navigate to ```samples\SidePanelnode\``` folder
- Select ```SidePanelnode``` Folder
3. Run ngrok - point to port 3978
   ```ngrok http -host-header=rewrite 3978```
4. Create a new Bot by following steps mentioned in [Build a bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots?view=msteams-client-js-latest#build--a-bot-for-teams-with-the-microsoft-bot-framework) documentation.
5. Go to .env file  and add ```MicrosoftAppId``` and  ```MicrosoftAppPassword``` information.
6. Run your app, either from Visual Studio code  with ``` npm start``` or using ``` Run``` in the Terminal.
7. Update the manifest.json file with ```Microsoft-App-ID``` and ```BaseUrl``` value.
8. [Install the App in Teams Meeting](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings?view=msteams-client-js-latest#meeting-lifecycle-scenarios)

## Interacting with the app in Teams Meeting
Interact with SidePanel by clicking on the App icon present on the top menu beside the "more actions" during a meeting.
1. Once the app is clicked, sidepanel appears with the default agenda list. Only organizer gets the feasibility to add new agenda points to the list using "Add New Agenda Item" button.
2. On click of "Add" button, agenda point will be added to the agenda list.
3. On click of "Publish Agenda", the agenda list will be sent to the meeting chat.
