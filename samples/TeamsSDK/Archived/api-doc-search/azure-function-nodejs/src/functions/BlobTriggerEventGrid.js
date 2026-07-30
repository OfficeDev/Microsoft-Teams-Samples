// Importing necessary modules
// 'app' is imported from '@azure/functions' to create and manage Azure functions
const { app } = require('@azure/functions');

// 'InitiateEmbeddings' is  imported from 'embedFunctions.js' which contains the logic to generate embeddings
const { InitiateEmbeddings } = require('./embedFunctions');
require('dotenv').config();

const appInsights = require('applicationinsights');

// Configure Application Insights with your instrumentation key or connection string
const instrumentationKey = process.env.APPINSIGHTS_INSTRUMENTATIONKEY;

// Or use connection string
const connectionString = process.env.APPINSIGHTS_CONNECTIONSTRING;

appInsights.setup(connectionString || instrumentationKey)
    .setAutoCollectRequests(true)
    .setAutoCollectPerformance(true, true)
    .setAutoCollectExceptions(true)
    .setAutoCollectDependencies(true)
    .setAutoCollectConsole(true, true)
    .setUseDiskRetryCaching(true)
    .start();

const client = appInsights.defaultClient;

// Define an Azure Function triggered by Blob storage events using Event Grid
app.storageBlob('BlobTriggerEventGrid', {
    path: 'samples-workitems/{name}', // The path to the Blob storage container, with a placeholder for the blob name
    source: 'EventGrid', // The source of the events is Event Grid
    connection: 'c0008c_STORAGE', // The name of the connection string to the Azure Storage account

    // Handler function that processes the Blob and generates embeddings
    handler: async (blob, context) => {
        const blobName = context.triggerMetadata.name;
        const blobSize = blob.length;

        // Logging the name and size of the processed blob
        context.log(`Storage blob (using Event Grid) function processed blob "${blobName}" with size ${blobSize} bytes`);
        console.log(`Storage blob (using Event Grid) function processed blob "${blobName}" with size ${blobSize} bytes`);
        client.trackEvent({ name: "BlobProcessed", properties: { blobName, blobSize } });

        try {
            context.log(`Embeddings creation started: blob "${blobName}" with size ${blobSize} bytes`);
            console.log(`Embeddings creation started: blob "${blobName}" with size ${blobSize} bytes`);
            client.trackEvent({ name: "EmbeddingsCreationStarted", properties: { blobName, blobSize } });

            // Initiating the embedding process using the blob's URI
            await InitiateEmbeddings(context.triggerMetadata.uri, context);

            context.log(`Embeddings creation Succeeded: blob "${blobName}" with size ${blobSize} bytes`);
            console.log(`Embeddings creation Succeeded: blob "${blobName}" with size ${blobSize} bytes`);
            client.trackEvent({ name: "EmbeddingsCreationSucceeded", properties: { blobName, blobSize } });
        } catch (error) {
            // Logging any errors encountered during the embedding creation process
            console.error(`ERROR: Embeddings are not generated for this blob "${blobName}" with size ${blobSize} bytes. Error:`, error);
            client.trackException({ exception: error, properties: { blobName, blobSize } });
        }
    }
});