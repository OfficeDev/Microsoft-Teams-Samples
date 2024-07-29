// <copyright file="database.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

const { TableClient, AzureNamedKeyCredential } = require("@azure/data-tables");
const { ConsoleTranscriptLogger } = require("botbuilder");

const path = require('path');

const ENV_FILE = path.join(__dirname, '.env');

require('dotenv').config({ path: ENV_FILE });

const config = require('./config');

 
// Replace with your actual storage account name and account key
const accountName = config.Account_Name;
const accountKey = config.Account_Key;
const tableName = config.Table_Name;
 
// Initialize the TableClient
const credential = new AzureNamedKeyCredential(accountName, accountKey);
const tableClient = new TableClient(
    `https://${accountName}.table.core.windows.net`,
    tableName,
    credential
);
 
// Function to store data in Azure Table Storage
async function storeData(partitionKey, rowKey, data) {
    try
    {
        const entity = {
            partitionKey: partitionKey,
            rowKey: rowKey,
            ...data
        };
        await tableClient.createEntity(entity);
        console.log(`Entity with PartitionKey: ${partitionKey}, RowKey: ${rowKey} has been created.`);
    }
    catch(ex)
    {
        console.log(ex);
    }
}
 
// Function to retrieve data from Azure Table Storage
 async function getData(partitionKey, rowKey, subscriptionId) {
    try {
        
        // Perform a query based on attributes
        const queryOptions = { filter: `subscriptionId eq '${subscriptionId}'` };
        const entitiesIterator = tableClient.listEntities(queryOptions);
        
        const userActionItems = [];
        const seenKeys = new Set();
            
        // Iterate over the results
        for await (const entity of entitiesIterator) {
            const key = `${entity.onlineMeetingId}_${entity.conversationId}_${entity.userId}`; // Adjust as per your unique identifier
            
            if (entity.subscriptionId == subscriptionId) {
                seenKeys.add(key);
                console.log(entity.givenName);
                userActionItems.push(entity); 
            }
        }

        // console.log(`Entity retrieved: `, entity);
         return userActionItems;
    } catch (error) {
        console.error(`Error retrieving entity: `, error.message);
    }
}

// Export the function
module.exports = {
    getData: getData,
    storeData: storeData 
  };