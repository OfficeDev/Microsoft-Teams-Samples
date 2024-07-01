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
- [Azure subscription](https://portal.azure.com)
- You will need to create [Azure AI Search](https://learn.microsoft.com/en-us/azure/search/search-create-service-portal), [Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal) and [Azure OpenAI](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal) resources on Azure portal.
- [Teams Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

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
      AZURE_OPENAI_SERVICE_NAME= the name of the Azure OpenAI resource 
      AZURE_OPENAI_DEPLOYMENT_NAME= the deployment name of the `text-embedding-ada-002` model
      AZURE_OPENAI_API_KEY= the key available under Keys and endpoints on Azure OpenAI resource
      AZURE_SEARCH_ENDPOINT= the endpoint url of Azure AI Search
      AZURE_SEARCH_ADMIN_KEY= the admin key available under Keys on Azure AI Search resource
      AZURE_SEARCH_INDEX_NAME= the index name created when uploading documents
   ```


### Step 3 - Test the app in Copilot for Microsoft 365
Navigate to the Microsoft Copilot for Microsoft 365 chat. Check the lower left of the chat user interface, below the compose box. You should see a plugin icon. Click this and enable the EcoGroceries Call Center plugin.
