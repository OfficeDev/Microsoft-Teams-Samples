const express = require('express');
const path = require('path');
const auth = require('./auth');
const app = express();
const msal = require('@azure/msal-node');
const axios = require('axios');
const isomorphicFetch = require('isomorphic-fetch');
const { SimpleGraphClient } = require('./simpleGraphClient');

var delegatedToken = "";
var applicationToken = "";
var localdata = [];
var recipientPartitionSize = 85;

app.use(express.static(path.join(__dirname, 'static')));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// parse application/json
app.use(express.json());

app.get('/broadcast', function (req, res) {
  var tenantId = process.env.TenantId;
  auth.getAccessToken(tenantId).then(async function (token) {
    applicationToken = token;
    var requestData = localdata;
    res.render('./views/BroadcastNotification', { data: JSON.stringify(requestData) });
  });
});

app.get('/BroadcastDetails', function (req, res) {
  var requestId = req.url.split('=')[1];
  let requestData = {};
  if (requestId != null) {
    localdata.map(item => {
      if (item.id == requestId) {
        requestData = item;
      }
    })
  }
  res.render('./views/BroadcastDetails', { data: JSON.stringify(requestData) });
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
app.get('/auth-start', function (req, res) {
  res.render('./views/auth-start', { clientId: JSON.stringify(process.env.ClientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
app.get('/auth-end', function (req, res) {
  var clientId = process.env.ClientId;
  res.render('./views/auth-end', { clientId: JSON.stringify(process.env.ClientId) });
});

// On-behalf-of token exchange
app.post('/auth/token', function (req, res) {
  var tid = req.body.tid;
  var token = req.body.token;
  var scopes = ["https://graph.microsoft.com/User.Read"];

  // Creating MSAL client
  const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: process.env.ClientId,
      clientSecret: process.env.ClientSecret
    }
  });

  var oboPromise = new Promise((resolve, reject) => {
    msalClient.acquireTokenOnBehalfOf({
      authority: `https://login.microsoftonline.com/${tid}`,
      oboAssertion: token,
      scopes: scopes,
      skipCache: true
    }).then(result => {
      delegatedToken = result.accessToken
      resolve();
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  oboPromise.then(function (result) {
    res.json(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});

// Send notification to group chat for task creation.
app.post('/SendNotificationToOrganisation', async (req, res) => {
  var taskDetails = {
    id: req.body.id,
    title: req.body.title,
    description: req.body.description,
    createdBy: req.body.userName,
    userId: req.body.userId
  };

  localdata.push(taskDetails);
  var appId;

  const client = new SimpleGraphClient(delegatedToken);

  var response = await client.getInstalledAppsForUser(req.body.userId);
  appId = getAppId(response);
  var userList = await client.getUserList();
  var recipientsList = [];

  if (userList.value) {
    await forEachAsync(userList.value, async (users) => {
      try 
      {
        let appList = await client.getInstalledAppsForUser(users.id);

        if (appList) {
          let userAppId = getAppId(appList);
          if (userAppId == undefined) {
            await client.installAppForUser(users.id, appId)
          }

          recipientsList.push({
            "@odata.type": "microsoft.graph.aadUserNotificationRecipient",
            "userId": users.id
          });
        }
      }
      catch (ex) {
        console.error("Installation failed for " + users.displayName);
      }
    });

    var recipientsChunks = splitIntoChunks(recipientsList, recipientPartitionSize);

    for (let recipientChunk of recipientsChunks) 
    {
      var encodedContext = encodeURI('{"subEntityId": ' + req.body.id + '}');
      let postData = 
      {
        "topic": {
          "source": "text",
          "value": req.body.title,
          "webUrl": 'https://teams.microsoft.com/l/entity/' + appId + '/broadcast?context=' + encodedContext
        },
        "activityType": "approvalRequired",
        "previewText": {
          "content": "Broadcast by" + req.body.userName
        },
        "recipients": recipientChunk,
        "templateParameters": [
          {
            "name": "approvalTaskId",
            "value": req.body.title
          }
        ]
      };

      axios.post("https://graph.microsoft.com/beta/teamwork/sendActivityNotificationToRecipients", postData, {
        headers: {
          "accept": "application/json",
          "contentType": 'application/json',
          "authorization": "bearer " + applicationToken
        }
      })
        .then(res => {
          console.log(`statusCode: ${res.status}`);
        });
    }
  }
});

async function forEachAsync(usersList, callback) {
  await Promise.all(usersList.map(async (users) => {
    await callback(users);
  }));
}

function splitIntoChunks(array, chunkSize) {
  return Array(Math.ceil(array.length / chunkSize)).fill().map(function (_, i) {
    return array.slice(i * chunkSize, i * chunkSize + chunkSize);
  });
}

// Get app id.
function getAppId(appList) {
  var list = appList.value;
  for (var i = 0; i < list.length; i++) {
    if (list[i].teamsAppDefinition['displayName'] == process.env.AppName) {
      return list[i].teamsAppDefinition['teamsAppId'];
    }
  }
}

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});