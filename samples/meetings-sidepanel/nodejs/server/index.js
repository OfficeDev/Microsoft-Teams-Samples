// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot
// Import required pckages

const path = require('path');
const restify = require('restify');
var bodyParser = require('body-parser');
const socketio = require('socket.io')(8080);
var fs = require('file-system');
const cacheService = require('./Services/myCache');
Controller = require('./Controllers/HomeController')

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder')

const { SidePanelBot } = require('./bots/SidePanelBot');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '..', '.env');
require('dotenv').config({ path: ENV_FILE });

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
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
const bot = new SidePanelBot();

// Create HTTP server.
const server = restify.createServer();

var agendaPointsInitial = ["Approve 5% dividend payment to shareholders.", "Increase research budget by 10%.", "Continue with WFH for next 3 months."];
var agendaPoints = agendaPointsInitial;
cacheService.myCache.del("points");

cacheService.myCache.set("points", agendaPointsInitial, 36000);
agendaPoints = cacheService.myCache.get("points");
console.log(agendaPoints);

const port = 3000;
server.listen(port || process.env.PORT || 3000, function () {
    console.log(`\n${server.name} listening to ${port}`);
});

// // Listen for incoming requests.
// server.post('/api/messages', (req, res, next) => {
//     adapter.processActivity(req, res, async (context) => {
//         await bot.run(context);
//         return next();
//     });
// });

server.post('/api/messages', (req, res, next) => {
    // Route received a request to adapter for processing
    adapter.process(req, res, async (context) => await bot.run(context));
    return next();
});

server.use(bodyParser.json()); // for parsing application/json
server.use(bodyParser.urlencoded({ extended: true }));

server.post('/api/sendAgenda', Controller.getAgenda);
server.post('/api/setContext', Controller.setContext);

addNewPoint = (point) => {
    point ? agendaPoints.push(point) : null;
    cacheService.myCache.del("points");
    cacheService.myCache.set("points", agendaPoints, 36000);
    agendaPoints = cacheService.myCache.get("points");
    console.log(agendaPoints);
}



