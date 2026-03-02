// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
    CardFactory
} = require('botbuilder');
const { BotActivityHandler } = require('./bot/botActivityHandler');
const candidateHandler = require('./data/candidate')
const questionsHandler = require('./data/questions')
const notesHandler = require('./data/notes')
const feedbackHandler = require('./data/feedback')
const { ConversationRef } = require('./bot/botActivityHandler');
const cardHelper = require('./cards/cardHelper')
const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

// Create bot handlers
const botActivityHandler = new BotActivityHandler();

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. server insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       serverlication insights.
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

// Listen for incoming activities and route them to your bot main dialog.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
});



server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.get('/api/candidate', async (req, res) => {
    candidateHandler.getCandidateDetails(function(response) {
        res.send(response);
    });
});

server.post('/api/Question/insertQuest', (req, res) => {
    questionsHandler.saveQuestions(req.body, function(response) {
        res.send(response);
    });
});

server.get('/api/Question', (req, res) => {
    const meetingId = req.query.meetingId;
    questionsHandler.getQuestions(meetingId, function (response) {
        res.send(response);
    });
});

server.post('/api/Question/edit', (req, res) => {
    questionsHandler.editQuestion(req.body, function(response) {
        res.send(response);
    });
});

server.post('/api/Question/delete', (req, res) => {
    questionsHandler.deleteQuestion(req.body, function(response) {
        res.send(response);
    });
});

server.post('/api/Feedback', (req, res) => {
    feedbackHandler.saveFeedback(req.body, function(response) {
        res.send(response);
    });
});

server.get('/api/Notes', (req, res) => {
    const email = req.query.email;
    notesHandler.getNotes(email, function (response) {
        res.send(response);
    });
});

server.post('/api/Notes', (req, res) => {
    notesHandler.addNote(req.body, function(response) {
        res.send(response);
    });
});

server.get('/api/Candidate/file', (req, res) => {
    let filePath = path.join(__dirname, "./files/test.pdf");
    res.download(filePath);
});

server.post('/api/Notify', async (req, res) => {
    for (const conversationReference of Object.values(ConversationRef)) {
        const botAppId = process.env.MicrosoftAppId || process.env.AAD_APP_CLIENT_ID || '';
        await adapter.continueConversationAsync(botAppId, conversationReference, async turnContext => {
            var actions = new Array();
            req.body.files.map((file) => {
                actions.push({
                    type: "Action.OpenUrl",
                    title: file,
                    url: process.env.BlobUrl +"/" +file
                });
            })
            const userCard = CardFactory.adaptiveCard(cardHelper.getCardForMessage(req.body.message, actions));
            await turnContext.sendActivity({ attachments: [userCard] });
        });
    }
    res.setHeader('Content-Type', 'text/html');
    res.writeHead(200);
    res.write('<html><body><h1>Proactive messages have been sent.</h1></body></html>');
    res.end();
});

getCardForMessage = (message, actions) => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: message
        }
    ],
    actions: actions,
    type: 'AdaptiveCard',
    version: '1.4'
});

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});


