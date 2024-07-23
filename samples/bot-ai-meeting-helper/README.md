---
page_type: sample
description: This sample showcases how to utilize Azure Open AI Search within Teams Toolkit for Visual Studio Code to extract Action Items and Meeting summaries from meeting transcriptions.
products:
- office-teams
- copilot-m365
languages:
- javascript
---

# Meeting Helper with Azure Open AI

This sample demonstrates generating action items and a meeting summary based on the attendees and transcription, then sending them to all participants.

- To achieve this process, the user will schedule a meeting, which can be either a one-time or recurring event. Attendees interested in receiving the meeting summary and action items must individually subscribe to the meeting. For recurring meetings, multiple instances will be displayed, allowing users to subscribe to each instance separately.
 
- After the meeting concludes, a webhook endpoint will be triggered in the background to extract the meeting transcription.

- Using Azure OpenAI, the extracted meeting transcription will be processed to extract action items and a meeting summary. Finally, this information will be sent to all meeting attendees who have subscribed to the meeting.

## Included Features
* **Graph API:** Utilize graph APIs to retrieve meeting transcriptions and other meeting or user-related details.
* **Azure Open AI:** Leverages Azure OpenAI for extracting action items and meeting summaries.
* **Azure Table Storage:** Utilizes Azure Table Storage to store subscribed user information, including conversation IDs for sending meeting details.

## Interaction with app

 ![bot-ai-meeting-helperGif](Images/bot-ai-meeting-helperGif.gif)

## Prerequisites

- [Node.js 18.x](https://nodejs.org/download/release/v18.18.2/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Table Storage](https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-portal)
- [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
- [Azure OpenAI](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line&pivots=programming-language-studio)
- [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-macos) if on macOS (`brew install --cask powershell`)

### Create an Azure Open AI service
- In Azure portal, create a [Azure Open AI service](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).
- Create and collect `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_DEPLOYMENT_NAME`, and save those value  to update in `.env` file later.

### Create an Azure Table Storage
- In Azure portal, create a [Azure Table Storage](https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-portal).
- Create and collect `Account_Name`, `Account_Key`, `Table_Name`, and save those value  to update in `.env` file later.

## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
1. Navigate to the `samples/bot-ai-meeting-helper` folder and open with Visual Studio Code.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

> When the application is running for the first time, the Teams toolkit will generate an app registration along with a password and other necessary credentials which is specified in "teamsapp.local.yml".

## Setup and use the sample locally 
1) Navigate to the `samples/bot-ai-meeting-helper/.localConfigs1` and rename to ".localConfigs" and update the values below.

   ```txt
        BOT_ID="BOT_ID"
        BOT_PASSWORD="SECRET_BOT_PASSWORD"
        AZURE_OPENAI_API_KEY="SECRET_AZURE_OPENAI_API_KEY"
        AZURE_OPENAI_ENDPOINT="AZURE_OPENAI_ENDPOINT"
        AZURE_OPENAI_DEPLOYMENT_NAME="AZURE_OPENAI_DEPLOYMENT_NAME" 
        BOT_ENDPOINT="BOT_ENDPOINT"
        Base64EncodedCertificate="Base_64_Encoded_Certificate"
        EncryptionCertificateId="Encryption_Certificate_Id"
        PRIVATE_KEY_PATH="Pem_File_Path"
        Account_Name="Azure_Storage_Account"
        Account_Key="Azure_Storage_Account_Key"
        Table_Name="Azure_Storage_Table"
        partitionKey="Azure_Storage_Table_PartitionKey"
        AI_Model="Azure_Open_AI_Model"
        SubscriptionURL="https://graph.microsoft.com/v1.0/subscriptions"
        SystemPrompt="Generate a filtered list of action items from meeting transcriptions by user in bullet point user wise categorized with proper format like:  <b> Attendee:</b> 
        Action Items in bullet points"
        LocalTimeZone=Asia/Kolkata
        APPINSIGHTS_INSTRUMENTATIONKEY=""
        APPINSIGHTS_CONNECTIONSTRING=""
    ``` 
1) Create a policy for a demo tenant user for creating the online meeting on behalf of that user using the following PowerShell script (Images for reference only)
 
      PowerShell script

    ```powershell
    # Import-Module MicrosoftTeams
    # Call Connect-MicrosoftTeams using no parameters to open a window allowing for MFA accounts to authenticate
    Connect-MicrosoftTeams
    New-CsApplicationAccessPolicy -Identity “<<policy-identity/policy-name>>” -AppIds "<<microsoft-app-id>>" -Description "<<policy-description>>"
    Grant-CsApplicationAccessPolicy -PolicyName “<<policy-identity/policy-name>>” -Identity "<<object-id-of-the-user-to-whom-the-policy-needs-to-be-granted>>"
    
    OR
    # For global access
    # Grant-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Global
    ```
    Example:

    ```powershell
      # Import-Module MicrosoftTeams
      Connect-MicrosoftTeams

      New-CsApplicationAccessPolicy -Identity Meeting-policy-dev -AppIds "d0bdaa0f-8be2-4e85-9e0d-2e446676b88c" -Description "Online meeting policy - contoso town"
      
      Grant-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Identity "782f076f-f6f9-4bff-9673-ea1997283e9c"
      OR
      # For global access
      # Grant-CsApplicationAccessPolicy -PolicyName Meeting-policy-dev -Global
    ```
    
     ![PolicySetup](Images/Policy.png)
     
