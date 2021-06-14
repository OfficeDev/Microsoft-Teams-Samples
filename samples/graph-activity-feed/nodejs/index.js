const path = require('path');
const dotenv = require('dotenv');
const pug = require("pug");
const fs = require("fs");


// Import required bot configuration.
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

const restify = require('restify');
const render = require('restify-render-middleware');

const { TaskDTO } = require("./models/TaskViewModel");
const ActivityService = require("./bin/activity-service");

// Crete Http server
const server = restify.createServer();
server.use(render({
    engine: {
        name: 'pug',
        extname: 'pug'
    },
    dir: path.join(__dirname, "views")
}));
server.use(restify.plugins.bodyParser({
    requestBodyOnGet: true
}));
server.use(restify.plugins.queryParser());
server.get('/public/*', restify.plugins.serveStatic({
    directory: __dirname
}))

server.listen(process.env.port || 3978, () => {
    console.log(`\n ${server.name} listening to ${server.url}`);
    console.log('\nTo talk to your bot, open the emulator select Open Bot');
});

// bot related task
// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');
// This bot's main dialog
const { EchoBot } = require('./bots/echo-bot');


const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

// Catch-all for errors.
const onTurnErrorHandler = async (context, error) => {
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

    // Send a message to the user
    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

adapter.onTurnError = onTurnErrorHandler;
const myBot = new EchoBot();

/* end-points start */

server.get('/', async (req, res) => {
    return res.send("Home-Page");
});
server.get('/configure', (req, res) => {
    return res.render('index');
});


let tasks = [
    new TaskDTO('task 1', 'no desc', 1),
    new TaskDTO('task 3', 'no desc', 2),
    new TaskDTO('task 2', 'no desc', 3),
];

server.get('/tab', (req, res) => {
    return res.render('tab', { tasks });
});

server.post('/notificatio-to-channel', async (req, res) => {

    const _as = new ActivityService();
    let ct = await _as.sendNotificationToUser(req.body.task);
    if (ct.status) {
        let _o = new TaskDTO();
        let o = { ..._o, ...req.body.task }
        tasks.push(o);
    }

    return res.json(ct);
});

// Listen for incoming requests.
// for bot
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Route to main dialog.
        await myBot.run(context);
    });
});

/* end-points end */

server.on('upgrade', (req, socket, head) => {
    // Create an adapter scoped to this WebSocket connection to allow storing session data.
    const streamingAdapter = new BotFrameworkAdapter({
        appId: process.env.MicrosoftAppId,
        appPassword: process.env.MicrosoftAppPassword
    });
    streamingAdapter.onTurnError = onTurnErrorHandler;

    streamingAdapter.useWebSocket(req, socket, head, async (context) => {
        // After connecting via WebSocket, run this logic for every request sent over
        // the WebSocket connection.
        await myBot.run(context);
    });
});
