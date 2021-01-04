'use strict';
const fetch = require("node-fetch");
const querystring = require("querystring");
var config = require('config');

module.exports.setup = function(app) {
    var path = require('path');
    var express = require('express')
    
    // Configure the view engine, views folder and the statics path
    app.use(express.static(path.join(__dirname, 'static')));
    app.set('view engine', 'pug');
    app.set('views', path.join(__dirname, 'views'));
    
    // Use the JSON middleware
    app.use(express.json());
    
    // Setup home page
    app.get('/', function(req, res) {
        res.render('hello');
    });
    
    // Setup the configure tab, with first and second as content tabs
    app.get('/configure', function(req, res) {
        res.render('configure');
    });    

    // ------------------
    // SSO demo page
    app.get('/ssodemo', function(req, res) {
        res.render('ssoDemo');
    }); 

    // Pop-up dialog to ask for additional permissions, redirects to AAD page
    app.get('/auth/auth-start', function(req, res) {
        var clientId = config.get("tab.appId");
        res.render('auth-start', { clientId: clientId });
    });

    // End of the pop-up dialog auth flow, returns the results back to parent window
    app.get('/auth/auth-end', function(req, res) {
        var clientId = config.get("tab.appId");
        res.render('auth-end', { clientId: clientId });
    }); 

    // On-behalf-of token exchange
    app.post('/auth/token', function(req, res) {
        var tid = req.body.tid;
        var token = req.body.token;
        var scopes = ["https://graph.microsoft.com/User.Read"];

        var oboPromise = new Promise((resolve, reject) => {
            const url = "https://login.microsoftonline.com/" + tid + "/oauth2/v2.0/token";
            const params = {
                client_id: config.get("tab.appId"),
                client_secret: config.get("tab.appPassword"),
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
                  reject({"error":json.error});
                });
              } else {
                result.json().then(json => {
                  resolve(json.access_token);
                });
              }
            });
        });

        oboPromise.then(function(result) {
            res.json(result);
        }, function(err) {
            console.log(err); // Error: "It broke"
            res.json(err);
        });
    });

};
