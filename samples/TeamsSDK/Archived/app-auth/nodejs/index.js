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

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
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

// Create HTTP server.
server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});

// Endpoint to fetch Auth tab page
server.get('/AuthTab', (req, res) => {
  const clientId = process.env.FaceBookAppId;
  res.render('./views/AuthTab', { clientId: clientId });
});

// Auth flow start - initiates AAD authentication
server.get('/auth-start', (req, res) => {
  const clientId = process.env.MicrosoftAppId;
  res.render('./views/auth-start', { clientId: JSON.stringify(clientId) });
});

// Auth flow completion - returns authentication results
server.get('/auth-end', (req, res) => {
  const clientId = process.env.MicrosoftAppId;
  res.render('./views/auth-end', { clientId: JSON.stringify(clientId) });
});

// Silent authentication tab
server.get('/silent-tab', (req, res) => {
  const clientId = process.env.MicrosoftAppId;
  res.render('./views/silent-tab', { clientId: JSON.stringify(clientId) });
});

// Silent auth flow start
server.get('/silent-start', (req, res) => {
  const clientId = process.env.MicrosoftAppId;
  res.render('./views/silent-start', { clientId: JSON.stringify(clientId) });
});

// Silent auth flow completion
server.get('/silent-end', (req, res) => {
  const clientId = process.env.MicrosoftAppId;
  res.render('./views/silent-end', { clientId: JSON.stringify(clientId) });
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
  // Route received a request to adapter for processing
  await adapter.process(req, res, (context) => bot.run(context));
});

// On-behalf-of token exchange
server.post('/getProfileOnBehalfOf', async function (req, res) {
  try {
    const tid = process.env.TenantId;
    const token = req.body.idToken;
    const scopes = ["https://graph.microsoft.com/User.Read", "openid"];

    // Creating MSAL client
    const msalClient = new msal.ConfidentialClientApplication({
      auth: {
        clientId: process.env.MicrosoftAppId,
        clientSecret: process.env.MicrosoftAppPassword
      }
    });

    const result = await msalClient.acquireTokenOnBehalfOf({
      authority: `https://login.microsoftonline.com/${tid}`,
      oboAssertion: token,
      scopes: scopes,
      skipCache: true
    });

    const client = new SimpleGraphClient(result.accessToken);
    const myDetails = await client.getMeAsync();
    res.json(myDetails);
  } catch (error) {
    console.error('Error in /getProfileOnBehalfOf:', error);
    res.status(500).json({ error: error.message || 'Failed to get profile' });
  }
});

// Listen for decoded token endpoint
server.post('/decodedToken', async (req, res) => {
  try {
    const idToken = req.body.idToken;
    const decodedToken = jwt.decode(idToken, { complete: true });
    res.json(decodedToken.payload);
  } catch (error) {
    console.error('Error in /decodedToken:', error);
    res.status(400).json({ error: error.message || 'Failed to decode token' });
  }
});

// Listen for user details endpoint
server.post('/GetUserDetails', async (req, res) => {
  try {
    const accessToken = req.body.accessToken;
    const client = new SimpleGraphClient(accessToken);
    const myDetails = await client.getMeAsync();
    res.json(myDetails);
  } catch (error) {
    console.error('Error in /GetUserDetails:', error);
    res.status(500).json({ error: error.message || 'Failed to get user details' });
  }
});