var express = require('express');
var router = express.Router();

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

/* Delete all embeddings from Cosmos DB. */
router.get('/', async function (req, res, next) {
  try {
    const { resources: items } = await container.items.readAll().fetchAll();

    for (const item of items) {
      await container.item(item.id).delete();
      console.log(`Deleted item with id: ${item.id}`);
    }

    console.log('All items deleted successfully.');
    appInsightsClient.trackEvent({ name: "All items deleted successfully.", properties: { items } });
    res.status(200).send('All items deleted successfully.');
  } catch (error) {
    res.status(500).send('Error occurred while delting all items from CosmosDB.');
    console.error('Error occurred while delting all items from CosmosDB.');
    appInsightsClient.trackException({ exception: error, properties: { error } });
  }
});

module.exports = router;
