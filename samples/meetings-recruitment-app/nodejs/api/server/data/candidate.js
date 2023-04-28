const tableStore = require('azure-storage');
const { Console } = require('console');
const authStore = require('../keys');
const authObject = new authStore();
// unique identifiers for our PartitionKey
const { v4: uuidv4 } = require('uuid')

const tableClient = tableStore.createTableService(authObject.accountName, authObject.accessKey);

// The table name in table storage.
const tableName = "CandidateDetails";

// Candidates test data.
var candidateTestData = [
    {
        CandidateName: "Aaron Brooker",
        Role: "Software Engineer",
        Experience: "4 years 2 mos",
        Email: "aaron.b@gmail.com",
        Mobile: "+1 98765432",
        Skills: "React JS, .Net Core",
        Source: "Website",
        Attachments: "url1,url2",
        Education: "B Tech",
        LinkedInUrl: "https://in.linkedin.com/",
        TwitterUrl: "https://twitter.com/"
    },
    {
        CandidateName: "John Doe",
        Role: "Data Scientist",
        Experience: "5 years",
        Email: "john.d@gmail.com",
        Mobile: "+1 2456789",
        Skills: "Python, ML",
        Source: "Website",
        Attachments: "url1,url2",
        Education: "M Tech",
        LinkedInUrl: "https://in.linkedin.com/",
        TwitterUrl: "https://twitter.com/"
    }
]

// Checking if table exists, creating new table with test data if table doesn't exist.
tableClient.doesTableExist(tableName, (error, result) => {
    if (error) {
        console.log(`Error Occured while checking existence ${error.message}`);
    } else {
        if (result.exists) {
            console.log(tableName + " already exists");
        }
        else {
            // Ensuring candidate table is created if not already exists.
            tableClient.createTable(tableName, (error, result) => {
                if (error) {
                    console.log(`Error Occured in table creation ${error.message}`);
                } else {
                    console.log(`${result.TableName} ${result.created}`);
                    saveCandidateDetails(candidateTestData);
                }
            });
        }
    }
});

var query = new tableStore.TableQuery();

// Method to get candidate details.
function getCandidateDetails(callback) {
    tableClient.queryEntities(tableName, query, null, (error, result, resp) => {
        if (!error) {
            return callback(resp.body.value);
        }
    });
}

// Method to save Questions to be used in a meeting
function saveCandidateDetails(candidateDetails) {
    candidateDetails.forEach(function (obj) {
        var entity = {}
        if (obj != null) {
            obj.PartitionKey = obj.Email;
            obj.RowKey = uuidv4();
try {
    // Use { echoContent: true } if you want to return the created item including the Timestamp & etag
    tableClient.insertEntity(tableName, obj, { echoContent: true }, function (error, result, response) {
        if (!error) {
            // sending response once all entries are created
            console.log("Candidate details inserted for -" + obj.Email)
        } else {
            // In case of an error we return an appropriate status code and the error returned by the DB
            console.log("Candidate details insert error for -" + obj.Email)
        }
    });
    
} catch (error) {
    Console.log("errormsg:-" + error)
}
            
        }
    })
}

module.exports = {
    getCandidateDetails
}
