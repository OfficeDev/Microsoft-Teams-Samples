---
page_type: sample
description: This sample demonstrates how to integrate Azure AI Search in a Teams message extension to compare a contract proposal against a guidance policy checklist to see if the proposal complies with the guidance. It uses Teams Toolkit for Visual Studio Code and JavaScript, and the message extension can be used as a plugin in Microsoft Copilot for Microsoft 365.
products:
- office-teams
- copilot-m365
languages:
- javascript
---

# Compliance Checker with Azure AI Search sample

## Interaction with app

 ![msgext-doc-compliance-checkerGif](Images/msgext-doc-compliance-checkerGif.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams Msgext Doc Compliance Checker sample app:** [Manifest](/samples/msgext-ai-doc-compliance-checker/demo-manifest/msgext-ai-doc-compliance-checker.zip)

## Prerequisites

- [Node.js 18.x](https://nodejs.org/download/release/v18.18.2/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal)
- [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
- [Open AI](https://platform.openai.com/docs/quickstart/build-your-application) or [Azure OpenAI]([https://azure.microsoft.com/free/](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line&pivots=programming-language-studio))

## Run the app (Using Teams Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Teams Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup and use the sample
1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
1) Navigate to the `samples/msgext-ai-doc-compliance-checker` folder and open with Visual Studio Code.

1) Navigate to the `samples/msgext-ai-doc-compliance-checker/.localConfigs` directory and update the values below.

   ```txt
      END_POINT="https://<your-service>.azurewebsites.net"
      API_KEY="your-api-key"
      DEPLOYMENT_ID="your-deployment-id"
      AZURE_STORAGE_CONNECTION_STRING="The connection string for your Azure Storage account."
      CONTAINER_NAME="your-container"
      CHECKLIST_NAME="your-checklist-name"
   ```

## Running the sample

**Select Upload an app**
![UploadCustomeApp](Images/0.UploadCustomeApp.png)

**Select Add**
![ComplianceCheckerInstallApp](Images/1.ComplianceCheckerInstallApp.png)

**Search for your app in Messaging Extensions**
![ComplianceCheckerSearchApp](Images/2.ComplianceCheckerSearchApp.png)

**Select App**
![ComplianceCheckerSelectApp](Images/3.ComplianceCheckerSelectApp.png)

**Search File Name**
![ComplianceCheckerSearchFileName](Images/4.ComplianceCheckerSearchFileName.png)

**File Name Results**
![ComplianceCheckerSelectResults](Images/5.ComplianceCheckerSelectResults.png)

**Compliance Checker Results**
![6.ComplianceCheckerResults](Images/6.ComplianceCheckerResults.png)

**Compliance Checker Yes Results**
![7.ComplianceCheckerYesResults](Images/7.ComplianceCheckerYesResults.png)

**Compliance Checker No Results**
![8.ComplianceCheckerNoResults](Images/8.ComplianceCheckerNoResults.png)

### Test the app in Copilot for Microsoft 365
Navigate to the Microsoft Copilot for Microsoft 365 chat. Check the lower left of the chat user interface, below the compose box. You should see a plugin icon. Click this and enable the msgext-ai-doc-compliance-checker plugin.

## Further reading

### AI, Message Extensions And Blob Storage

- [Message extensions for Microsoft Copilot for Microsoft 365](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/overview-message-extension-bot)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/overview)
- [Quickstart: Azure Blob Storage](https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-nodejs?tabs=managed-identity%2Croles-azure-portal%2Csign-in-azure-cli)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/msgext-ai-doc-compliance-checker" />