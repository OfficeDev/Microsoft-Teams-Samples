const tableStore = require('azure-storage');

// unique identifiers for our PartitionKey
const { v4: uuidv4 } = require('uuid')
const authStore = require('../keys');
const authObject = new authStore();

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

const tableName = "Questions";

tableClient.createTableIfNotExists(tableName, (error, result) => {
    if (error) {
        console.log(`Error Occured in table creation ${error.message}`);
    } else {
        console.log(`${result.TableName} ${result.created}`);
    }
});

function getQuestions(meetingId, callback) {
    var query = new tableStore.TableQuery()
        .where('PartitionKey eq ?', meetingId);

    tableClient.queryEntities(tableName, query, null, (error, result, resp) => {
        if (!error) {
            return callback(resp.body.value);
        }
    });
}

function saveQuestions(questionsObj, callback) {
    questionsObj.forEach(function (obj) {
        var entity = {}
        if (obj != null) {
            entity.PartitionKey = obj.meetingId;
            entity.RowKey = uuidv4();
            entity.MeetingId = obj.meetingId;
            entity.IsDelete = 0
            entity.Question = obj.question;
            entity.SetBy = obj.setBy;

            // Use { echoContent: true } if you want to return the created item including the Timestamp & etag
            tableClient.insertEntity(tableName, entity, { echoContent: true }, function (error, result, response) {
                if (!error) {
                    // This returns a 201 code + the database response inside the body
                    // Calling status like this will automatically trigger a context.done()
                } else {
                    // In case of an error we return an appropriate status code and the error returned by the DB
                    return callback({ error: error });
                }
            });
        }
    })
    return callback(true);
}

function editQuestion(questionObj, callback) {
    if (questionObj != null) {
        questionObj.PartitionKey = questionObj.MeetingId;
        questionObj.RowKey = questionObj.QuestionId;
        questionObj.ETag = "*";

        // Use { echoContent: true } if you want to return the created item including the Timestamp & etag
        tableClient.replaceEntity(tableName, questionObj, function (error, result, response) {
            if (!error) {
                return callback(response);
            } else {
                // In case of an error we return an appropriate status code and the error returned by the DB
                return callback({ error: error });
            }
        });
    }
}

function deleteQuestion(questionObj, callback) {
    if (questionObj != null) {
        questionObj.PartitionKey = questionObj.MeetingId;
        questionObj.RowKey = questionObj.QuestionId;
        questionObj.ETag = "*";

        // Use { echoContent: true } if you want to return the created item including the Timestamp & etag
        tableClient.deleteEntity(tableName, questionObj, function (error, result, response) {
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
    getQuestions,
    saveQuestions,
    editQuestion,
    deleteQuestion
}
