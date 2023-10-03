const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');

const auth = require('./auth');
const indexRouter = require('./routes/index');
require('isomorphic-fetch');

const app = express();

app.use(express.static(__dirname + '/Styles'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

var token;
var recipientId;

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
  var tenantId = process.env.TenantId;
    res.render('./views/sendNotification');
});

app.post('/sendFeedNotification', function (req, res) {
    recipientId = req.data.reciepientUserId;
    token = auth.getAccessToken(tenantId).then(async function (token) {
    await getInstalledAppList(token, recipientId);
  })
});

// Get installed app id.
function getAppId(appList) {
  var list = appList;
  var i;
  for (i = 0; i < list.length; i++) {
    if (list[i].teamsAppDefinition['displayName'] == "RSC-GraphAPI NodeJs") {
      return list[i].id;
    }
  }
}

// Fetch the list of installed apps for user
async function getInstalledAppList(accessToken, reciepientUserId) {
  $.ajax({
    url: "https://graph.microsoft.com/v1.0/users/" + reciepientUserId + "/teamwork/installedApps/?$expand=teamsAppDefinition",
    type: "GET",
    beforeSend: function (request) {
      request.setRequestHeader("Authorization", "Bearer " + accessToken);
    },
    success: function (result) {
      var appId = getAppId(result.value);
      sendActivityFeedNotification(token, reciepientUserId, appId)
    },
    error: function (xhr, textStatus, errorThrown) {
      console.log("textStatus: " + textStatus + ", errorThrown:" + errorThrown);
    },
  });
}

// Send activity feed notification to user
async function sendActivityFeedNotification(accessToken, reciepientUserId, appId) {

  var postData = {
    topic: {
      source: "entityUrl",
      value: `https://graph.microsoft.com/beta/users/${reciepientUserId}/teamwork/installedApps/${appId}`
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

  $.ajax({
    url: `https://graph.microsoft.com/beta/users/${reciepientUserId}/teamwork/sendActivityNotification`,
    type: "POST",
    contentType: 'application/json',
    data: JSON.stringify(postData),
    beforeSend: function (request) {
      request.setRequestHeader("Authorization", "Bearer " + accessToken);
    },
    success: function (profile) {
      console.log(profile);
    },
    error: function (xhr, textStatus, errorThrown) {
      console.log("textStatus: " + textStatus + ", errorThrown:" + errorThrown);
    },
  });
}


app.listen(3978 || 3978, function () {
  console.log('app listening on port 3978!');
});