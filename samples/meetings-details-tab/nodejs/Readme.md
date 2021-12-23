---
page_type: sample
description: Microsoft Teams meeting extensibility sample for iteracting with Details Tab in-meeting
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

# Meetings Details Tab

This sample app illustrates the implementation of Details Tab in Meeting. User can create a poll and post poll in meeting chat and participants can submit their feedback in Meeting.

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version
    ```

      
 - [Ngrok](https://ngrok.com/download) (Only for devbox testing) Latest (any other tunneling      software       can also be used)
    ```bash

     # run ngrok locally
    ngrok http -host-header=localhost 3978
    ```
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## To try this sample

1. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

    - In a terminal, navigate to `samples/meetings-details-tab/nodejs`

        ```bash
        cd samples/meetings-details-tab/nodejs
        ```

    - Install modules and Start the bot
    - Server will run on PORT:  `4001`

        ```bash
        npm run server
        ```

        > **This command is equivalent to:**
        _npm install > npm run build-client > npm start_

    - Start client application
    - Client will run on PORT:  `3978`

        ```bash
        npm run client
        ```
        
        > **This command is equivalent to:**
         _cd client > npm install > npm start_

Create a new Bot by following steps mentioned in [Build a bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots?view=msteams-client-js-latest#build--a-bot-for-teams-with-the-microsoft-bot-framework) documentation.

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

Go to .env file  and add ```BotId``` ,  ```BotPassword``` and ```BaseUrl as ngrok URL``` information.
Update the manifest.json file with ```Microsoft-App-ID```,```BotId```, ```BaseUrl as ngrok URL```
Install the app in Teams. 


## Interacting with the bot

Interact with Details Tab in Meeting.

1. Install the Details Tab manifest in meeting chat.
2. Add the Details Tab in Meeting
3. Click on Add Agenda
4. Newly added agenda will be added to Tab.
![Image](https://user-images.githubusercontent.com/50989436/120268903-5af02c00-c2c4-11eb-9061-c8af7436715e.png)
5. Click on Send button in Agenda from Tab.
6. An Adaptive Card will be posted in meeting chat for feedback.
![Image](https://user-images.githubusercontent.com/50989436/120431715-7c214d00-c396-11eb-8919-0dbb6192ce22.png)
7. Participants in meeting can submit their response in adaptive card
8. Response will be recorded and Bot will send an new adaptive card with response.
![Image](https://user-images.githubusercontent.com/50989436/120431763-92c7a400-c396-11eb-8daf-dce922b380ad.png)
9. Participants in meeting can view the results from meeting chat or Tab itself.

