// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const msal = require('@azure/msal-node');
const express = require('express');
const cors = require('cors');
const { SimpleGraphClient } = require('./simpleGraphClient');
const { polyfills } = require('isomorphic-fetch');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();

const tid = process.env.TenantId;

var scopes = ["https://graph.microsoft.com/User.Read"];

const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: process.env.MicrosoftAppId,
      clientSecret: process.env.MicrosoftAppPassword
    }
  });

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
  var scope = req.url.split('=')[1];
  var data = {
    clientId:process.env.MicrosoftAppId,
    scope:scope
  };
  res.render('./views/auth-start', { data: JSON.stringify(data) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/auth-end', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/auth-end', { clientId: JSON.stringify(clientId) });
});

// Exchange the id token with access token
const getDelegateAccessToken = async (tid, token) => {
    
  try {
  var result = await msalClient.acquireTokenOnBehalfOf({
      authority: `https://login.microsoftonline.com/${tid}`,
      oboAssertion: token,
      scopes: scopes,
      skipCache: true
    });
  }catch(error){
      console.log("Error occured"+error);
  }

  return result.accessToken;
}

// Get user details.
server.post('/GetUserPhoto', async (req, res) => {
  var idToken = req.body.idToken;
  var accesstoken = await getDelegateAccessToken(tid, idToken);
  const client = new SimpleGraphClient(accesstoken);
  var userImage = await client.getUserPhoto();
  var userData;
  await userImage.arrayBuffer().then(result => {
    imageString = Buffer.from(result).toString('base64');
    img2 = "data:image/png;base64," + imageString;
    userData = {
      image: img2,
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

// Get user mails.
server.post('/GetUserMails', async (req, res) => {
  var idToken = req.body.idToken;
  var accesstoken = await getDelegateAccessToken(tid, idToken);
  const client = new SimpleGraphClient(accesstoken);
  const usermails = await client.getMailAsync();
    var responseMessage = Promise.resolve(usermails.value);
    responseMessage.then(function (result) {
      res.json(result);
    }, function (err) {
      console.log(err); // Error: "It broke"
      res.json(err);
    });
});

server.post('/decodeToken',(req, res)=>{
  var token = req.body.idToken;
  if(token !== null || token !== undefined){
    const base64String = token.split('.')[1];
    const decodedValue = JSON.parse(Buffer.from(base64String,    
      'base64').toString('ascii'));
      res.json(decodedValue);
    }
})