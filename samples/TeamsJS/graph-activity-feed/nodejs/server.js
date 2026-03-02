const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');
const auth = require('./auth');
const app = express();
const msal = require('@azure/msal-node');
const axios = require('axios');

var delegatedToken = "";
var applicationToken = "";

app.use(bodyparser.urlencoded({ extended: false }))
app.use(bodyparser.json())
app.use(express.static(path.join(__dirname, 'static')));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// parse application/json
app.use(express.json());

app.get('/configure', function (req, res) {
  res.render('./views/configure');
});

app.get('/GroupChatNotification', function (req, res) {
  var tenantId = process.env.TenantId;
  auth.getAccessToken(tenantId).then(async function (token) {
    applicationToken = token;
    res.render('./views/GroupChatNotification', { clientId: JSON.stringify(process.env.ClientId), tenantId: JSON.stringify(process.env.TenantId) });
  });
});

app.get('/TeamNotification', function (req, res) {
  var tenantId = process.env.TenantId;
  auth.getAccessToken(tenantId).then(async function (token) {
    applicationToken = token;
    res.render('./views/TeamNotification', { clientId: JSON.stringify(process.env.ClientId), tenantId: JSON.stringify(process.env.TenantId) });
  });
});

app.get('/UserNotification', function (req, res) {
  var tenantId = process.env.TenantId;
  auth.getAccessToken(tenantId).then(async function (token) {
    applicationToken = token;
    res.render('./views/UserNotification', { clientId: JSON.stringify(process.env.ClientId), tenantId: JSON.stringify(process.env.TenantId) });
  });
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
app.get('/auth-start', function (req, res) {
  res.render('./views/auth-start', { clientId: JSON.stringify(process.env.ClientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
app.get('/auth-end', function (req, res) {
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
    console.log(err);
    res.json(err);
  });
});

// Send notification to group chat for task creation.
app.post('/sendNotificationToGroupChat', (req, res) => {
  const postData = {
    "topic": {
      "source": "entityUrl",
      "value": "https://graph.microsoft.com/beta/chats/" + req.body.groupchatId
    },
    "activityType": "taskCreated",
    "previewText": {
      "content": "New Task Created"
    },
    "recipient": {
      "@odata.type": "microsoft.graph.aadUserNotificationRecipient",
      "userId": process.env.UserId
    },
    "templateParameters": [
      {
        "name": "taskName",
        "value": req.body.title
      }
    ],
    "iconId": "taskCreatedId"

  };

  var url = "https://graph.microsoft.com/beta/chats/" + req.body.groupchatId + "/sendActivityNotification"
    axios.post(url, postData, {
      headers: {
        "accept": "application/json",
        "contentType": 'application/json',
        "authorization": "bearer " + req.body.accessToken
      }
    })
      .then(res => {
        console.log(`statusCode: ${res.status}`)
        console.log(res)
      })
      .catch(error => {
        console.error(error)
      })
  
});

// Send notification of custom topic in group chat.
app.post('/sendCustomTopicNotificationGroupChat', (req, res) => {
  const message = {
    "body": {
      "content": "New Deployment: " + req.body.DeployementTitle
    }
  }

  var url = "https://graph.microsoft.com/beta/chats/" + req.body.groupchatId + "/messages"
  axios.post(url, message, {
    headers: {
      "accept": "application/json",
      "contentType": 'application/json',
      "authorization": "bearer " + req.body.accessToken
    }
  })
    .then(res => {
      const postData = {
        "topic": {
          "source": "entityUrl",
          "value": "https://graph.microsoft.com/beta/chats/" + req.body.groupchatId
        },
        "activityType": "approvalRequired",
        "previewText": {
          "content": "Deployment requires your approval"
        },
        "recipient": {
          "@odata.type": "microsoft.graph.aadUserNotificationRecipient",
          "userId": process.env.UserId
        },
        "templateParameters": [
          {
            "name": "approvalTaskId",
            "value": req.body.DeployementTitle
          }

        ],
        "iconId": "approvalRequiredId"
      }

      axios.post("https://graph.microsoft.com/beta/chats/" + req.body.groupchatId + "/sendActivityNotification", postData, {
        headers: {
          "accept": "application/json",
          "contentType": 'application/json',
          "authorization": "bearer " + req.body.accessToken
        }
      }).then(response => {
        console.log(`statusCode: ${response.status}`)
      })
    })
    .catch(error => {
      console.error(error)
    })
});

// Send notification to channel in a team for task creation.
app.post('/sendNotificationToChannel', (req, res) => {
  const postData = {
    "topic": {
      "source": "entityUrl",
      "value": "https://graph.microsoft.com/beta/teams/" + req.body.teamId
    },
    "activityType": "pendingFinanceApprovalRequests",
    "previewText": {
      "content": "Internal spending team has a pending finance approval requests"
    },
    "recipient": {
      "@odata.type": "microsoft.graph.aadUserNotificationRecipient",
      "userId": process.env.UserId
    },
    "templateParameters": [
      {
        "name": "pendingRequestCount",
        "value": req.body.title
      }
    ],
    "iconId": "pendingFinanceApprovalRequestsId"
  };

  var url = "https://graph.microsoft.com/beta/teams/" + req.body.teamId + "/sendActivityNotification"
  axios.post(url, postData, {
    headers: {
      "accept": "application/json",
      "contentType": 'application/json',
      "authorization": "bearer " + req.body.accessToken
    }
  })
    .then(res => {
      console.log(`statusCode: ${res.status}`)
      console.log(res)
    })
    .catch(error => {
      console.error(error)
    })
});

// Send notification for channel tab.
app.post('/channelTabTeamNotification', (req, res) => {
  var url = "https://graph.microsoft.com/v1.0/teams/" + req.body.teamId + "/channels/" + req.body.channelId + "/tabs?$expand=teamsApp"

  axios.get(url, {
    headers: {
      "accept": "application/json",
      "contentType": 'application/json',
      "authorization": "bearer " + req.body.accessToken
    }
  })
    .then(res => {
      var tabUrl = getTabURL(res.data);
      const postData = {
        "topic": {
          "source": "entityUrl",
          "value": "https://graph.microsoft.com/beta/teams/" + req.body.teamId + "/channels/" + req.body.channelId + "/tabs/" + tabUrl
        },
        "activityType": "reservationUpdated",
        "previewText": {
          "content": "You have moved up the queue"
        },
        "recipient": {
          "@odata.type": "microsoft.graph.aadUserNotificationRecipient",
          "userId": req.body.userId
        },
        "templateParameters": [
          {
            "name": "reservationId",
            "value": req.body.id
          },
          {
            "name": "currentSlot",
            "value": req.body.currentSlot
          }
        ]
      };

      axios.post("https://graph.microsoft.com/beta/teams/" + req.body.teamId + "/sendActivityNotification", postData, {
        headers: {
          "accept": "application/json",
          "contentType": 'application/json',
          "authorization": "bearer " + req.body.accessToken
        }
      }).then(response => {
        console.log(`statusCode: ${response.status}`)
      })

    })
    .catch(error => {
      console.error(error)
    })
});

// Send notification for custom topic in team.
app.post('/customTopicTeamNotification', (req, res) => {
  const message = {
    "body": {
      "content": "New Deployment :" + req.body.title
    }
  };
  var url = "https://graph.microsoft.com/beta/teams/" + req.body.teamId + "/channels/" + req.body.channelId + "/messages"

  axios.post(url, message, {
    headers: {
      "accept": "application/json",
      "contentType": 'application/json',
      "authorization": "bearer " + req.body.accessToken
    }
  })
    .then(res => {
      const postData = {
        "topic": {
          "source": "text",
          "value": "Deployment Approvals Channel",
          "webUrl": res.data.webUrl,
        },
        "activityType": "deploymentApprovalRequired",
        "previewText": {
          "content": "New deployment requires your approval"
        },
        "recipient": {
          "@odata.type": "microsoft.graph.aadUserNotificationRecipient",
          "userId": process.env.UserId
        },
        "templateParameters": [
          {
            "name": "deploymentId",
            "value": req.body.title
          }
        ]
      };

      axios.post("https://graph.microsoft.com/v1.0/teams/" + req.body.teamId + "/sendActivityNotification", postData, {
        headers: {
          "accept": "application/json",
          "contentType": 'application/json',
          "authorization": "bearer " + req.body.accessToken
        }
      }).then(response => {
        console.log(`statusCode: ${response.status}`)
      })
    })
    .catch(error => {
      console.error(error)
    })
});

// Send notification to user.
app.post('/sendNotificationToUser', (req, res) => {
  var url = "https://graph.microsoft.com/beta/users/" + req.body.userId + "/teamwork/installedApps/?$expand=teamsAppDefinition"

  axios.get(url, {
    headers: {
      "accept": "application/json",
      "contentType": 'application/json',
      "authorization": "bearer " + req.body.accessToken
    }
  }).then(res => {
    var appId = getAppId(res.data);
    const postData = {
      "topic": {
        "source": "text",
        "value": "Loop thread",
        "webUrl": "https://teams.microsoft.com/l/entity/" + process.env.TeamsAppId
      },
      "activityType": "approvalRequired",
      "previewText": {
        "content": "new announcement posted"
      },
      "templateParameters": [
        {
          "name": "approvalTaskId",
          "value": req.body.title
        }
      ],
      "iconId": "approvalRequiredId"
    };

    axios.post("https://graph.microsoft.com/beta/users/" + process.env.UserId +"/teamwork/sendactivitynotification",
      postData,
      {
        headers: {
          "accept": "application/json",
          "contentType": 'application/json',
          "authorization": "bearer " + req.body.accessToken
        }
      }).then(response => {
        console.log(`statusCode: ${response.status}`)
      })
  }
  ).catch(error => {
    console.log("error:" + error)
  })
});


// Send notification for custom topic to user.
app.post('/customTopicNotificationToUser', (req, res) => {
  const message = {
    "body": {
      "content": "New Deployment: " + req.body.DeployementTitle
    }
  };
  var url = "https://graph.microsoft.com/beta/teams/" + req.body.teamId + "/channels/" + req.body.channelId + "/messages"

  axios.post(url, message, {
    headers: {
      "accept": "application/json",
      "contentType": 'application/json',
      "authorization": "bearer " + req.body.accessToken
    }
  })
    .then(res => {
      const postData = {
        "topic": {
          "source": "text",
          "value": "Deployment Approvals Channel",
          "webUrl": res.data.webUrl
        },
        "activityType": "deploymentApprovalRequired",
        "previewText": {
          "content": "New deployment requires your approval"
        },
        "templateParameters": [
          {
            "name": "deploymentId",
            "value": req.body.DeployementTitle
          }
        ],
        "iconId": "deploymentApprovalRequiredId"
      }

      axios.post("https://graph.microsoft.com/beta/users/" + process.env.UserId + "/teamwork/sendActivityNotification", postData, {
        headers: {
          "accept": "application/json",
          "contentType": 'application/json',
          "authorization": "bearer " + req.body.accessToken
        }
      }).then(response => {
        console.log(`statusCode: ${response.status}`)
      })
    })
    .catch(error => {
      console.error(error)
    })
})

// Get tab Url.
function getTabURL(appList) {
  list = appList.value;
  var i = 0;
  for (i = 0; i < list.length; i++) {
    if (list[i].teamsApp['displayName'] == "NotifyFeedApp") {
      return list[i].id;
    }
  }
}

// Get app id.
function getAppId(appList) {
  var list = appList.value;
  var i;
  for (i = 0; i < list.length; i++) {
    if (list[i].teamsAppDefinition['displayName'] == "NotifyFeedApp") {
      return list[i].id;
    }
  }
}

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});