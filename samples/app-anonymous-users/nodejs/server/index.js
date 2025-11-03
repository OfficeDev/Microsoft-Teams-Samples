// <copyright file="index.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// index.js is used to setup and configure your bot
// </copyright>

const path = require('path');
const cors = require('cors');
// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3000;
const express = require('express');
const app = express();
// Create HTTP server.
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({
  extended: true
}));

const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });
const { SimpleGraphClient } = require('../server/simpleGraphClient');
const axios = require('axios')
const msal = require('@azure/msal-node');
const { polyfills } = require('isomorphic-fetch');

io.on('connection', (socket) => {
   // receive a message from the client
  socket.on('message', msg => {
    // send a message to the client
    io.emit('message', msg);
  });
});

// Get facebook profile of user.
app.post('/getFacebookLoginUserInfo', function (req, res) {
  var token = req.body.token;
  var accessToken;
  var scopes = ['name','picture'].join(',');

  var facebookAuthPromise = new Promise((resolve, reject) => {
    axios.get('https://graph.facebook.com/v12.0/oauth/access_token', {
      params: {
        client_id: process.env.FaceBookAppId,
        redirect_uri: process.env.ApplicationBaseUrl + '/facebook-auth-end',
        client_secret: process.env.FaceBookAppPassword,
        code: token,
      }
    }).then(response => {
      console.log(response);
      accessToken = response.data.access_token;

      axios.get('https://graph.facebook.com/v2.6/me', {
        params: {
          fields: scopes,
          access_token: accessToken,
        }
      }).then(profile => {
        console.log(profile);
        resolve(profile.data);
      })
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  facebookAuthPromise.then(function (result) {
    res.json(result);
    console.log(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});

// Get user access token
app.post('/GetLoginUserInformation', function (req, res) {
  var tid = req.body.tid;
  var token = req.body.token;
  var scopes = ["https://graph.microsoft.com/User.Read", "openid"];

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
      
      const myDetails = await client.getMeAsync();
      var userData = {
        details: myDetails
      }
      resolve(userData);
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

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter,
  ConfigurationServiceClientCredentialFactory, createBotFrameworkAuthenticationFromConfiguration } = require('botbuilder');

const { TeamsConversationBot } = require('../bots/teamsConversationBot');

const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppType: process.env.MicrosoftAppType,
  MicrosoftAppId: process.env.MicrosoftAppId,
  MicrosoftAppPassword: process.env.MicrosoftAppPassword,
  MicrosoftAppTenantId: process.env.MicrosoftAppTenantId
});

const botFrameworkAuthentication = createBotFrameworkAuthenticationFromConfiguration(null, credentialsFactory);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
adapter.onTurnError = async (context, error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights. See https://aka.ms/bottelemetry for telemetry 
  //       configuration instructions.

  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Send a trace activity, which will be displayed in Bot Framework Emulator
  await context.sendTraceActivity(
      'OnTurnError Trace',
      `${error}`,
      'https://www.botframework.com/schemas/error',
      'TurnError'
  );

  // Uncomment below commented line for local debugging.
  // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Create the bot that will handle incoming messages.
const bot = new TeamsConversationBot();

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

// Listen for incoming activities and route them to your bot handler.
app.post('/api/messages', async (req, res) => {
   // Route received a request to adapter for processing
  await adapter.process(req, res, (context) => bot.run(context));
});