const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');
const fs = require('fs');
const auth = require('./auth');
const indexRouter = require('./routes/index');
require('isomorphic-fetch');
const axios = require('axios');

const app = express();

app.use(bodyparser.urlencoded({ extended: false }))
app.use(bodyparser.json())
app.use(express.static(__dirname + '/Styles'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

app.use('/', indexRouter);

app.get('/configure', function (req, res) {
  res.render('./views/configure');
});

app.get('/rscdemo', function (req, res) {
  var tenantId = req.url.split('=')[1];
  auth.getAccessToken(tenantId).then(async function (token) {
    res.render('./views/rscdemo', { token: JSON.stringify(token) });
  });
});

app.get('/sendNotification', function (req, res) {
  res.render('./views/sendNotification');
});

app.get('/RSCGraphAPI', function (req, res) {
  fs.readFile('APImethods.json', 'utf8', (err, data) => {
    if (err) {
        console.error('Error reading file:', err);
        return;
    }

    try {
        // Parse JSON data
        const items = JSON.parse(data);
        console.log(items);
        res.render('./views/RSCGraphAPI',{items});
    } catch (error) {
        console.error('Error parsing JSON:', error);
    }
});
});

app.post('/sendFeedNotification', function (req, res) {
  var recipientId = req.body.recipientUserId;
  var tenantId = req.body.tenantId;
  sendNotificationFlow(tenantId, recipientId).then(function(){
    console.log('Notification send success');
    res.status(200).send('ok')
  }).catch(function(err){
    res.status(500).send('error: ' + err.message)
  });
});

async function sendNotificationFlow(tenantId, recipientId) {
  var token = await auth.getAccessToken(tenantId);
  var appId = await getAppId(token, recipientId);
  await sendActivityFeedNotification(token, recipientId, appId);
}

// Get installed app id.
function findAppIdInList(appList) {
  for (var i = 0; i < appList.length; i++) {
    if (appList[i].teamsAppDefinition['displayName'] == "RSC-GraphAPI NodeJs") {
      return appList[i].id;
    }
  }
}

// Fetch the list of installed apps for user
async function getAppId(accessToken, reciepientUserId) {

  const config = {
    headers: {
      Authorization: "Bearer " + accessToken
    }
  };

  var res = await axios.get("https://graph.microsoft.com/v1.0/users/" + reciepientUserId + "/teamwork/installedApps/?$expand=teamsAppDefinition", config)
  var appId = findAppIdInList(res.data.value);
  return appId;
}

// Send activity feed notification to user
async function sendActivityFeedNotification(accessToken, recipientUserId, appId) {

  var postData = {
    topic: {
      source: "entityUrl",
      value: `https://graph.microsoft.com/beta/users/${recipientUserId}/teamwork/installedApps/${appId}`
    },
    activityType: "taskCreated",
    previewText: {
      content: "New Task Created"
    },
    templateParameters: [
      {
        name: "taskName",
        value: "test"
      }
    ]
  };

  const config = {
    headers: {
      Authorization: "Bearer " + accessToken
    }
  };
  
  await axios.post(`https://graph.microsoft.com/beta/users/${recipientUserId}/teamwork/sendActivityNotification`, postData, config)
  console.log('Notification sent');
}

app.listen(3978 || 3978, function () {
  console.log('app listening on port 3978!');
});