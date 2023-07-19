const express = require('express');
// const bodyparser = require('body-parser');
// const env = require('dotenv');
const path = require('path');
const auth = require('./auth');
const fetch = require("node-fetch");
const querystring = require("querystring");
//const server = express();

// server.use(express.static(path.join(__dirname, 'static')));
// server.engine('html', require('ejs').renderFile);
// server.set('view engine', 'ejs');
// server.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

var localdata = [];

const server = express();

const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);

server.use(express.static(path.join(__dirname, 'static')));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

// parse serverlication/json
//server.use(express.json());

server.get('/UserNotification', function (req, res) {
  var tenantId = process.env.TenantId;
  
  auth.getAccessToken(tenantId).then(async function (token) {
    var requestData = {
      "requestDetails": localdata,
      "token": token
    };

    res.render('./views/UserNotification', { data: JSON.stringify(requestData) });
  });
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
server.get('/auth/auth-start', function (req, res) {
  var clientId = process.env.ClientId;
  res.render('./views/auth-start', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/auth/auth-end', function (req, res) {
  var clientId = process.env.ClientId;
  res.render('./views/auth-end', { clientId: JSON.stringify(clientId) });
});

server.get('/tabAuth', function (req, res) {
  res.render('./views/tabAuth');
});

server.get('/UserRequest', function (req, res) {
  var requestId = req.url.split('=')[1];
  let requestData = {};
  if(requestId != null){
    localdata.map(item => {
      if(item.id == requestId){
         requestData = item;
      }
    })
  }
  res.render('./views/UserRequest', { data: JSON.stringify(requestData) });
});

server.post('/serverroveRejectRequest', function (req, res) {
localdata.map((item, index) => {
  if(item.id == req.body.taskId){
    item.status = req.body.status;
  }
})
});

server.post('/SaveRequest', function (req, res) {
  var taskDetails = {
    id: req.body.id,
    title: req.body.title,
    description: req.body.description,
    assignedTo: req.body.assignedTo,
    createdBy: req.body.createdBy,
    status: "Pending"
  };

  localdata.push(taskDetails);
 });

// On-behalf-of token exchange
server.post('/auth/token', function (req, res) {
  var tid = req.body.tid;
  var token = req.body.token;
  var scopes = ["https://graph.microsoft.com/User.Read"];

  var oboPromise = new Promise((resolve, reject) => {
    const url = "https://login.microsoftonline.com/" + tid + "/oauth2/v2.0/token";
    const params = {
      client_id: process.env.ClientId,
      client_secret: process.env.ClientSecret,
      grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
      assertion: token,
      requested_token_use: "on_behalf_of",
      scope: scopes.join(" ")
    };

    fetch(url, {
      method: "POST",
      body: querystring.stringify(params),
      headers: {
        Accept: "serverlication/json",
        "Content-Type": "serverlication/x-www-form-urlencoded"
      }
    }).then(result => {
      
      if (result.status !== 200) {
        result.json().then(json => {
          // TODO: Check explicitly for invalid_grant or interaction_required
          reject({ "error": json.error });
        });
      } else {
        result.json().then(json => {
          resolve(json.access_token);
        });
      }
    });
  });

  oboPromise.then(function (result) {
    res.json(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});

// server.listen(3978, function () {
//   console.log('server listening on port 3978!');
// });