---
page_type: sample
description: Demonstrating on how to implement sequential flow, user specific view and upto date adaptive cards in bot.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:26"
---

# Sequential workflow adaptive cards

This sample illustrates sequential workflow, user specific views and upto date adaptive cards bot and the list of incident created can be seen in messaging extension and can share a specific incident to the chat/team.

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/bot-sequential-flow-adaptive-cards/csharp/` folder
  - Select `SequentialUserSpecificFlow.csproj` file

3) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```

4) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `Manifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`).
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

1) Update appsettings.json file with Microsoft App Id, App Secret.
2) Run your app, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

## Workflows

### Workflow for bot interaction

```mermaid
sequenceDiagram
    participant Teams User B    
    participant Teams User A
    participant Teams Client
    Teams User A->>+Teams Client: Enters create incident bot commands
    Sample App->>+Teams Client: loads card with option 
    Teams User A->>+Teams Client: Enters required details and assigns to user B
    Sample App-->>Teams Client: Posts the incidet card with auto-refresh for user A and user B
    Teams Client->>Teams User A: loads incident card with loading indicator 
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User A: Responds with Updated AC for the user A
    Teams User B->>Teams Client: User opens the chat
    Teams Client-->>Teams User B: Loads the incident base card
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User B: Responds with card for user B with option to approve/reject
```

### Workflow for messaging extension interaction


```mermaid
sequenceDiagram
    participant Teams User B    
    participant Teams User A
    participant Teams Client
    Teams User A->>+Teams Client: Clicks on Incidents ME action in a group chat
    opt App not installed flow
        Teams Client-->>Teams User A: App install dialog
        Teams User A->>Teams Client: Installs app
    end   
    Teams Client->>+Sample App: Launches Task Module
    Sample App-->>-Teams Client: Loads existing incidents created using Bot
    Teams User A->>Teams Client: Selects incident to share in chat
    Teams Client->>Sample App: Invoke action callback composeExtension/submitAction
    Sample App-->>Teams Client: Posts Base card with auto-refresh for user A and user B
    Teams Client->>Teams User A: loads incident card with loading indicator 
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User A: Responds with Updated AC for the user A
    Teams User B->>Teams Client: User opens the chat
    Teams Client-->>Teams User B: Loads the incident base card
    Teams Client->>Sample App: Automatically invokes refresh action
    Sample App-->>Teams User B: Responds with card for user B with option to approve/reject
```


## Interacting with the bot

1. In Teams, Once the app is successfully installed in a group chat, ping the bot by @mentioning it. Bot will reply with a card showing that the person has initiated the incident. 

  ![image](https://user-images.githubusercontent.com/80379013/123651438-19ae5600-d849-11eb-9024-3897bf833d39.png)
  
2. Using refresh activity only the person who has initiated will be able to proceed further by entering the details of the incident and assign it to a person from the group chat, while others in the group chat will still be able to see only the initiated card.

  ![image](https://user-images.githubusercontent.com/80379013/123651344-0307ff00-d849-11eb-98e5-029daa7dc49f.png)

3. User who has initiated the incident will be able to enter the details using the series of cards in a sequential flow and submit it for the further approval/rejection process.

  ![image](https://user-images.githubusercontent.com/80379013/123651705-5417f300-d849-11eb-89e2-b99564c30b43.png)
  ![image](https://user-images.githubusercontent.com/80379013/123651751-5f6b1e80-d849-11eb-889c-e08bcdd37540.png)
  ![image](https://user-images.githubusercontent.com/80379013/123652355-da343980-d849-11eb-817b-87ad8f4598f8.png)
  
4. Once the details are submitted and assigned to a person from the group chat, it will send an updated card to the chat with all the entered details.

  ![image](https://user-images.githubusercontent.com/80379013/123652434-eb7d4600-d849-11eb-9e02-e9d3496f3e66.png)
  
5. Now, only the person assigned to will be able to either approve or reject it.

  ![image](https://user-images.githubusercontent.com/80379013/123652525-02239d00-d84a-11eb-8d43-f45607b1ef2f.png)  

6. After the approval/rejection of the card, the final updated card will be sent to the group chat.

  ![image](https://user-images.githubusercontent.com/80379013/123652838-4616a200-d84a-11eb-96c4-580979287b63.png)


## Interaction from messaging extension.


1. On selecting app from messaging extension,it checks whether bot is installed in chat/team. If not installed, user will get a option for justInTimeInstallation card.

   ![just in time installation card](SequentialUserSpecificFlow/Images/justInTimeInstallation.png)

2. After successful installation, list of all incident will be available in messaging extension.

   ![incident list card](SequentialUserSpecificFlow/Images/incidentListCard.png).
   
3. User can select any incident from the list and can share to that chat/team.

   ![share incident](SequentialUserSpecificFlow/Images/shareIncidentCard.png).   

