// <copyright file="auth.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

var request = require('request');
// var graph = require('@microsoft/microsoft-graph-client');
var Q = require('q');

const axios = require('axios'); 
// The auth module object.
var auth = {}; 
 
const path = require('path');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const APP_REGISTRATION_ID = process.env.APP_REGISTRATION_ID;
const CLIENT_SECRET = process.env.CLIENT_SECRET;

// Function to obtain an access token using the tenant ID.
auth.getAccessToken = function (tenantId) {
    var deferred = Q.defer();
    var requestParams = {
      grant_type: 'client_credentials',
      client_id: APP_REGISTRATION_ID,
      client_secret: CLIENT_SECRET,
      scope: 'https://graph.microsoft.com/.default'
    };
  
    var url = "https://login.microsoftonline.com/"+ tenantId+"/oauth2/v2.0/token";
    request.post({ url: url, form: requestParams }, function (err, response, body) {
      var parsedBody = JSON.parse(body);
  
      if (err) {
        deferred.reject(err);
        console.log(err);
      } else if (parsedBody.error) {
        console.log(parsedBody.error_description);
        deferred.reject(parsedBody.error_description);
      } else {
        console.log("success");
        // If successful, return the access token.
        deferred.resolve(parsedBody.access_token);
      }
    });
  
    return deferred.promise;
  };
  
  module.exports = auth;