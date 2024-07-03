// Import necessary libraries and modules

const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { RecursiveCharacterTextSplitter } = require('langchain/text_splitter');
const { BlobServiceClient } = require('@azure/storage-blob');
const pdfParse = require('pdf-parse');
const path = require('path');
const { v4: uuidv4 } = require('uuid');
const { CosmosClient } = require("@azure/cosmos");
require('dotenv').config();
const WordExtractor = require("word-extractor");
const appInsights = require('applicationinsights');

// Configure Application Insights with your instrumentation key or connection string
const instrumentationKey = process.env.APPINSIGHTS_INSTRUMENTATIONKEY;

// Connection string
const connectionString = process.env.APPINSIGHTS_CONNECTIONSTRING;

appInsights.setup(connectionString || instrumentationKey)
  .setAutoCollectRequests(true)
  .setAutoCollectPerformance(true, true)
  .setAutoCollectExceptions(true)
  .setAutoCollectDependencies(true)
  .setAutoCollectConsole(true, true)
  .setUseDiskRetryCaching(true)
  .start();

const appInsightsClient = appInsights.defaultClient;

// Azure Storage connection string and container name.
const azureStorageConnString = process.env.AzureStorageDBConnString;
const azureBlobContainerName = process.env.AzureBlobContainerName;

// Azure Open AI service endpoint, API key, and deployment name.
const azureAiApiEndpoint = process.env.AzureOpenAIEndpoint;
const openAIApiKey = process.env.AzureOpenAIApiKey;
const openAIDeploymentName = process.env.AzureOpenAIDeploymentName; // Ensure this is correct

// Create an instance of the Azure OpenAI client
const azureOpenAIClient = new OpenAIClient(azureAiApiEndpoint, new AzureKeyCredential(openAIApiKey));

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

/**
 * Function to split the text content of a file into smaller chunks.
 *
 * @param {string} fileContentsAsString - The content of the file as a single string.
 * @returns {Array} - An array of objects, each containing a chunk of the original text.
 * @throws Will throw an error if text splitting fails.
 */
async function splitText(fileContentsAsString) {
  // Create an instance of the RecursiveCharacterTextSplitter with specified options
  const splitter = new RecursiveCharacterTextSplitter({
    chunkSize: 800,
    chunkOverlap: 10,
    separators: ['\n\n', '\n', ' ', '']
  });

  // Split the file content into smaller chunks and return the result
  return await splitter.createDocuments([fileContentsAsString]);
}

/**
 * Function to list all blobs in a container and retrieve their URLs.
 *
 * @returns {Array} - An array containing URLs of all blobs in the container.
 * @throws Will throw an error if listing blobs fails.
 */
async function InitiateEmbeddings(blobUrl, context) {
  try {
    const blobName = path.basename(blobUrl);

    const extension = path.extname(blobName).toLowerCase();
    if (extension === '.txt') {
      const fileContents = await readBlobContents(blobUrl);
      const fileChunks = await createFileChunks(fileContents, context);

      context.log(`FileName ${blobName}, Chunks ${fileChunks.length}`);
      console.log(`FileName ${blobName}, Chunks ${fileChunks.length}`);
      console.log(`=================================================`);

      await createEmbeddings(fileChunks, blobUrl, blobName, context);
    }
    else if (extension === '.pdf') {
      const blobServiceClient = BlobServiceClient.fromConnectionString(azureStorageConnString);
      const containerClient = blobServiceClient.getContainerClient(azureBlobContainerName);

      const blobName = getBlobNameFromUrl(blobUrl);
      const blockBlobClient = containerClient.getBlockBlobClient(blobName);

      // Download blob content as a buffer
      const downloadBlockBlobResponse = await blockBlobClient.download();
      const blobContentBuffer = await streamToBuffer(downloadBlockBlobResponse.readableStreamBody);

      fileContent = await readPdfContent(blobContentBuffer);
      await parseOfficeFile(blobUrl, blobName, context, fileContent);
    } else if (extension === '.docx') {
      const blobServiceClient = BlobServiceClient.fromConnectionString(azureStorageConnString);
      const containerClient = blobServiceClient.getContainerClient(azureBlobContainerName);

      const blobName = getBlobNameFromUrl(blobUrl);
      const blockBlobClient = containerClient.getBlockBlobClient(blobName);

      // Download blob content as a buffer
      const downloadBlockBlobResponse = await blockBlobClient.download();
      const blobContentBuffer = await streamToBuffer(downloadBlockBlobResponse.readableStreamBody);

      const extractor = new WordExtractor();
      const extracted = extractor.extract(blobContentBuffer);

      extracted.then(async function (doc) {
        console.log(doc.getBody());
        await parseOfficeFile(blobUrl, blobName, context, doc.getBody());
      });
    }
    else {
      context.log(`${blobName} is not one of the allowed file types.`);
      console.log(`${blobName} is not one of the allowed file types.`);
      appInsightsClient.trackEvent({ name: "BlobNotAllowed", properties: { blobName } });
    }
  } catch (error) {
    context.log(`Failed to process blob ${blobUrl}:`, error);
    console.error(`Failed to process blob ${blobUrl}:`, error);
    appInsightsClient.trackException({ exception: error, properties: { blobUrl } });
  }
}

