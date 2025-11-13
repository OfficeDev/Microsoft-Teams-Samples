// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');

const fs = require('fs');
const dotenv = require('dotenv');
// Import required configuration. Load root .env then optional env/.env.local produced by toolkit.
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });
const LOCAL_ENV_FILE = path.join(__dirname, 'env', '.env.local');
if (fs.existsSync(LOCAL_ENV_FILE)) {
    dotenv.config({ path: LOCAL_ENV_FILE });
    console.log(`[startup] Loaded additional environment from env/.env.local`);
} else {
    console.log('[startup] No env/.env.local found (optional).');
}

const PORT = process.env.PORT || 3978;
const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');

// Import required Agents SDK services.
// See https://learn.microsoft.com/en-us/microsoft-365/agents-sdk/ to learn more about the Agents SDK.
const {
    CloudAdapter,
    loadAuthConfigFromEnv,
    authorizeJWT
} = require('@microsoft/agents-hosting');

// Load auth config (expects MICROSOFT_APP_ID / MICROSOFT_APP_PASSWORD). Provide fallbacks for legacy variable names.
const rawAuthConfig = loadAuthConfigFromEnv();
// Fallback mapping: some older samples use MicrosoftAppId or AAD_APP_CLIENT_ID.
rawAuthConfig.appId = process.env.MICROSOFT_APP_ID || process.env.MicrosoftAppId || process.env.AAD_APP_CLIENT_ID || rawAuthConfig.appId;
rawAuthConfig.appPassword = process.env.MICROSOFT_APP_PASSWORD || process.env.MicrosoftAppPassword || rawAuthConfig.appPassword;
const authConfig = rawAuthConfig;
if (!authConfig.appId) {
    console.warn('[startup] Missing MICROSOFT_APP_ID (or fallback). Bot will not authenticate with Channels. Set MICROSOFT_APP_ID in your env.');
}
if (!authConfig.appPassword) {
    console.warn('[startup] Missing MICROSOFT_APP_PASSWORD (client secret). For local emulator testing you can proceed, but Teams channel will reject calls.');
}
// This bot's main dialog.
//const { DeepLinksTest } = require('./bot');
const DeepLinkTabsnode = require('./Bots/DeepLinkTabsnode');

// Create HTTP server
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
// Global request logger for diagnostics
server.use((req, _res, next) => {
    if (req.path === '/api/messages') {
        console.log(`[http] ${req.method} ${req.url} headers.authorization=${req.headers.authorization ? 'present' : 'missing'}`);
    }
    next();
});

// Add JWT authorization middleware for Agents SDK only if credentials present (avoid blocking unauthenticated dev traffic).
if (process.env.AGENTS_DISABLE_AUTH === '1') {
    console.log('[startup] JWT authorization disabled via AGENTS_DISABLE_AUTH=1');
} else if (authConfig.appId && authConfig.appPassword) {
    // NOTE: Removed authorizeJWT because Teams channel incoming activities use Bot Service tokens
    // not Agents JWT. Leaving endpoint unauthenticated for local dev. TODO: Replace with appropriate
    // channel validation once Agents SDK provides Teams/BotService compatibility middleware.
    console.log('[startup] JWT middleware skipped intentionally for /api/messages to allow Bot Service activities.');
} else {
    console.log('[startup] JWT authorization skipped (missing credentials). Set MICROSOFT_APP_ID / MICROSOFT_APP_PASSWORD or AGENTS_DISABLE_AUTH=1 for local debugging.');
}

server.listen(PORT, () => {
    console.log('Server listening on port: ' + PORT);
}).on('error', (err) => {
    console.error('Server failed to start:', err);
    process.exit(1);
});

// Create adapter.
// See https://learn.microsoft.com/en-us/microsoft-365/agents-sdk/ to learn more about the Agents SDK.
const adapter = new CloudAdapter(authConfig);
// Instrument adapter.process for deeper diagnostics.
const originalProcess = adapter.process.bind(adapter);
adapter.process = async (req, res, logic) => {
    console.log('[adapter] process(): starting');
    try {
        await originalProcess(req, res, async (context) => {
            console.log('[adapter] process(): turn logic invoked');
            await logic(context);
        });
        console.log('[adapter] process(): completed statusCode=', res.statusCode);
    } catch (e) {
        console.error('[adapter] process(): error', e);
        throw e;
    }
};

// Catch-all for errors.
const onTurnErrorHandler = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights for telemetry.
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

// Set the onTurnError for the CloudAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Create the main dialog.
const myBot = new DeepLinkTabsnode();

// Listen for incoming requests.
// Route-specific raw body capture & detailed diagnostics
server.post('/api/messages',
    bodyParser.json({ verify: (req, _res, buf) => { req.rawBody = buf.toString(); } }),
    async (req, res) => {
        console.log('[http] Incoming activity POST /api/messages');
        console.log('[http] request headers (subset):', {
            authorization: req.headers.authorization ? 'present' : 'missing',
            contentType: req.headers['content-type'],
            contentLength: req.headers['content-length']
        });
        console.log('[http] rawBody length:', req.rawBody?.length || 0);
        console.log('[http] parsed body keys:', Object.keys(req.body || {}));
        try {
            await adapter.process(req, res, async (context) => {
                console.log(`[turn] Activity received type=${context.activity.type} text="${context.activity.text || ''}" conversationType=${context.activity.conversation?.conversationType}`);
                await myBot.run(context);
            });
            console.log('[http] adapter.process completed statusCode=', res.statusCode);
        } catch (err) {
            console.error('[http] Error processing activity:', err);
        }
    });

// Listen for Upgrade requests for Streaming.
server.on('upgrade', (req, socket, head) => {
    // Create an adapter scoped to this WebSocket connection to allow storing session data.
    const streamingAdapter = new CloudAdapter(authConfig);
    // Set onTurnError for the CloudAdapter created for each connection.
    streamingAdapter.onTurnError = onTurnErrorHandler;

    streamingAdapter.useWebSocket(req, socket, head, async (context) => {
        // After connecting via WebSocket, run this logic for every request sent over
        // the WebSocket connection.
        await myBot.run(context);
    });
});

server.get('/api/getAppId', (req, res) => {
    // Expose resolved appId for front-end deep link helpers.
    res.send({ microsoftAppId: authConfig.appId });
});


// Static Middleware
server.use(express.static(path.join(__dirname, './pages')));