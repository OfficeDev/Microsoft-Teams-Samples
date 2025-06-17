require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const path = require('path');
const authRouter = require('./controllers/AuthController');

const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const { TeamsConversationBot } = require('./bots/teamsConversationBot');

const app = express();

// Use body parsing middleware
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// Serve static files (e.g., auth-end.html)
app.use('/public', express.static(path.join(__dirname, 'public')));

// Use the auth router
app.use('/api/auth', authRouter);

// Create Bot Framework Adapter
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling for the bot adapter
adapter.onTurnError = async (context, error) => {
    console.error(`[onTurnError] unhandled error: ${error}`);

    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    await context.sendActivity('Sorry, it looks like something went wrong.');
};

const bot = new TeamsConversationBot();

// Listen for incoming messages
app.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});

// Start the server
const port = process.env.PORT || 3978;
app.listen(port, () => {
    console.log(`Server is running on http://localhost:${port}`);
});