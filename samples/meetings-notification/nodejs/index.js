// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required pckages
const path = require('path');
// const {TaskmoduleIds} =require('./models/taskmoduleids');

// console.log(TaskmoduleIds);
// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const restify = require('restify');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');
const { MeetingNotificationBot } = require('./bots/meetingNotificationBot');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

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
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`\n${ server.name } listening to ${ server.url }`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator select "Open Bot"');
});

 // Listen for incoming requests.
server.post('/api/messages', (req, res) =>{
    adapter.processActivity(req, res, async (context) =>{
        await bot.run(context);
    });
});

const{contentBubbleTitles}=require('./models/contentbubbleTitle')
var bodyParser = require("body-parser"); 
server.set("view engine", "ejs"); 
server.set("views", __dirname + "/views");
server.engine('html',require('ejs').renderFile); 
server.use(bodyParser.urlencoded({ extended:false}));
server.get('/', (req,res)=>{res.render('index.html',{question: contentBubbleTitles.contentQuestion})});