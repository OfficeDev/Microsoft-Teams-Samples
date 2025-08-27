// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages

const path = require('path');
const express = require('express');
const cors = require('cors');
const { SimpleGraphClient } = require('./simpleGraphClient');
const msal = require('@azure/msal-node');
const axios = require('axios')
const jwt = require('jsonwebtoken');

const spark =  require('@microsoft/teams.apps');



// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = 3977;
const server = express();
let multer = require('multer');
let upload = multer({ storage: multer.memoryStorage() });

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  createBotFrameworkAuthenticationFromConfiguration, BotFrameworkAdapter, ConversationState, MemoryStorage, UserState } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');
const { stat } = require('fs');

const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: process.env.MicrosoftAppId,
  MicrosoftAppPassword: process.env.MicrosoftAppPassword,
  MicrosoftAppType: process.env.AppType,
  MicrosoftAppTenantId: process.env.TenantId
});

const botFrameworkAuthentication = createBotFrameworkAuthenticationFromConfiguration(null, credentialsFactory);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

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

  // Note: Since this Messaging Extension does not have the messageTeamMembers permission
  // in the manifest, the bot will not be allowed to message users.
};

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

// Create the main dialog.
const dialog = new MainDialog();
// Create the bot that will handle incoming messages.
const bot = new TeamsBot(conversationState, userState, dialog);
var app;
// Create HTTP server.
server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
   app = new spark.App({
    clientId: process.env.MicrosoftAppId,
    clientSecret: process.env.MicrosoftAppPassword,
});
app.start();
});
const globalMap = {};
let code;
server.get('/api/setAuthToken', (req, res) => {
  console.log(req.query);
   code = Date.now()
  globalMap[`${code}`] = req.query.accessToken;
  res.cookie('authToken', req.query.accessToken, {maxAge: 900000, sameSite: 'None', secure: true});
  res.send(200);
  //res.redirect(`/Auth0Success?code=${code}`);
});

server.get('/api/authorize', (req, res) => {
  console.log(req.query);

  res.redirect(`${req.query.redirect_uri}?code=${code}&state=${req.query.state}&scope=${req.query.scope}`);
  // globalMap[`${code}`] = null; // Clear the token after redirect
  // code = null; // Reset the code
});

server.post('/api/token', (req, res) => {
  console.log(req.body);
  res.json({
    access_token: globalMap[`${req.body.code}`]
  });
  globalMap[`${code}`] = null; // Clear the token after redirect
  code = null; // Reset the code
});

server.post('/api/linkAccounts', async (req, res) => {
  console.log(req.body);
  const naaBasedToken = req.body.naaAuth0Payload.access_token;
  const naaTokenDecoded = jwt.decode(naaBasedToken, { complete: true });
  console.log('NAA BASED TOKEN', naaBasedToken, naaTokenDecoded);
  try {
  const responseBot = await app.api.users.token.get({
   userId: '29:1QCvxM3V0UEmyvan41zkATQ4F8PgnH1s1A_Hvl7ISyui7k0OF3ukIGGpSI8FCn0xbMp-WQQt5ai0bx8VxvFL8zA',
   connectionName: "Auth0",
});
const primaryTokenDecoded = jwt.decode(responseBot.token, { complete: true });
console.log('Primary account token', responseBot, primaryTokenDecoded);
          var linkOptions = {
            method: 'POST',
            url: `https://dev-zvtwcdg4lg5a82cq.us.auth0.com/api/v2/users/${primaryTokenDecoded.payload.sub}/identities`,
            headers: {authorization: `Bearer ${responseBot.token}`},
            data:  {
                                        
                // "provider": "oauth2",
                // "user_id": "Microsoftentracustom|af53eca6-db6f-4085-8436-b9d2cb446ea3"

                "link_with": req.body.naaAuth0Payload.id_token
            }
            
            };
            axios.request(linkOptions).then(function (response) {
                console.log('linked ', {response});
                  res.send(200);

            }).catch((err) => {
                console.log('link error', err);
                  res.send(400);

            });
  } catch (error) {
    console.error("Error linking accounts:", error);
                      res.send(400);

  }

});

server.get('/api/testAuthToken', (req, res) => {
  console.log(req.cookies);
  res.send(200);
});

// Endpoint to fetch Auth tab page.
server.get('/AuthTab', (req, res, next) => {
  var clientId = process.env.FaceBookAppId;

  res.render('./views/AuthTab', { clientId: clientId, })
});

server.get('/public/entra-bundle.js', (req, res, next) => {
  const jsPath = path.join(__dirname, '/public/entra-bundle.js');
  console.log({jsPath})

  res.sendFile(jsPath, {headers: {'Content-Type' : 'text/javascript'}})
});

server.get('/Auth0Success', (req, res, next) => {
  var clientId = process.env.FaceBookAppId;
  res.render('./views/Auth0Success', { clientId: clientId })
});

server.get('/entraAuthorize', (req, res, next) => {
  //var clientId = process.env.FaceBookAppId;
  res.render('./views/EntraAuthorize', { clientId: 'entra' })
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
server.get('/auth-start', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/auth-start', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/auth-end', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/auth-end', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/silent-tab', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/silent-tab', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/silent-start', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/silent-start', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/silent-end', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/silent-end', { clientId: JSON.stringify(clientId) });
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
  // Route received a request to adapter for processing
  await adapter.process(req, res, (context) => {
    return bot.run(context);
});
});

// On-behalf-of token exchange
server.post('/getProfileOnBehalfOf', function (req, res) {
  var tid = process.env.TenantId;
  var token = req.body.idToken;
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
      
      try {
        var userImage = await client.getUserPhoto()
        await userImage.arrayBuffer().then(result => {
          console.log(userImage.type);
          imageString = Buffer.from(result).toString('base64');
          img2 = "data:image/png;base64," + imageString;
          var userData = {
            details: myDetails,
            image: img2
          }
          resolve(myDetails);
        });
      }
      catch (error) {
        console.log(error);
      }
      
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

// Listen for incoming requests.
server.post('/decodedToken', async (req, res) => {
  var idToken = req.body.idToken;
  const decodedToken = jwt.decode(idToken, { complete: true });
    var responseMessage = Promise.resolve(decodedToken.payload);

    responseMessage.then(function (result) {
      res.json(result);
      console.log(result);
    }, function (err) {
      console.log(err); // Error: "It broke"
      res.json(err);
    });
});

// Listen for incoming requests.
server.post('/GetUserDetails', async (req, res) => {
  var accessToken = req.body.accessToken;
  const client = new SimpleGraphClient(accessToken);
  const myDetails = await client.getMeAsync();
    var responseMessage = Promise.resolve(myDetails);
    responseMessage.then(function (result) {
      res.json(result);
      console.log(result);
    }, function (err) {
      console.log(err); // Error: "It broke"
      res.json(err);
    });
  });