1) In Azure [App Registration](https://ms.portal.azure.com/) Under left menu, navigate to **API Permissions**, and make sure to add the following permissions of Microsoft Graph API > Application permissions:

    - `Calendars.Read`
    - `Calendars.ReadBasic.All`
    - `Calendars.ReadWrite`
    - `EventListener.Read.All`
    - `EventListener.ReadWrite.All`
    - `OnlineMeetingArtifact.Read.All`
    - `OnlineMeetingRecording.Read.All`
    - `OnlineMeetings.Read.All`
    - `OnlineMeetings.ReadWrite.All`
    - `OnlineMeetingTranscript.Read.All`
    - `User.Read.All`
    - `User.ReadBasic.All`
    - `User.ReadWrite.All`

        ![Application Permission](Images/ApplicationPermission.png)

1) Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams Meeting Helper sample app:** [Manifest](/samples/bot-ai-meeting-helper/demo-manifest/bot-ai-meeting-helper.zip)

- To run the demo, you must set up and grant the necessary policy. Please follow the second step mentioned above for instructions on how to do this. 

## Running the sample

1) **Select Upload an app and choose the app package if you are running demo-manifest then choose downloaded demo-manifest zip file. If the sample is running locally then root folder, appPackage >> build >> appPackage.local.zip**
![UploadCustomeApp](Images/0.UploadCustomeApp.png)

1) **To fetch all upcoming meetings when a user requests "What is my upcoming meetings," the application will display all future scheduled one-time meetings and recurring meetings, showing individual instances of each recurring meeting.**
![MeetingHelperSearchApp](Images/2.MeetingHelperFindUpcomingMeeting.png)

1) **When the user clicks on a specific meeting, the application will call the Graph API to create a subscription for that particular instance, store the subscription details in the database, and then send an acknowledgment.**
![MeetingHelperSelectApp](Images/3.MeetingHelperSubscription.png)

1) **Once the meeting has started, the user needs to join the meeting.**
![MeetingHelperSearchFileName](Images/4.MeetingHelperJoinMeeting.png)

1) **After joining the meeting, the user must navigate to the "More options" menu at the top and select "Record and transcribe," then choose "Start transcription" to begin recording the transcription.**
![MeetingHelperSelectResults](Images/5.MeetingHelperStartTranscription.png)

1) **Once the meeting is concluded, the transcription will be generated in the background. Additionally, the application will internally call the Graph API to obtain the transcription. Using Azure OpenAI, it will extract all meeting action items and generate a meeting summary.**
![6.MeetingHelperResults](Images/6.MeetingHelperEndMeeting.png)

1) **All subscribed users will automatically receive the meeting summary and action items.**
![7.MeetingHelperYesResults](Images/7.MeetingHelperResults.png)

## Further reading

### AI, Personal Chat And Table Storage

- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/overview)
- [Table Storage](https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-portal)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-ai-meeting-helper" />