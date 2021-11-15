# sequential-workflow-adaptive-card
This sample illustrates sequential workflow, user specific views and upto date adaptive cards bot.

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

## Interacting with the app in Teams

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

