---
page_type: sample
description: "This sample illustrates a meeting experience for recruitment scenario using Apps In Meetings."
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
  contentType: samples
  createdDate: "10/1/2021 2:36:57 PM"
---

# Recruitment App Sample using Apps in Meetings

This sample illustrates a meeting experience for recruitment.

It has meeting details and in-meeting app that helps in the interview process.

![Details](MeetingApp/Images/details.png)

![Sidepanel Overview](MeetingApp/Images/sidepanel_overview.png)

![Sidepanel Questions](MeetingApp/Images/sidepanel_questions.png)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  ```bash
  # run ngrok locally
  ngrok http -host-header=localhost 3978
  ```

- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## To try this sample
1) Create a Bot Registration
   In Azure portal, create a [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2#create-the-resource).

2) Create a Azure Storage account(This is needed to store/retrieve data that is used in the app) 
  [Create storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal)

  This step will create a storage account. You will require storage account name and keys in following steps.
  Please follow [View account keys](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?tabs=azure-portal#view-account-access-keys) to see the keys info.

3) Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

4) In a terminal, navigate to `samples/meeting-recruitment-app/csharp`

    ```bash
    # change into project folder
    cd # MeetingApp
    ```
5) Run ngrok - point to port 3978

    ```bash
    # ngrok http -host-header=rewrite 3978
    ```
6) Modify the `manifest.json` in the `/AppPackage` folder and replace the following details
   - `<<App-ID>>` with some unique GUID   
   - `<<BASE-URL>>` with your application's base url, e.g. https://1234.ngrok.io
   - `<<VALID DOMAIN>>` with your app domain e.g. *.ngrok.io

7) Zip the contents of `AppPackage` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams.

8) Modify the `/appsettings.json` and fill in the `{{ MicrosoftAppId }}`,`{{ MicrosoftAppPassword }}` with the id from step 1.

9) Modify the `/appsettings.json` and fill in the `{{ StorageConnectionString }}` from step 2.

10) Run the app from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the app
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `MeetingApp` folder
  - Select `MeetingApp.csproj` file
  - Press `F5` to run the project

11) Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.
   - Select Add to meeting in the pop-up dialog box. Your app is uploaded to Teams.

## Features of this sample

1) Details page:
   The details page shows basic information of the candidate, timeline, Questions (that can be added for meeting), Notes (provided by peers)

   ![Details](MeetingApp/Images/details.png)

2) Action on Questions:
   The interviewer can Add/Edit or Delete question.

   ![Add Question](MeetingApp/Images/add_question.png)

   - Add Questions Task Module
   ![Add Question Task](MeetingApp/Images/add_task.png)

   ![Edit Delete Question](MeetingApp/Images/edit_questions.png)

   - Edit Question Task Module
   ![Edit Task](MeetingApp/Images/edit_task.png)

3) Add Notes:
   The interviewer can add notes that will appear to other peers.

   ![Add Notes](MeetingApp/Images/add_note.png)

   Add Note Task Module
   ![Add Notes](MeetingApp/Images/add_note_task.png)

4) Sidepanel:
    The in-meeting side panel shows two sections as follows:
    A) Overview: Shows the basic details of the candidate.
    B) Questions: The questions set in the details page appear here. The interviewer can use this to provide rating and submit final feedback.

    ![Sidepanel Overview](MeetingApp/Images/sidepanel_overview.png)

    ![Sidepanel Questions](MeetingApp/Images/sidepanel_questions.png)

5) Share assets:
   This is used to share assets to the candidate.
   ![Share Assets](MeetingApp/Images/share_assets.png)
  
## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.5.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
