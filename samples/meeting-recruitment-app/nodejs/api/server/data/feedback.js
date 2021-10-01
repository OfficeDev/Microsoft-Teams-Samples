const tableStore = require('azure-storage');
const authStore = require('../keys');
const authObject = new authStore();

// unique identifiers for our PartitionKey
const { v4: uuidv4 } = require('uuid')

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

// The table name in table storage.
const tableName = "Feedback";

// Ensuring feedback table is created if not already exists.
tableClient.createTableIfNotExists(tableName, (error, result) => {
     if (error) {
         console.log(`Error Occured in table creation ${error.message}`);
     } else {
         console.log(`${result.TableName} ${result.created}`);
     }
 });

// Method to save feedback details.
function saveFeedback(feedbackObj, callback) {
    if (feedbackObj != null) {
        feedbackObj.PartitionKey = feedbackObj.MeetingId;
        feedbackObj.RowKey = uuidv4();

        // Use { echoContent: true } if you want to return the created item including the Timestamp & etag
        tableClient.insertEntity(tableName, feedbackObj, function (error, result, response) {
            if (!error) {
                return callback(response);
            } else {
                // In case of an error we return an appropriate status code and the error returned by the DB
                return callback({ error: error });
            }
        });
    }
}

module.exports = {
    saveFeedback
}
