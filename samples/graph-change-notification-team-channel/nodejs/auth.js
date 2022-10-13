// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

var request = require('request');
var Q = require('q');

// The auth module object.
var auth = {};

// Gets the application level access token.
auth.getAccessToken = function () {
  var deferred = Q.defer();
  var requestParams = {
    grant_type: 'client_credentials',
    client_id: process.env.MicrosoftAppId,
    client_secret: process.env.MicrosoftAppPassword,
    scope: 'https://graph.microsoft.com/.default'
  };

  var url = "https://login.microsoftonline.com/" + process.env.MicrosoftAppTenantId + "/oauth2/v2.0/token";
  request.post({ url: url, form: requestParams }, function (err, response, body) {
    var parsedBody = JSON.parse(body);

    if (err) {
      deferred.reject(err);
    } else if (parsedBody.error) {
      deferred.reject(parsedBody.error_description);
    } else {
      // If successful, return the access token.
      deferred.resolve(parsedBody.access_token);
    }
  });

  return deferred.promise;
};

module.exports = auth;