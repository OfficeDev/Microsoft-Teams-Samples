---
page_type: sample
description: Microsoft Teams meeting extensibility sample for iteracting with Side Panel in-meeting
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:27"
---

# Meetings SidePanel

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

- Register a bot with Azure Bot Service, following the instructions [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0).
- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
- While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.
    > NOTE: When you create your bot you will create an App ID and App password - make sure you keep these for later.

1. Clone the repository
      ```bash
      git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
      ```

    - In a terminal, navigate to `samples/meetings-sidepanel/nodejs`

        ```bash
        cd samples/meetings-sidepanel/nodejs
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
    - Navigate to ```samples/meetings-sidepanel/nodejs``` folder
    - Select ```meeting-sidepanel``` Folder
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

![](https://user-images.githubusercontent.com/50989436/118759535-d7c7e280-b88e-11eb-955b-8843d1a4a814.png)

2. On click of "Add" button, agenda point will be added to the agenda list by organizer.![](https://user-images.githubusercontent.com/50989436/118760002-ad2a5980-b88f-11eb-821d-3a1f74d9fa71.png)![](https://user-images.githubusercontent.com/50989436/118759709-28d7d680-b88f-11eb-9aa7-a6b67daa639c.png)

3. On click of "Publish Agenda", the agenda list will be sent to the meeting chat.![](https://user-images.githubusercontent.com/50989436/118759762-3e4d0080-b88f-11eb-8880-b0ed3739cbe0.png)

