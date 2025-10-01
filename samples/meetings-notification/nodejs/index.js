// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required pckages
const path = require('path');
// const {TaskmoduleIds} =require('./models/taskmoduleids');

// console.log(TaskmoduleIds);
// Read botFilePath and botFileSecret from .env file.


const restify = require('restify');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');
const { MeetingNotificationBot } = require('./bots/meetingNotificationBot');

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry 
    //       configuration instructions.
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

// Create the bot that will handle incoming messages.
const bot = new MeetingNotificationBot();

// Create HTTP server.
var server = restify.createServer();
server= require("express")();
server.use(require("express").json());
const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => 
    console.log(`App service listening at http://localhost:${port}`)
);

 // Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => bot.run(context));
});

const{contentBubbleTitles}=require('./models/contentbubbleTitle')
var bodyParser = require("body-parser"); 
server.set("view engine", "ejs"); 
server.set("views", __dirname + "/views");
server.engine('html',require('ejs').renderFile); 
server.use(bodyParser.urlencoded({ extended:false}));
server.get('/', (req,res)=>{res.render('index.html',{question: contentBubbleTitles.contentQuestion})});