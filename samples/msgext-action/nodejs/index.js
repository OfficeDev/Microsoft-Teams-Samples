const { app, handleMessagingExtension } = require("./app");
const express = require('express');
const path = require('path');

require('dotenv').config({ path: './env/.env.local' });
require('dotenv').config({ path: './env/.env.local.user' });

const port = process.env.PORT || process.env.port || 3978;
const server = express();

// Configure EJS view engine
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', path.join(__dirname, 'views'));

// Add body parsing middleware
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

// Add static file serving
server.use("/Images", express.static(path.join(__dirname, 'Images')));

server.get('/customForm', (req, res) => {
  res.render('CustomForm');
});

server.get('/staticPage', (req, res) => {
  res.render('StaticPage');
});

const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
  console.error('Error:', error);
  await context.sendTraceActivity(
    'OnTurnError Trace',
    `${error}`,
    'https://www.botframework.com/schemas/error',
    'TurnError'
  );
};

const botHandler = async (context) => {
  const activity = context.activity;
  
  if (activity.type === 'invoke' && (activity.name === 'composeExtension/submitAction' || activity.name === 'composeExtension/fetchTask')) {
    try {
      const { handleMessagingExtension } = require('./app');
      const result = await handleMessagingExtension(context);
      
      if (result !== undefined) {
        await context.sendActivity({
          type: 'invokeResponse',
          value: {
            status: 200,
            body: result
          }
        });
        return;
      }
    } catch (error) {
      console.error('Error:', error.message);
      await context.sendActivity({
        type: 'invokeResponse',
        value: {
          status: 500,
          body: { error: error.message }
        }
      });
      return;
    }
  }
  
  // Handle regular messages
  if (activity.type === 'message') {
    const text = activity.text?.replace(/<at[^>]*>.*?<\/at>/g, '').trim();
    if (text) {
      if (text === '/reset') {
        await context.sendActivity('Conversation state reset.');
      } else if (text === '/runtime') {
        await context.sendActivity(`Node: ${process.version}, Teams AI v2`);
      } else {
        await context.sendActivity(`Echo: ${text}`);
      }
    }
  }
};

// Handle bot messages through the adapter
server.post('/api/messages', async (req, res) => {
  await adapter.process(req, res, botHandler);
});

(async () => {
  try {
    server.listen(port, () => {
      console.log(`Server listening on port ${port}`);
    });
  } catch (error) {
    console.error('Server startup error:', error);
    process.exit(1);
  }
})();
