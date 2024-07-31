
## This sample has two solutions
First, setup the CosmosDB NoSQL and Azure Function solution (`azure-function-nodejs`) to create and store embedding vectors in CosmosDB. Next, setup and run the API application (`nodejs`) to perform RAG-based semantic search on the Azure CosmosDB NoSQL with embedding vector-based search for user prompts.

## azure-function-nodejs
This Azure function will be triggered to create and store embedding vectors in CosmosDB once a file is uploaded to Azure Blob Storage or local storage emulation.

## nodejs
This sample API application demonstrates how to perform RAG-based semantic search on the Azure CosmosDB NoSQL with embedding vector-based search for user prompts.