const tableStore = require('azure-storage');
const authStore = require('../keys');
const authObject = new authStore();
const { ConversationDataRef } = require('../bot/botActivityHandler');

// unique identifiers for our PartitionKey
const { v4: uuidv4 } = require('uuid')

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

// The table name in table storage.
const tableName = "Notes";

// Ensuring notes table is created if not already exists.
tableClient.createTableIfNotExists(tableName, (error, result) => {
     if (error) {
         console.log(`Error Occured in table creation ${error.message}`);
     } else {
         console.log(`${result.TableName} ${result.created}`);
     }
 });

// Method to get notes from table storage.
function getNotes(email, callback) {
    var query = new tableStore.TableQuery()
        .where('PartitionKey eq ?', email);

    tableClient.queryEntities(tableName, query, null, (error, result, resp) => {
        if (!error) {
            return callback(resp.body.value);
        }
    });
}

// Method to add a new note.
function addNote(notesObj, callback) {
    if (notesObj != null) {
        notesObj.PartitionKey = notesObj.CandidateEmail;
        notesObj.RowKey = uuidv4();
        notesObj.AddedByName = ConversationDataRef != null && ConversationDataRef.members.length > 0 
        ? ConversationDataRef.members.find(entity => entity.email === notesObj.AddedBy).name
        : "Unknown";
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
