// <copyright file="search.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// server.js is used to setup and configure your API server.
// </copyright>

var express = require('express');
var router = express.Router();

const { OpenAIClient, AzureKeyCredential } = require("@azure/openai"); // Import Azure OpenAI SDK
const { CosmosClient } = require("@azure/cosmos");
require('dotenv').config();
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

/* GET search listing. */
router.get('/', async function(req, res, next) {
  try {
    const query = req.query.query;
    appInsightsClient.trackEvent({ name: "SearchStarted", properties: { query } });
    // Perform semantic search using the query parameter from the request
    const result = await semanticSearchDocumentsAsync(query);

    appInsightsClient.trackEvent({ name: "SearchCompleted", properties: { query } });
    // Send the search result as the response
    res.send(result);
  } catch (error) {
    // Log and send an error response if the search fails
    console.error("Error during semantic search:", error);
    res.status(500).send("Error during semantic search");
    appInsightsClient.trackException({ exception: error, properties: { error } });
  }
});

/**
 * Function to get the embedding vector for a given content.
 * It calls an API to generate the embedding for the provided content.
 *
 * @param {string} content - The content for which to generate an embedding.
 * @returns {Array} - The generated embedding vector.
 * @throws Will throw an error if the API call fails.
 */
async function getEmbeddingAsync(content) {

  try {
    // Call the API to get embeddings
    const response = await client.getEmbeddings(openAIDeploymentName, [content]);

    // The response includes the generated embedding
    const item = response.data[0];
    const embedding = item.embedding;

    return embedding;
  } catch (error) {
    console.error("Error getting embeddings:", error);
    appInsightsClient.trackException({ exception: error, properties: { error } });

    throw error;
  }
}

/**
 * Function to perform a semantic search on documents.
 * It retrieves the top 5 most similar documents based on the provided query.
 *
 * @param {string} query - The search query for which to find similar documents.
 * @returns {Array} - A sorted array of the top 5 most similar documents.
 * @throws Will throw an error if the search fails.
 */
async function semanticSearchDocumentsAsync(query) {
  try {
    appInsightsClient.trackEvent({ name: "EmbeddingStarted", properties: { query } });

    // Get embedding vector for the search query.
    const embedding = await getEmbeddingAsync(query);
    appInsightsClient.trackEvent({ name: "EmbeddingStarted", properties: { query } });

    const similarityScore = Number.parseFloat(process.env.SimilarityScore);

    appInsightsClient.trackEvent({ name: "SimilarityScore Parsed Successfully", properties: { similarityScore } });

    // The SQL query to find the top 5 most similar documents
    const queryText = `
      SELECT TOP 5 c.contents, c.fileName, c.url,
      VectorDistance(c.vectors, @vectors, false) as similarityScore
      FROM c
      WHERE VectorDistance(c.vectors, @vectors, false) > @similarityScore`;

    const querySpec = {
      query: queryText,
      parameters: [
        { name: "@vectors", value: embedding },
        { name: "@similarityScore", value: similarityScore }
      ]
    };

    // Fetch the top 5 results directly
    const { resources } = await container.items.query(querySpec).fetchAll();
    var result = resources.sort((a, b) => b.similarityScore - a.similarityScore);
    appInsightsClient.trackEvent({ name: "Query Result Generated Successfully", properties: { result } });

    return result;
  } catch (error) {
    console.error("Error during semantic search:", error);
    appInsightsClient.trackException({ exception: error, properties: { error } });

    throw error;
  }
}

module.exports = router;
