
# Setup Azure Function to store Embedding Vectors in CosmosDB

## Create Azure Open AI service
**> Note: You can skip this step if you have already created the Azure Open AI service.**
- In Azure portal, create an [Azure Open AI servie](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).
- Create and collect `AzureOpenAIEndpoint`, `AzureOpenAIApiKey`, `AzureOpenAIDeploymentName` and save these values in Notepad to update in `.local.settings.json` file later.

- **Azure Open AI deployed model:** Deploy the model in Azure Open AI service for the application to use embedding vectors.
![Azure Opne AI deployed model](Images/5.azure-open-ai-deployed-model.png)

## Create and Configure Azure Cosmos DB for NoSQL
**> Note: You can skip this step if you have already created the Azure Cosmos DB account.**

 - **[Create Azure Cosmos DB Account](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/quickstart-portal#create-account)** in Azure portal and [Enroll in the Vector Search Preview Feature](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search#enroll-in-the-vector-search-preview-feature)
  
 - Create and collect `CosmosDBEndpoint`, `CosmosDBKey`, `CosmosDBDatabaseId`, `CosmosDBContainerId`, `PartitionKey` and save these values in Notepad to update in `.local.settings.json` file later.

## Create Azure Blob storage:
**> Note: You can skip this step if you have already created the Azure Blob storage.**

- In Azure portal, create a [Azure Blob storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction).
- Create and collect `AzureStorageDBConnString`, `AzureBlobContainerName` and save these values in Notepad to update in `.local.settings.json` file later.

## Create Azure Function

- [Create the function app](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#create-the-function-app).

## Create Azure App Insights
- [Azure App Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/nodejs)
- Create and collect `APPINSIGHTS_INSTRUMENTATIONKEY`, `APPINSIGHTS_CONNECTIONSTRING` and save these values in Notepad to update in `.local.settings.json` file later.


## Create the Event Subscription
**> Note: Please follow below documentation carefully to make your Azure function successfully up and running.**

- [Create the event subscription](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#create-the-event-subscription).

- [Trigger Azure Functions on blob containers using an event subscription](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#create-the-event-subscription).

## Code Setup

  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
  - Navigate to `samples/api-doc-search/azure-function-nodejs` folder and open the project in Visual Studio Code.
  - Open `.local.settings1.json` file and rename it as `.local.settings.json` and update the configuration for the application to use the `AzureOpenAIEndpoint`, `AzureOpenAIApiKey`, `AzureOpenAIDeploymentName`, `CosmosDBEndpoint`, `CosmosDBKey`, `CosmosDBDatabaseId`, `CosmosDBContainerId`, `PartitionKey`, `SimilarityScore`, `APPINSIGHTS_INSTRUMENTATIONKEY`, `APPINSIGHTS_CONNECTIONSTRING` values.
  
  - In a terminal, navigate to `samples/api-doc-search/azure-function-nodejs`

 - Install node modules and run application via pressing F5 in Visual Studio Code
 
   ```bash
    npm install
   ```

## Running the sample

[Prepare local storage emulation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#prepare-local-storage-emulation)

[Run the function locally](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#run-the-function-locally)

[Upload a file to the blob container and it will trigger Azure function automatically and Azure function with start creating and storing embedding vectors in CosmosDB](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#upload-a-file-to-the-container)

- **Blob Container:** Upload the file to blob container and it will trigger Azure function automatically.
![Blob Container](Images/1.blob-container.png)

- **Azure Function Invocation:** Azure function will invoke and start creating and storing embedding vectors in CosmosDB.
![Azure Function Invocation](Images/3.azure-function-invocation.png)
  
- **Cosmos DB Embeddings:** You can see the embedding vectors stored in CosmosDB.
![Cosmos DB Embeddings](Images/4.cosmos-db-embeddings.png)

## Deploy the sample in Azure environment

[Deploy your function code to azure function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#deploy-your-function-code)

## Further reading

- [Azure CosmosDB](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search)

- [Vector Search Preview Feature](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search#enroll-in-the-vector-search-preview-feature)

- [Azure Open AI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview)

- [Azure Function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript)

- [Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction)

- [Azure App Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/nodejs)

- [Trigger Azure Functions on blob containers using an event subscription](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript#deploy-your-function-code)