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

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

let multer = require('multer');
let upload = multer({ storage: multer.memoryStorage() });

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  createBotFrameworkAuthenticationFromConfiguration, ConversationState, MemoryStorage, UserState } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');

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
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${ error }`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

     // Uncomment below commented line for local debugging.
     // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);    
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

// Create HTTP server.
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));

server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);

// Endpoint to fetch Auth tab page.
server.get('/AuthTab', (req, res, next) => {
  var clientId = process.env.FaceBookAppId;
  res.render('./views/AuthTab', { clientId: clientId })
});

// Endpoint to fetch Signin popup page.
server.get('/popUpSignin', function (req, res) {
  res.render('./views/popUpSignin');
});

server.get('/config', function (req, res) {
  res.render('./views/config');
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

// Endpoint to facebook auth redirect page.
server.get('/fb-auth', function (req, res) {
  res.render('./views/fb-auth');
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
  // Route received a request to adapter for processing
  await adapter.process(req, res, (context) => bot.run(context));
});

// On-behalf-of token exchange
server.post('/getProfileOnBehalfOf', function (req, res) {
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
          resolve(userData);
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
server.post('/GetUserDetails', async (req, res) => {
  var accessToken = req.body.accessToken;
  const client = new SimpleGraphClient(accessToken);
  const myDetails = await client.getMeAsync();
  var userImage = await client.getUserPhoto()
  await userImage.arrayBuffer().then(result => {
    console.log(userImage.type);
    imageString = Buffer.from(result).toString('base64');
    img2 = "data:image/png;base64," + imageString;
    var userData = {
      details: myDetails,
      image: img2
    }
    var responseMessage = Promise.resolve(userData);
    responseMessage.then(function (result) {
      res.json(result);
      console.log(result);
    }, function (err) {
      console.log(err); // Error: "It broke"
      res.json(err);
    });
  });
});

server.post('/tabCredentialsAuth', function (req, res) {
  var userNAme = req.body.userName;
  var password = req.body.password;
  var resultResponse;
  if (userNAme == "testaccount@test123.onmicrosoft.com" && password == "testpassword") {
    resultResponse = "Authentication successfull";
  }
  else {
    resultResponse = "Authentication failed";
  }

  var responseMessage = Promise.resolve(resultResponse);
  responseMessage.then(function (result) {
    res.json(result);
    console.log(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});

// Facebook Oauth token axchange
server.post('/getFbAccessToken', function (req, res) {
  var token = req.body.token;
  var accessToken;
  var scopes = ['name', 'picture'].join(',');

  var fbPromise = new Promise((resolve, reject) => {
    axios.get('https://graph.facebook.com/v12.0/oauth/access_token', {
      params: {
        client_id: process.env.FaceBookAppId,
        redirect_uri: process.env.ApplicationBaseUrl + '/fb-auth',
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

  fbPromise.then(function (result) {
    res.json(result);
    console.log(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});