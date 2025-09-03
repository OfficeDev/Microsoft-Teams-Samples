// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const express = require('express');

const { SimpleGraphClient } = require('./simpleGraphClient');

let multer = require('multer');
let upload = multer({ storage: multer.memoryStorage() });

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter, ConversationState, MemoryStorage, UserState, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');

const { tokenData } = require('./dialogs/mainDialog');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication({
    MicrosoftAppId: process.env.MicrosoftAppId,
    MicrosoftAppPassword: process.env.MicrosoftAppPassword
});

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
    
    // Clear out state
    await conversationState.delete(context);
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

// Add body parser middleware for CloudAdapter
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);

// Endpoint to fetch upload page for uploading files.
server.get('/Upload', (req, res, next) => {
    res.render('./views/Upload')
});

// Endpoint to listen to multiport/form-data requests.
server.post('/Save', upload.single('file'), async(req, res) => {
    try {
        if(req.file == null) {
            return res.status(400).json({ error: 'No file uploaded' });
        }

        var token = tokenData["token"];
        if (!token) {
            return res.status(401).json({ error: 'No authentication token available' });
        }

        var buffer = req.file.buffer;
        console.log('Creating GraphClient...');
        const client = new SimpleGraphClient(token);
        
        console.log('Getting site details...');
        const site = await client.getSiteDetails(process.env.SharePointTenantName, process.env.SharePointSiteName);

        if (site != null) {
            var drive = await client.getDriveDetails(site.id);

            if (drive != null && drive.value && drive.value.length > 0) {
                if (buffer != null) {
                    var uploadResult = await client.uploadFile(site.id, drive.value[0].id, buffer, req.file.originalname);
                    
                    if (uploadResult) {
                        res.json({ success: true, message: 'File uploaded successfully' });
                    } else {
                        res.status(500).json({ error: 'Failed to upload file' });
                    }
                } else {
                    res.status(400).json({ error: 'File buffer is empty' });
                }
            } else {
                res.status(404).json({ error: 'SharePoint drive not found' });
            }
        } else {
            res.status(404).json({ error: 'SharePoint site not found' });
        }
    } catch (error) {
        console.error('Upload error:', error);
        res.status(500).json({ error: 'Internal server error during upload' });
    }
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});