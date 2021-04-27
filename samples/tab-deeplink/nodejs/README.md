# DeepLink

Bot Framework v4 DeepLink sample

This sample displays how to consume SubEntity Id to DeepLink from Bot to Tab and Tab to Tab.

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

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- In a terminal, navigate to `samples/javascript_nodejs/DeepLinkTabnode`

    ```bash
    cd samples/javascript_nodejs/DeepLinkTabnode
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
- Navigate to ```samples\DeepLinkBotnode\``` folder
- Select ```DeepLinkBotnode``` Folder
3. To run the application required  node modules.Please use this command to install modules npm i
4. Run ngrok - point to port 3978
   ```ngrok http -host-header=rewrite 3978```
5. Create a new Bot by following steps mentioned in [Build a bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots?view=msteams-client-js-latest#build--a-bot-for-teams-with-the-microsoft-bot-framework) documentation.
6. Go to .env file  and add ```MicrosoftAppId``` and  ```MicrosoftAppPassword``` information.
7. Run your app, either from Visual Studio code  with ``` npm start``` or using ``` Run``` in the Terminal.
8. Update the manifest.json file with ```Microsoft-App-ID```,```ContentUrl```, ```WebsiteUrl``` and ```EntityID``` value.
9. Install the app in Teams.


## Interacting with the bot

Enter text in the emulator.  The text will be echoed back by the bot.
1. Interact with DeepLink bot by pinging it. 
2. Select the option from the options displayed in the adaptive card. This will redirect to the respective Task in the Tab.
3. Click on Back to List to view all the options. User can select an option which will redirect to the respective Task in the Tab.
