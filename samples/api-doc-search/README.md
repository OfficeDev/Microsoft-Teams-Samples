
## This sample is having two solutions
Setp below two solutions in your local machine and run the applications.
**Note: Setup the CosmosDB and Azure Function firstly to create and store embedding vectors in CosmosDB and then run the API application to perform RAG-based semantic search using Azure CosmosDB NoSQL with embedding vector-based search.**

## azure-function-nodejs
This Azure function will be triggered to create and store embedding vectors in CosmosDB once a file is uploaded to Azure Blob Storage or local storage emulation.

## nodejs
This sample API application demonstrates how to perform RAG-based semantic search using Azure CosmosDB NoSQL with embedding vector-based search for user queries.