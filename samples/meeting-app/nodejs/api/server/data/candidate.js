const tableStore = require('azure-storage');
const express = require('express');
const authStore = require('../keys');
const authObject = new authStore();

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

const tableName = "CandidateDetails";

tableClient.createTableIfNotExists(tableName, (error, result) => {
    if (error) {
        console.log(`Error Occured in table creation ${error.message}`);
    } else {
        console.log(`${result.TableName} ${result.created}`);
    }
});

var query = new tableStore.TableQuery();

function getCandidateDetails(callback){
    tableClient.queryEntities(tableName, query, null, (error, result, resp) => {
        if (!error) {
            return callback(resp.body.value);
        }
    });
}

module.exports = {
    getCandidateDetails
}
