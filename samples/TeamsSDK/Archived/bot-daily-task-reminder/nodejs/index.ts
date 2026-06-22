import path from 'path';
import dotenv from 'dotenv';
dotenv.config({ path: path.join(__dirname, '.env') });

import express from 'express';
import { CloudAdapter, ConfigurationBotFrameworkAuthentication } from 'botbuilder';
import { TeamsBot } from './bots/teamsBot';

const PORT = process.env.PORT || 3978;
const server = express();

server.use(express.json());
server.use(express.urlencoded({ extended: true }));

server.set('view engine', 'ejs');
server.set('views', path.join(__dirname, 'views'));

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
    process.env as Record<string, string>
);
const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
};

const bot = new TeamsBot();

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

server.use('/Images', express.static(path.resolve(__dirname, 'Images')));

server.get('/scheduleTask', (_req, res) => {
    res.render('ScheduleTask');
});

server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});
