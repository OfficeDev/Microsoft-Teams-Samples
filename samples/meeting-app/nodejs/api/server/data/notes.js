const tableStore = require('azure-storage');
const authStore = require('../keys');
const authObject = new authStore();

// unique identifiers for our PartitionKey
const { v4: uuidv4 } = require('uuid')

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

const tableName = "Notes";

 tableClient.createTableIfNotExists(tableName, (error, result) => {
     if (error) {
         console.log(`Error Occured in table creation ${error.message}`);
     } else {
         console.log(`${result.TableName} ${result.created}`);
     }
 });

function getNotes(email, callback) {
    var query = new tableStore.TableQuery()
        .where('PartitionKey eq ?', email);

    tableClient.queryEntities(tableName, query, null, (error, result, resp) => {
        if (!error) {
            return callback(resp.body.value);
        }
    });
}

function addNote(notesObj, callback) {
    if (notesObj != null) {
        notesObj.PartitionKey = notesObj.CandidateEmail;
        notesObj.RowKey = uuidv4();

        // Use { echoContent: true } if you want to return the created item including the Timestamp & etag
        tableClient.insertEntity(tableName, notesObj, function (error, result, response) {
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
    getNotes,
    addNote
}
