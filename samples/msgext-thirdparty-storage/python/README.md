---
page_type: sample
description: This app enables seamless integration with third-party cloud storage providers for files dragged and dropped in Teams chats or channels. It uses the Microsoft Teams JavaScript SDK's thirdPartyCloudStorage module to fetch and upload files efficiently.
products:
- office-teams
- office
- office-365
languages:
- python
extensions:
 contentType: samples
 createdDate: "2025-01-17 17:00:25"
urlFragment: officedev-microsoft-teams-samples-msgext-thirdparty-storage-python
---

# Third-Party Storage Integration in Microsoft Teams
Microsoft Teams allows users to integrate third-party cloud storage providers by modifying the default storage options from OneDrive and SharePoint. This capability enables files that are dragged and dropped into the Teams chat or message compose area to be stored in a third-party storage service. Using the Microsoft Teams JavaScript client library (TeamsJS), third-party apps can capture these files through the getDragAndDropFiles API and then upload them to their own cloud storage.

To enable this feature, the latest version of the TeamsJS SDK is required, along with a properly configured app manifest and the app ID of the third-party storage provider. When a file is dragged and dropped, it is temporarily cached in Teams before being fetched by the third-party app and uploaded to its storage. This seamless integration enhances flexibility for businesses using external storage solutions within the Teams environment.

## Included Features
* Message Extensions

**Interaction with bot**
![msgext-thirdparty-storage](Images/msgext-thirdparty-storage.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [Python SDK](https://www.python.org/downloads/) version 3.7
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
- [Python SDK](https://www.python.org/downloads/) min version 3.11

## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. Remember to select `requirements.txt` as dependencies to install when creating the virtual environment.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

1) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

1) Create [Bot Framework registration resource](https://docs.microsoft.com/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running the tunnelling application. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Bot Framework registration](https://docs.microsoft.com/microsoftteams/platform/bots/how-to/create-a-bot-for-teams#register-your-web-service-with-the-bot-framework)

1) Bring up a terminal, navigate to `Microsoft-Teams-Samples\samples\msgext-thirdparty-storage\python` folder

1) Activate your desired virtual environment

1) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

1) Update the `config.py` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

1) Set "BASE_URL" in the `config.py` as per your application like the ngrok forwarding url (ie `https://xxxx.ngrok-free.app`) after starting ngrok and if you are using dev tunnels, your URL will be like: https://12345.devtunnels.ms.

1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `{{AAD_APP_CLIENT_ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appManifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the bot to personal/team/groupChat scope (Supported scopes)

1) Run your bot with `python app.py`

## Running the sample

**Upload an Application to Teams**
![1.UploadApp](Images/1.UploadApp.png)

**Select the Build for Your Organization**
![2.BuildOrgApp](Images/2.BuildOrgApp.png)

**Log in to Admin Center and Copy App ID**
![3.AdminCenter](Images/3.AdminCenter.png)

**Admin Settings for File Drag-and-Drop to Third-Party Storage** 
- [Admin settings for file drag-drop to third-party storage](https://review.learn.microsoft.com/en-us/microsoftteams/admin-settings-for-file-drag-drop-to-third-party-storage?branch=main&branchFallbackFrom=pr-en-us-11741)

![4.AdminSettings](Images/4.AdminSettings.png)

**Install the Application in Teams**
![5.InstallApp](Images/5.InstallApp.png)

**Select a Channel for File Upload**
![6.SelectChannelChat](Images/6.SelectChannelChat.png)

**Open the Channels to Access Files**
![7.OpenChannel](Images/7.OpenChannel.png)

**Drag and Drop All Files into Teams**
![8.drag_drop_all_file](Images/8.drag_drop_all_file.png)

**Files Successfully Dragged and Dropped**
![9.drag_drop_files](Images/9.drag_drop_files.png)

**All Files Loaded in Task Module**
![10.drag_drop_files_task_module](Images/10.drag_drop_files_task_module.png)

**Files Loaded into the Compose Area**
![11.Upload_ComposeFile](Images/11.Upload_ComposeFile.png)

**Send All Dragged and Dropped Files**
![12.SendComposeFile](Images/12.SendComposeFile.png)

**Drag and Drop Files in Chat**
![13.drag_drop_all_file_chat](Images/13.drag_drop_all_file_chat.png)

**Drop Files in the Chat Window**
![14.drag_drop_file_Chat](Images/14.drag_drop_file_Chat.png)

**Load All Files in the Task Module**
![15.drag_drop_file_load_Task_module](Images/15.drag_drop_file_load_Task_module.png)

**Upload All Files in the Chat**
![16.UploadChat](Images/16.UploadChat.png)

**Send Files in the Chat**
![17.SendChat](Images/17.SendChat.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Third-party storage capability](https://learn.microsoft.com/en-us/microsoftteams/platform/concepts/device-capabilities/third-party-storage-capability?branch=pr-en-us-11741)
- [Turn off Teams Native File Upload policy](https://learn.microsoft.com/en-us/microsoftteams/turn-off-teams-native-file-upload-policy)
- [Admin settings for file drag-drop to third-party storage](https://learn.microsoft.com/en-us/microsoftteams/admin-settings-for-file-drag-drop-to-third-party-storage?branch=main&branchFallbackFrom=pr-en-us-11741)
- [App manifest](https://learn.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema?branch=main)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/msgext-thirdparty-storage-python" />