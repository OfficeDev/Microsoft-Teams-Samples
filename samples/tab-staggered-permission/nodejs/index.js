// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const express = require('express');
const cors = require('cors');
const { SimpleGraphClient } = require('./simpleGraphClient');
const msal = require('@azure/msal-node');
const { polyfills } = require('isomorphic-fetch');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

// Create HTTP server.
server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});

// Endpoint to fetch Auth tab page.
server.get('/tab', (req, res, next) => {
  res.render('./views/tab')
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
server.get('/auth-start', function (req, res) {
  var clientId =process.env.MicrosoftAppId;
  res.render('./views/auth-start', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/auth-end', function (req, res) {
  res.render('./views/auth-end');
});

// On-behalf-of token exchange
server.post('/getProfileOnBehalfOf', function (req, res) {
  var tid = req.body.tid;
  var token = req.body.token;
  var scopes = ["https://graph.microsoft.com/User.Read Mail.Read"];

  // Creating MSAL client
  const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: process.env.MicrosoftAppId,
      clientSecret: process.env.MicrosoftAppPassword
    }
  });

  var oboPromise = new Promise((resolve, reject) => {
    msalClient.acquireTokenOnBehalfOf({
      authority: `https://login.microsoftonline.com/${tid}`,
      oboAssertion: token,
      scopes: scopes,
      skipCache: true
    }).then(async result => {
      const client = new SimpleGraphClient(result.accessToken);
      const usermails = await client.getMailAsync();
      var userImage = await client.getUserPhoto()
      await userImage.arrayBuffer().then(result => {
        imageString = Buffer.from(result).toString('base64');
        img2 = "data:image/png;base64," + imageString;
        var userData = {
          details: usermails.value,
          image: img2
        }
        resolve(userData);
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

// Get user details.
server.post('/GetUserDetails', async (req, res) => {
  var accessToken = req.body.accessToken;
  const client = new SimpleGraphClient(accessToken);
  var userImage = await client.getUserPhoto();
  const usermails = await client.getMailAsync();
  var userData;
  await userImage.arrayBuffer().then(result => {
    imageString = Buffer.from(result).toString('base64');
    img2 = "data:image/png;base64," + imageString;
    userData = {
      image: img2,
      usermails:usermails.value
    }
  });
  var responseMessage = new Promise((resolve, reject) => {
      resolve(userData)
  });
  responseMessage.then(function (result) {
    res.json(result);
  }, function (err) { // Error: "It broke"
    res.json(err);
  });
});

server.post('/decodeToken',(req, res)=>{
  var token = req.body.accessToken;
  if(token !== null || token !== undefined){
    const base64String = token.split('.')[1];
    const decodedValue = JSON.parse(Buffer.from(base64String,    
      'base64').toString('ascii'));
      res.json(decodedValue);
    }
})