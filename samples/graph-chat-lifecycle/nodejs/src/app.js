'use strict';

var config = require('config');
console.log(config["port"]);
var express = require('express');
const server = express();
var Helper = require("../helpers/chatHelper");

// Decide which port to use
var port = process.env.PORT || config["port"] ? config["port"] : 3978;

// Listen for incoming requests
server.listen(port, function () {
  console.log(`App started listening on port ${port}`);
});


const fetch = require("node-fetch");
const querystring = require("querystring");

var bodyParser = require("body-parser");
server.use(express.static(__dirname + '/views'));
server.use(express.static(__dirname + '/static'));
server.set("view engine", "ejs");
server.set("views", __dirname + "/views");

server.engine('html', require('ejs').renderFile);
server.use(bodyParser.urlencoded({ extended: false }));
server.get('/tab', (req, res) => { res.render('chatLifecycle.html') });

server.post('/api/getAdaptiveCard', Helper.getAdaptiveCard);
server.post('/api/createGroupChat', Helper.createGroupChat);

// Use the JSON middleware
server.use(express.json());

// Setup the configure tab, with first and second as content tabs
server.get('/configure', function (req, res) {
  res.render('configure.html');
});

server.post('/CreateAdaptiveCard', function (request, response) {
  response.sendFile('adaptive.html');
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
server.get('/auth/auth-start', function (req, res) {
  var clientId = config["tab"].appId;
  res.render('auth-start.html', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/auth/auth-end', function (req, res) {
  var clientId = config["tab"].appId;
  res.render('auth-end.html', { clientId: JSON.stringify(clientId) });
});

// On-behalf-of token exchange
server.post('/auth/token', function (req, res) {
  const tid = req.body.tid;
  const token = req.body.token;
  var scopes = ["https://graph.microsoft.com/User.Read"];

  var oboPromise = new Promise((resolve, reject) => {
    const url = "https://login.microsoftonline.com/" + tid + "/oauth2/v2.0/token";
    const params = {
      client_id: config["tab"]["appId"],
      client_secret: config["tab"]["appPassword"],
      grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
      assertion: token,
      requested_token_use: "on_behalf_of",
      scope: scopes.join(" ")
    };

    fetch(url, {
      method: "POST",
      body: querystring.stringify(params),
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded"
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




