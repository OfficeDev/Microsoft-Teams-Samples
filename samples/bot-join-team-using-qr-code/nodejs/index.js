const path = require('path');
const express = require('express');
const cors = require('cors');
const QRCode = require('qrcode');

require('dotenv').config({ path: path.join(__dirname, '.env') });

const { CloudAdapter, ConfigurationBotFrameworkAuthentication, MemoryStorage, ConversationState, UserState } = require('botbuilder');

const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');
const { teamData } = require('./bots/dialogBot');

const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

// ------------------ Adapter Setup (CloudAdapter) ------------------
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// ------------------ Error Handling ------------------
const memoryStorage = new MemoryStorage();
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

adapter.onTurnError = async (context, error) => {
    console.error(`[onTurnError] unhandled error: ${error}`);

    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    await context.sendActivity('The bot encountered an error or bug.');
    await conversationState.delete(context);
};

// ------------------ Bot Setup ------------------
const dialog = new MainDialog(conversationState);
const bot = new TeamsBot(conversationState, userState, dialog);

// ------------------ Start Server ------------------
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

// ------------------ Web Routes ------------------
server.get('/generate', (req, res) => {
    res.render('./views/generate');
});

server.get('/qrcode', async (req, res) => {
    const teamId = req.query.teamId;
    if (!teamId) return res.status(400).send('Missing teamId query parameter');

    try {
        const qrData = await QRCode.toDataURL(teamId);
        res.render('./views/qrcode', { qrData: JSON.stringify(qrData) });
    } catch (err) {
        console.error(err);
        res.status(500).send('Error generating QR code');
    }
});

server.get('/teamDetails', (req, res) => {
    const teamDetails = teamData["data"];
    res.render('./views/generate', { teamDetails: JSON.stringify(teamDetails) });
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

const { ConnectorClient } = require('botframework-connector');

// ------------------ Bot Message Endpoint ------------------
// Inside your adapter.process call:
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, async (context) => {
        // ✅ Get the ConnectorClient and UserTokenClient from the adapter's turnState
        const connectorClient = context.turnState.get(adapter.ConnectorClientKey);
        const userTokenClient = context.turnState.get(adapter.UserTokenClientKey);

        context.turnState.set('UserTokenClient', userTokenClient);
        context.turnState.set('ConnectorClient', connectorClient);

        await bot.run(context);
    });
});