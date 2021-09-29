const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

const candidateHandler = require('./data/candidate')
const questionsHandler = require('./data/questions')
const notesHandler = require('./data/notes')
const feedbackHandler = require('./data/feedback')
const { AdapterRef } = require('./api/botController');
const { ConversationRef } = require('./bot/botActivityHandler');
const { CardFactory } = require('botbuilder');
const cardHelper = require('./cards/cardHelper')

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.use('/api', require('./api'));

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
        await AdapterRef.continueConversation(conversationReference, async turnContext => {
            await turnContext.sendActivity({ attachments: [CardFactory.adaptiveCard(cardHelper.getCardForMessage(req.body.message))] });
        });
    }
    res.setHeader('Content-Type', 'text/html');
    res.writeHead(200);
    res.write('<html><body><h1>Proactive messages have been sent.</h1></body></html>');
    res.end();
});

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});


