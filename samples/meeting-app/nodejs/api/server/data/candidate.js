const tableStore = require('azure-storage');
const express = require('express');
const bodyParser = require('body-parser');
const authStore = require('./keys');
const authObject = new authStore();
const instance = express();

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

const tableName = "CandidateDetails";

tableClient.createTableIfNotExists(tableName, (error, result) => {
    if (error) {
        console.log(`Error Occured in table creation ${error.message}`);
    } else {
        console.log(`${result.TableName} ${result.created}`);
    }
});
