# Chat With Your Data

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://github.com/codespaces/new?hide_repo_select=true&ref=mbindra-ChatWithYourData&repo=Microsoft-Teams-Samples/samples/msteams-chat-with-your-data&devcontainer_path=.devcontainer%2Fdevcontainer.json&resume=1)

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

## Setting up the app in Github Codespaces
1. Click Open in GitHub Codespaces badge above to create a codespace for the sample app. Wait for the codespace to be setup, it may take a couple of minutes.
2. Using the Teams Toolkit extension, sign in to your Microsoft 365 account and Azure account under ```ACCOUNTS```.
3. [Set up your data source using Azure AI resources](#setting-up-your-data-source).
4. [Populate the environment files](#populating-the-env-files).
5. Press **Ctrl+Shift+D** to open the ```Run and Debug``` menu. Select ```Debug``` and press ```F5``` or click on the play button.
6. Download the zip file ```appPackage/build/appPackage.local.zip``` and [sideload the app to Teams](#sideloading-the-app-to-teams).

## Setting up the app locally
1. Clone the repository
   ```git clone https://github.com/t-mbindra/chat-with-your-data.git```
2. Install [Python 3.11](https://www.python.org/downloads/), [Node.js](https://nodejs.org/) and [Rust](https://www.rust-lang.org/tools/install).
4. Install  [Poetry](https://python-poetry.org/docs/#installation) and [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli).
5. Open the project folder(Microsoft-Teams-Samples/samples/msteams-chat-with-your-data) in [Visual Studio Code](https://code.visualstudio.com/download).
6. Install the [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python](https://marketplace.visualstudio.com/items?itemName=ms-python.python) extensions.
8. Run
   ```poetry install```
9. Run
   ```poetry build```
10. Using the Teams Toolkit extension, sign in to your Microsoft 365 account and Azure account under ```ACCOUNTS```.
11. [Set up your data source using Azure AI resources](#setting-up-your-data-source).
12. [Populate the environment files](#populating-the-env-files).
13. Press **Ctrl+Shift+D** to open the ```Run and Debug``` menu. Press ```F5``` or click on the play button.

## Deploying the app on Azure
Instead of the ```Debug``` or ```F5``` flow, you can deploy the app on Azure:
1. Using the Teams Toolkit Extension tab, click on ```Provision``` followed by ```DEPLOY``` under ```LIFECYCLE```. You will be asked to select the subscription and resource group for provisioning.
2. Download the zip file ```appPackage/build/appPackage.dev.zip``` and [sideload the app to Teams](#sideloading-the-app-to-teams).

## Setting up your data source
1. Run```sh deploy.sh``` in the terminal. You will be prompted to login to Azure and select a subscription.
2. Go to the [Azure AI Studio](https://oai.azure.com/portal), select relevant subscription and the resource ```teamsazureopenai-cognitive```. Proceed to the ```Chat Playground```. 
3. Click on ```Add a data source``` under the ```Add your data``` tab. Upload your data with ```Upload files``` or ```URL/ web address```.
4. Select```teamsazureopenai``` as your storage resource and ```teamsazureopanai-search``` as your search resource. Type the index-name you want to use. Ensure you have the correct subscription selected.
5. Add your data and select search type and chunk size. Select ```API Key``` as the authentication type. Save and Close and wait for the data to be ingested.

## Populating the environment files
1. You need to populate the environment variables in ```env/.env.local.user``` if you are using the ```Debug``` or ```F5``` flow. Else, populate the environment variables in ```env/.env.dev.user``` if you are dpleoying the app on Azure.
2. Go to the the [Azure portal](https://ms.portal.azure.com/) and navigate to the resource group ```ChatWithYourData```. 
3. Go to the ```teamsazureopenai-cognitive``` resource. Select the ```Keys and Endpoints``` tab under ```Resource Management```. Populate the ```SECRET_AZURE_OPENAI_KEY, SECRET_AZURE_OPENAI_ENDPOINT``` using ```Key 1``` and ```Endpoint```.   
4. Go to the ```teamsazureopenai-search``` resource. Populate ```SECRET_AZURE_SEARCH_ENDPOINT``` from the ```Url``` given. Select the ```Keys``` tab under ```Settings```.  Populate ```SECRET_AZURE_SEARCH_KEY``` using the ```Primary admin key```.
5. Populate ```AZURE_SEARCH_INDEX``` based on the index-name you chose while setting up the data source.

## Sideloading the app to Teams
1. Go to your Teams app and click on the ```Apps``` icon. Select ```Manage your apps``` followed by ```Upload an app```.
2. Select ```Upload a custom app``` and open the relevant zip file. Click on ```Add``` when prompted.

>[!Note]
> Check the status of all your local bots on [Microsoft Bot Framework](https://dev.botframework.com/bots).
> Check the status of all your Teams apps on [Teams Developer Portal](https://dev.teams.microsoft.com/apps).
> Check the status of all Azure resources on [Azure Portal](https://portal.azure.com/#home) by navigating to the relevant resource group.

> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.
