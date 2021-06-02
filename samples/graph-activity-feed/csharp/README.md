# Notify Feed App

Bot Framework v4 Activity Feed sample using Tab.

This sample has been created using [Microsoft Graph](https://docs.microsoft.com/en-us/graph/overview?view=graph-rest-beta), it shows how trigger a Activity feed notification from your Tab, it triggers the feed notification for User, Chat and Team scope and send back to conversation.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## To try this sample

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

- Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `samples/graph-activity-feed/csharp` folder

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/graph-activity-feed/csharp` folder
  - Select `TabActivityFeed.csproj` file
  - Press `F5` to run the project

## Permissions Required to run this sample

  - Following Graph API permissions are required for your Azure App.
  - `TeamsActivity.Send`, `ChannelMessage.Send`, `ChatMessage.Send`, `Chat.ReadWrite`, `User.Read`,  `TeamsAppInstallation.ReadForUser.All`.
  - You can navigate to API Permissions section for your App and Choose Microsoft Graph => Application/Delegated permissions.

## Setting up activity types in manifest
Teams Activity feed notification API uses activity to which user want a notification which we specify in app manifest, Add following activity type in your manifest for task creation.

  ```"activities": {
    "activityTypes": [
      {
        "type": "taskCreated",
        "description": "Task created activity",
        "templateText": "New created task {taskName} for you"
      }
    ]
}
```
## User Interaction with Tab Activity Feed App

- Install TabActivityFeed manifest in Teams
- Add Tab in Personal, GroupChat or Team scope
- Fill the Details in Page and click on Send notification button
![image](https://user-images.githubusercontent.com/50989436/109036739-eb303b00-76ef-11eb-91d2-806c2b180396.png)

- Notification triggred by Tab App will appear in Teams Activity Feed

![image](https://user-images.githubusercontent.com/50989436/109036793-fb481a80-76ef-11eb-9fc5-eafc763a67d8.png)

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Send Notification to User in Chat](https://docs.microsoft.com/en-us/graph/api/chat-sendactivitynotification?view=graph-rest-beta)
- [Send Notification to User in Team](https://docs.microsoft.com/en-us/graph/api/team-sendactivitynotification?view=graph-rest-beta&tabs=http)
- [Send Notification to User](https://docs.microsoft.com/en-us/graph/api/userteamwork-sendactivitynotification?view=graph-rest-beta&tabs=http)