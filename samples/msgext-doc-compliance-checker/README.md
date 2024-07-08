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


## Prerequisites

- [Node.js 18.x](https://nodejs.org/download/release/v18.18.2/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal)
- [Teams Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)

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
1) Navigate to the `samples/msgext-doc-compliance-checker` folder and open with Visual Studio Code.

1) Navigate to the `samples/msgext-doc-compliance-checker/.localConfigs` directory and update the values below.

   ```txt
      END_POINT="https://<your-service>.azurewebsites.net"
      API_KEY="your-api-key"
      DEPLOYMENT_ID="your-deployment-id"
      AZURE_STORAGE_CONNECTION_STRING="The connection string for your Azure Storage account."
      CONTAINER_NAME="your-container"
      CHECKLIST_NAME="your-checklist-name"
   ```
