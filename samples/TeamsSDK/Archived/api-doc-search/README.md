
## This sample has two solutions
First, setup the CosmosDB NoSQL and Azure Function solution (`azure-function-nodejs`) to create and store embedding vectors in CosmosDB. Next, setup and run the API application (`nodejs`) to perform RAG-based semantic search on the Azure CosmosDB NoSQL with embedding vectors-based search for user prompts.

## azure-function-nodejs
This Azure function will be triggered to create and store embedding vectors in CosmosDB once a file is uploaded to Azure Blob Storage or local storage emulation.

The function uses `@langchain/textsplitters` directly for text chunking. It does not install LangChain's optional Office parser and archive-extraction dependencies; PDF and Word extraction continue to use `pdf-parse` and `word-extractor`.

## nodejs
This sample API application demonstrates how to perform RAG-based semantic search on the Azure CosmosDB NoSQL with embedding vectors-based search for user prompts.