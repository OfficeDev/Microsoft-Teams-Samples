'use strict';
const fetch = require("node-fetch");
var config = require('config');
const msal = require('@azure/msal-node');

module.exports.setup = function (app) {
  var express = require('express')

  // Creating MSAL client
  const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: config.get("tab.appId"),
      clientSecret: config.get("tab.clientSecret")
    }
  });

  // Configure the view engine, views folder and the statics path
  // Use the JSON middleware
  app.use(express.json());

    // Setup the configure tab, with first and second as content tabs
  app.get('/configure', function (req, res) {
    res.render('configure');
  });

  // ------------------
  // SSO demo page
  app.get('/ssoDemo', function (req, res) {
    var clientId = config.get("tab.appId");
    var applicationIdUri = config.get("tab.applicationIdUri");
    res.render('ssoDemo', { clientId: clientId, applicationIdUri: applicationIdUri });
  });

  // Pop-up dialog to ask for additional permissions, redirects to AAD page
  app.get('/auth-start', function (req, res) {
    var clientId = config.get("tab.appId");
    res.render('auth-start', { clientId: clientId });
  });

  // End of the pop-up dialog auth flow, returns the results back to parent window
  app.get('/auth-end', function (req, res) {
    var clientId = config.get("tab.appId");
    res.render('auth-end', { clientId: clientId });
  });

  app.get('/Home/BrowserRedirect', function (req, res) {
    var clientId = config.get("tab.appId");
    var applicationIdUri = config.get("tab.applicationIdUri");
    res.render('browser-redirect', { clientId: clientId, applicationIdUri: applicationIdUri });
  });

  // On-behalf-of token exchange
  app.post('/getProfileOnBehalfOf', function (req, res) {
    var tid = req.body.tid;
    var token = req.body.token;
    var scopes = ["https://graph.microsoft.com/User.Read"];
    
    var oboPromise = new Promise((resolve, reject) => {
      msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tid}`,
        oboAssertion: token,
        scopes: scopes,
        skipCache: false
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
};