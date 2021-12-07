'use strict';
const fetch = require("node-fetch");
const querystring = require("querystring");
var config = require('config');
const msal = require('@azure/msal-node');

module.exports.setup = function (app) {
  var path = require('path');
  var express = require('express')

  // Configure the view engine, views folder and the statics path
  app.use(express.static(path.join(__dirname, 'static')));
  app.set('view engine', 'pug');
  app.set('views', path.join(__dirname, 'views'));

  // Use the JSON middleware
  app.use(express.json());

  // Setup home page
  app.get('/', function (req, res) {
    res.render('hello');
  });

  // Setup the configure tab, with first and second as content tabs
  app.get('/configure', function (req, res) {
    res.render('configure');
  });

  // ------------------
  // SSO demo page
  app.get('/ssodemo', function (req, res) {
    res.render('ssoDemo');
  });

  // Pop-up dialog to ask for additional permissions, redirects to AAD page
  app.get('/auth/auth-start', function (req, res) {
    var clientId = config.get("tab.appId");
    res.render('auth-start', { clientId: clientId });
  });

  // End of the pop-up dialog auth flow, returns the results back to parent window
  app.get('/auth/auth-end', function (req, res) {
    var clientId = config.get("tab.appId");
    res.render('auth-end', { clientId: clientId });
  });

  // On-behalf-of token exchange
  app.post('/getProfile', function (req, res) {
    var tid = req.body.tid;
    var token = req.body.token;
    var scopes = ["https://graph.microsoft.com/User.Read"];

    // Creating MSAL client
    const msalClient = new msal.ConfidentialClientApplication({
      auth: {
        clientId: config.get("tab.appId"),
        clientSecret: config.get("tab.appPassword")
      }
    });
    
    var oboPromise = new Promise((resolve, reject) => {
      msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tid}`,
        oboAssertion: token,
        scopes: scopes,
        skipCache: true
      }).then(result => {
            fetch("https://graph.microsoft.com/v1.0/me/",
              {
                method: 'GET',
                headers: {
                  "accept": "application/json",
                  "authorization": "bearer " + result.accessToken
                },
                mode: 'cors',
                cache: 'default'
              })
              .then((response) => {
                if (response.ok) {
                  return response.json();
                } else {
                  throw (`Error ${response.status}: ${response.statusText}`);
                }
              })
              .then((profile) => {
                resolve(profile);
              })
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

  app.post('/getUserProfile', function (req, res) {
    var token = req.body.token;
    var oboPromise = new Promise((resolve, reject) => {
      fetch("https://graph.microsoft.com/v1.0/me/",
        {
          method: 'GET',
          headers: {
            "accept": "application/json",
            "authorization": "bearer " + token
          },
          mode: 'cors',
          cache: 'default'
        })
        .then((response) => {
          if (response.ok) {
            return response.json();
          } else {
            throw (`Error ${response.status}: ${response.statusText}`);
          }
        })
        .then((profile) => {
          resolve(profile);
        });
    });

    oboPromise.then(function (result) {
      res.json(result);
    }, function (err) {
      console.log(err); // Error: "It broke"
      res.json(err);
    });   
  });
};