/**
 * Function to parse an office file, split the text, and store the parsed data along with embeddings in a database.
 *
 * @param {string} blobUrl - The URL of the blob storage where the file is stored.
 * @param {string} fileName - The name of the file.
 * @returns {Array} - An array of records containing similarity score, file name, URL, and contents.
 * @throws Will throw an error if parsing or embedding retrieval fails.
 */
async function parseOfficeFile(blobUrl, fileName, context, data) {
  try {
    // Split the parsed text
    const result = await splitText(data);
    const records = [];

    console.log(`FileName ${fileName}, Chunks ${result.length}`);
    console.log(`=================================================`);

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
        };

        records.push({
          // SimilarityScore: 0.0,
          FileName: fileName,
          Url: blobUrl,
          Contents: item.pageContent
        });

        // Insert the document into the Cosmos DB container
        await container.items.create(documentEmbedding);
      } catch (error) {
        context.log("Error getting embedding:", error);
        console.error("Error getting embedding:", error);
        appInsightsClient.trackException({ exception: error, properties: { records } });
      }
    }

    return records;
  } catch (err) {
    // Handle parsing error
    context.log("Error parsing office file:", err);
    console.error("Error parsing office file:", err);
    appInsightsClient.trackException({ exception: err, properties: { err } });
  }
}

/**
 * Function to get the embedding vector for a given content.
 * It calls an API to generate the embedding for the provided content.
 *
 * @param {string} content - The content for which to generate an embedding.
 * @throws Will throw an error if the API call fails.
 */
async function getEmbeddingAsync(content) {

  try {
    // Call the API to get embeddings
    const response = await azureOpenAIClient.getEmbeddings(openAIDeploymentName, [content]);

    // The response includes the generated embedding
    const item = response.data[0];
    const embedding = item.embedding;

    return embedding;
  } catch (error) {
    context.log("Error getting embeddings:", error);
    console.error("Error getting embeddings:", error);
    throw error;
  }
}

// Create small chunks of contents.
async function createFileChunks(fileContentsAsString, context) {
  try {
    const splitter = new RecursiveCharacterTextSplitter({
      chunkSize: 800,
      chunkOverlap: 10,
      separators: ['\n\n', '\n', ' ', '']
    });

    const fileChunks = await splitter.createDocuments([fileContentsAsString]);
    context.log('File chunks:', fileChunks);
    console.log('File chunks:', fileChunks);
    console.log(`=================================================`);
    return fileChunks;
  } catch (error) {
    context.log('Error processing file contents:', error);
    console.error('Error processing file contents:', error);
    throw error;
  }
}

// Create vector embeddings for each chunk of the file.
async function createEmbeddings(fileChunks, blobUrl, fileName, context) {
  try {
    const records = [];

    for (const chunk of fileChunks) {
      context.log(chunk);
      console.log(chunk);
      console.log("====================================");

      // Get embedding for the current item
      try {
        const embedding = await getEmbeddingAsync(chunk.pageContent);

        // Create the DocumentEmbeddingDetail object
        const documentEmbedding = {
          id: uuidv4(), // Ensure the item has a unique 'id'
          partitionKey: "teamid", // Adjust based on your partition key strategy
          contents: chunk.pageContent,
          fileName: fileName,
          url: blobUrl,
          vectors: Array.from(embedding), // Assuming embedding is already an array
        };

        records.push({
          // SimilarityScore: 0.0,
          FileName: fileName,
          Url: blobUrl,
          Contents: chunk.pageContent
        });

        // Insert the document into the Cosmos DB container
        await container.items.create(documentEmbedding);
      } catch (error) {
        context.log("Error getting embedding:", error);
        console.error("Error getting embedding:", error);
      }
    }

    return records;
  } catch (err) {
    context.log("Error parsing office file:", err);
    console.error("Error parsing office file:", err);
  }
}

// Helper function to read blob contents.
async function readBlobContents(blobUrl) {
  try {
    const blobServiceClient = BlobServiceClient.fromConnectionString(azureStorageConnString);
    const containerClient = blobServiceClient.getContainerClient(azureBlobContainerName);

    const blobName = getBlobNameFromUrl(blobUrl);
    const blockBlobClient = containerClient.getBlockBlobClient(blobName);

    // Download blob content as a buffer
    const downloadBlockBlobResponse = await blockBlobClient.download();
    const blobContentBuffer = await streamToBuffer(downloadBlockBlobResponse.readableStreamBody);

    // Convert buffer to string (assuming utf8 encoding)
    const blobContentString = blobContentBuffer.toString('utf8');

    return blobContentString;
  } catch (error) {
    console.error("Error reading blob contents:", error);
    throw error;
  }
}

// Helper function to convert readable stream to buffer.
async function streamToBuffer(readableStream) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    readableStream.on('data', (data) => {
      chunks.push(data instanceof Buffer ? data : Buffer.from(data));
    });
    readableStream.on('end', () => {
      resolve(Buffer.concat(chunks));
    });
    readableStream.on('error', reject);
  });
}

// Helper function to get blob name from URL.
function getBlobNameFromUrl(blobUrl) {
  return path.basename(blobUrl);
}

// Helper function to read PDF content.
async function readPdfContent(buffer) {
  try {
    const data = await pdfParse(buffer);
    return data.text;
  } catch (error) {
    console.error("Error reading PDF content:", error);
    throw error;
  }
}

module.exports = {
  InitiateEmbeddings,
};