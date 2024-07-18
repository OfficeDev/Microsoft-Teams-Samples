---
page_type: sample
description: This is a sample API application that demonstrates how to perform RAG-based semantic search using Azure CosmosDB NoSQL.
products:
- azure-cosmos-db
- azure-openai
- azure-function
- azure-blob-storage
languages:
- nodejs
extensions:
 contentType: samples
 createdDate: "07/15/2024 12:00:00 PM"
urlFragment: officedev-microsoft-teams-samples-api-doc-search-nodejs

---

# RAG-based Semantic Search API with Azure CosmosDB NoSQL

This sample demonstrate the concept of Retrieval Augmented Generation (RAG). 
 
To do this, we upload documents into an Azure blob storage. The contents of these documents are converted into vector embeddings that are stored in an Azure NoSQL Cosmos DB. Using an API endpoint, we can run prompt queries on the contents of the documents. 
 
Relevant results are returned by performing RAG on the document contents vector embeddings and shown along with a calculated similarity score.
 
The API endpoint can be called in two ways - using the browser directly, or a GET call using any API testing tool. 

## Included Features
* **Blob-based Event Subscription:** Enables event-driven actions based on changes or updates to Azure Blob Storage.
* **Azure Open AI Embeddings:** Utilizes OpenAI embeddings for enhanced understanding and representation of textual content.
* **Vector Search with NoSQL Cosmos DB:** Performs efficient vector searches using the VectorDistance() function within Azure Cosmos DB, a scalable NoSQL database.

## Interaction with the application

![RAG Based CosmosDB Semantic Search Gif](Images/rag-based-cosmos-db.gif)

## Prerequisites


- [NodeJS](https://nodejs.org/en/)
- [Azure Open AI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview)
- [Azure CosmosDB](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search)
- [Azure Function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript)


- [Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction)
- [Azure App Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/nodejs)
- API testing tools such as Postman

## Setup the application locally

### Setup Azure Function
- [Setup Azure Function and other resources](../azure-function-nodejs/README.md)

### Create and configure Azure Cosmos DB for NoSQL

 - **[Create Azure Cosmos DB Account](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/quickstart-portal#create-account)** in Azure portal and [Enroll in the Vector Search Preview Feature](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search#enroll-in-the-vector-search-preview-feature)
 - Create and collect `CosmosDBEndpoint`, `CosmosDBKey`, `CosmosDBDatabaseId`, `CosmosDBContainerId` and save those values to update in `.env` file later.

### Create an Azure Open AI service
- In Azure portal, create a [Azure Open AI service](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).
- Create and collect `AzureOpenAIEndpoint`, `AzureOpenAIApiKey`, `AzureOpenAIDeploymentName`, and save those value  to update in `.env` file later.
   
### Setup for code

  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
  - Navigate to `samples/api-doc-search/nodejs` folder and open the project in Visual Studio Code.
  - Open `.env` file and update the `.env` configuration for the application to use the `AzureOpenAIEndpoint`, `AzureOpenAIApiKey`, `AzureOpenAIDeploymentName`, `CosmosDBEndpoint`, `CosmosDBKey`, `CosmosDBDatabaseId`, `CosmosDBContainerId`, `SimilarityScore`, `APPINSIGHTS_INSTRUMENTATIONKEY`, `APPINSIGHTS_CONNECTIONSTRING` values.
  - In a terminal, navigate to `samples/api-doc-search/nodejs`

 - Install node modules and run application by pressing F5 in Visual Studio Code
 
   ```bash
    npm install
   ```

## Running the sample

In this step, we will run the sample by uploading files on which a prompt query needs to be executed. Once the file is uploaded, vector embeddings for it are created, and using an API endpoint, a prompt query can be fired which returns relevant responses using RAG, along with a similarity score.

- **Upload file to Azure Blob container:** Upload the file(s) for which you want to create vector embeddings into the Blob Storage container.

  ![Blob Container](../azure-function-nodejs/Images/1.blob-container.png)


- **Azure Function Invocation:** Uploading the files to blob will automatically trigger the Azure function which will start creating the required vector embeddings and store in the Azure NoSQL Cosmos DB.

  ![Azure Function Invocation](../azure-function-nodejs/Images/3.azure-function-invocation.png)

  ![Cosmos DB Embeddings](../azure-function-nodejs/Images/4.cosmos-db-embeddings.png)

- **You can call the API Endpoint to run the required prompts on the documents in two ways:**
**1) Directly from a web browser Once you run the sample locally and press F5, it will open the Application Homepage:**

  ![API Home page](Images/1.app-home-page.png)

- `http://localhost:3000/search?query=what is Teams AI Library?`
  ![Search query and result - 4](Images/5.search-result-web.png)


2) **Using an API testing tool, like Postman:**
- `http://localhost:3000/search?query=what is Teams AI Library`
  ![Search query and result - 1](Images/2.search-result-postman-1.png)


- **Open the Postman and search the query to get results:**

- `http://localhost:3000/search?query=what is Prompt Tuning?`
  ![Search query and result - 2](Images/3.search-result-postman-2.png)


## Deploy the sample in Azure environment (Optional)

[Deploy the code](https://learn.microsoft.com/en-us/azure/app-service/quickstart-nodejs?tabs=windows&pivots=development-environment-vscode#configure-the-app-service-app-and-deploy-code)

## Further reading

- [Azure CosmosDB](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search)

- [Vector Search Preview Feature](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/vector-search#enroll-in-the-vector-search-preview-feature)

- [Azure Open AI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview)

- [Azure Function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-javascript)

- [Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction)

- [Azure App Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/nodejs)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/api-doc-search" />