const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');
const auth = require('./auth');
const app = express();
const msal = require('@azure/msal-node');
const axios = require('axios');
const polly = require('polly-js');
const { polyfills } = require('isomorphic-fetch');
const { SimpleGraphClient } = require('./simpleGraphClient');
const peach = require('parallel-each');

var delegatedToken = "";
var applicationToken = "";
var localdata = [];
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
  res.render('./views/auth-end', { clientId: clientId });
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

  var response =  await client.getInstalledAppsForUser(req.body.userId);
  appId = getAppId(response);
  var userList = await client.getUserList();

  if(userList.value){
    peach(userList.value, async( users) =>{
      let appList = await client.getInstalledAppsForUser(users.id);
      if(appList){
        let userAppId = getAppId(appList);
        var encodedContext = encodeURI('{"subEntityId": ' + req.body.id + '}');
          const postData = {
            "topic": {
              "source": "text",
              "value": req.body.title,
              "webUrl": 'https://teams.microsoft.com/l/entity/' + appId + '/broadcast?context=' + encodedContext
            },
            "activityType": "approvalRequired",
            "previewText": {
              "content": "Broadcast by"+ req.body.userName
            },
            "templateParameters": [
              {
                "name": "approvalTaskId",
                "value": req.body.title
              }
            ]
          };

          if (userAppId == undefined) {
            var appInstalled = client.installAppForUser(users.id,appId)
            if(appInstalled)
            {
              axios.post("https://graph.microsoft.com/v1.0/users/" + users.id + "/teamwork/sendActivityNotification", postData, {
                headers: {
                  "accept": "application/json",
                  "contentType": 'application/json',
                  "authorization": "bearer " + applicationToken
                }
              })
              .then(res => {
                console.log(`statusCode: ${res.status}`)
                if(res.status == 429)
                {
                  handleTooManyRequestError(users.id, postData);
                }
              })
              .catch(error=>{
              console.log(error);
            });
            }
          }
          else{
            axios.post("https://graph.microsoft.com/v1.0/users/" + users.id + "/teamwork/sendActivityNotification", postData, {
              headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "bearer " + applicationToken
              }
            })
            .then(res => {
              console.log(`statusCode: ${res.status}`)
              if(res.status == 429)
              {
                handleTooManyRequestError(users.id,postData);
              }
            })
          }
      }
    }
    );
  }
});

// Get app id.
function getAppId(appList) {
  var list = appList.value;
  for (var i = 0; i < list.length; i++) {
    if (list[i].teamsAppDefinition['displayName'] == "Activity feed broadcast") {
      return list[i].teamsAppDefinition['teamsAppId'];
    }
  }
}

// Handle too many request (429?) error.
function handleTooManyRequestError(userId,postData)
{
  polly().
  retry(3).
  executeForPromise(function () {
    return axios.post("https://graph.microsoft.com/v1.0/users/" + userId + "/teamwork/sendActivityNotification", postData, {
      headers: {
        "accept": "application/json",
        "contentType": 'application/json',
        "authorization": "bearer " + applicationToken
      }
    })
  })
  .then(function(result) {
    console.log(`statusCode: ${result.status}`)
  }, 
  function(err) {console.error('Failed trying three times', err)})
}

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});