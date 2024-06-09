// Import necessary libraries and modules
const express = require('express'); // Import the Express framework              
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai"); // Import Azure OpenAI SDK
const { RecursiveCharacterTextSplitter } = require('langchain/text_splitter'); // Import the RecursiveCharacterTextSplitter
const { BlobServiceClient } = require('@azure/storage-blob');
const officeParser = require('officeparser');
const fs = require('fs');
const path = require('path');
const { v4: uuidv4 } = require('uuid');
const { CosmosClient } = require("@azure/cosmos");
require('dotenv').config();

// Azure Storage connection string and container name.
const azureStorageConnString = process.env.AzureStorageDBConnString;
const azureBlobContainerName = process.env.AzureBlobContainerName;

// Azure Open AI service endpoint, API key, and deployment name.
const azureAiApiEndpoint = process.env.AzureOpenAIEndpoint;
const openAIApiKey = process.env.AzureOpenAIApiKey;
const openAIDeploymentName = process.env.AzureOpenAIDeploymentName; // Ensure this is correct

// Create an instance of the Azure OpenAI client
const client = new OpenAIClient(azureAiApiEndpoint, new AzureKeyCredential(openAIApiKey));

// Cosmos DB configuration.
const cosmosEndpoint = process.env.CosmosDBEndpoint;
const cosmosKey = process.env.CosmosDBKey; 

const cosmosClientOptions = {}; // Optional client options
const cosmosClient = new CosmosClient({ endpoint: cosmosEndpoint, key: cosmosKey }, cosmosClientOptions);

// Cosmos DB database and container IDs.
const databaseId = process.env.CosmosDBDatabaseId;
const containerId = process.env.CosmosDBContainerId;

// Get a reference to the Cosmos DB container.
const container = cosmosClient.database(databaseId).container(containerId);

const app = express(); // Create an Express application

// Middleware to parse incoming JSON data
app.use(express.json());

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});

app.get('/search', (req, res) => {
  res.send(req.query);
});

// Function to split text using RecursiveCharacterTextSplitter
async function splitText(fileContentsAsString) {
  const splitter = new RecursiveCharacterTextSplitter({
    chunkSize: 500,
    chunkOverlap: 5,
    separators: ['\n\n', '\n', ' ', '']
  });

  return await splitter.createDocuments([fileContentsAsString]);
}

async function listBlobs() {
  // Create the BlobServiceClient object
  const blobServiceClient = BlobServiceClient.fromConnectionString(azureStorageConnString);

  // Get the container client
  const containerClient = blobServiceClient.getContainerClient(azureBlobContainerName);

  // List all blobs in the container and get their URLs
  const blobUrls = [];
  for await (const blob of containerClient.listBlobsFlat()) {
    // Construct the blob URL
    const blobUrl = `${containerClient.url}/${blob.name}`;
    blobUrls.push(blobUrl);

    // console.log(blobUrl);
  }

  //// Remove duplicates using a Set
  //const uniqueBlobUrls = new Set(blobUrls);

  //// Convert back to an array if needed
  //const uniqueBlobUrlsList = Array.from(uniqueBlobUrls);

  return blobUrls;
}

app.get('/getblobs', async (req, res) => {
  listBlobs().then((blobUrls) => {
    // console.log("Blob URLs:", blobUrls);

    // Usage example:
    const blobUrl = blobUrls[0];
    const blobName = path.basename(blobUrl);
    const downloadFilePath = path.join(__dirname, blobName);

    // Create the BlobServiceClient object
    const blobServiceClient = BlobServiceClient.fromConnectionString(azureStorageConnString);

    // Get the container client
    const containerClient = blobServiceClient.getContainerClient(azureBlobContainerName);

    downloadBlobToLocal(containerClient, blobName, downloadFilePath);

    if (fs.existsSync(downloadFilePath)) {
      // Example usage of parseOfficeFile function
      const result = parseOfficeFile(downloadFilePath, blobUrl, blobName);
      res.send(result);
    } else {
      console.error(`File ${downloadFilePath} does not exist!`);
      res.status(404).send(`File ${downloadFilePath} does not exist!`);
    }
  }).catch((error) => {
    console.error("Error listing blobs:", error);
    res.status(500).send("An error occurred while processing your request.");
  });

  // return listBlobs().then((blobUrls) => {
  //   res.send(blobUrls);
  // });
});

async function downloadBlobToLocal(containerClient, blobName, downloadFilePath) {
  const blockBlobClient = containerClient.getBlockBlobClient(blobName);
  await blockBlobClient.downloadToFile(downloadFilePath);
  // console.log(`Downloaded blob content to ${downloadFilePath}`);
}

// Function to parse the office file
async function parseOfficeFile(filePath, blobUrl, fileName) {
  try {
    // Parse the office file asynchronously
    const data = await officeParser.parseOfficeAsync(filePath);

    // Split the parsed text
    const result = await splitText(data);
    const records = [];

    // Log the result and get embeddings for each item
    for (const item of result) {
      console.log(item.pageContent);
      console.log("====================================");

      // Get embedding for the current item
      try {
        const embedding = await getEmbeddingAsync(item.pageContent);

        // Create the DocumentEmbeddingDetail object
        const documentEmbedding = {
          id: uuidv4(), // Ensure the item has a unique 'id'
          partitionKey: "teamid", // Adjust based on your partition key strategy
          contents: item.pageContent,
          fileName: fileName,
          url: blobUrl,
          vectors: Array.from(embedding), // Assuming embedding is already an array
          // similarityScore: 0.0
        };

        records.push({
          SimilarityScore: 0.0,
          FileName: fileName,
          Url: blobUrl,
          Contents: item.pageContent
        });

        // await container.createDocuments(documentEmbedding);
         // Insert the document into the Cosmos DB container
         const { resource: createdItem } = await container.items.create(documentEmbedding);
         console.log("Document created successfully:", createdItem);

        console.log("Embedding:", embedding);
        return records;
      } catch (error) {
        console.error("Error getting embedding:", error);
      }
    }

    return records;
  } catch (err) {
    // Handle parsing error
    console.error("Error parsing office file:", err);
  }
}

// Function to get embedding asynchronously
async function getEmbeddingAsync(content) {
  const embeddingsOptions = {
    deploymentName: openAIDeploymentName,
    input: [content],
  };

  try {
    // Call the API to get embeddings
    const response = await client.getEmbeddings(openAIDeploymentName, [content]);
    // The response includes the generated embedding
    const item = response.data[0];
    const embedding = item.embedding;

    return embedding;
  } catch (error) {
    console.error("Error getting embeddings:", error);
    throw error;
  }
}