// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const dotenv = require('dotenv');
const path = require('path');
const restify = require('restify');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    ActivityTypes,
    CloudAdapter,
    ConfigurationServiceClientCredentialFactory,
    InputHints,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');
const {
    allowedCallersClaimsValidator,
    AuthenticationConfiguration,
    AuthenticationConstants
} = require('botframework-connector');

// Load environment variables from .env file
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

// Import the bot's main dialog
const { AIBot } = require('./bot');

// Create HTTP server using restify
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 39784, () => {
    console.log(`\n${ server.name } listening to ${ server.url }`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator and select "Open Bot".');
});

// Serve manifest files
server.get('/manifest/*', restify.plugins.serveStatic({ directory: './manifest', appendRequestPath: false }));

// Parse allowed callers from environment variable
const allowedCallers = (process.env.AllowedCallers || '').split(',').filter((val) => val) || [];

// Create claims validator
const claimsValidators = allowedCallersClaimsValidator(allowedCallers);

// Configure valid token issuers for authentication
let validTokenIssuers = [];
const { MicrosoftAppTenantId } = process.env;

if (MicrosoftAppTenantId) {
    // For SingleTenant/MSI auth, the JWT tokens will be issued from the bot's home tenant.
    // Add these issuers to the list of valid token issuers.
    validTokenIssuers = [
        `${ AuthenticationConstants.ValidTokenIssuerUrlTemplateV1 }${ MicrosoftAppTenantId }/`,
        `${ AuthenticationConstants.ValidTokenIssuerUrlTemplateV2 }${ MicrosoftAppTenantId }/v2.0/`,
        `${ AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV1 }${ MicrosoftAppTenantId }/`,
        `${ AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV2 }${ MicrosoftAppTenantId }/v2.0/`
    ];
}

// Define authentication configuration
const authConfig = new AuthenticationConfiguration([], claimsValidators, validTokenIssuers);

// Create credentials factory using configuration service client
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
    MicrosoftAppId: process.env.MicrosoftAppId,
    MicrosoftAppPassword: process.env.MicrosoftAppPassword,
    MicrosoftAppType: process.env.MicrosoftAppType,
    MicrosoftAppTenantId: process.env.MicrosoftAppTenantId
});

// Instantiate bot framework authentication
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env, credentialsFactory, authConfig);

// Create adapter with bot framework authentication
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling for turns
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    await sendErrorMessage(context, error);
    await sendEoCToParent(context, error);
};

// Function to send error message to user
async function sendErrorMessage(context, error) {
    try {
        let onTurnErrorMessage = 'The skill encountered an error or bug.';
        await context.sendActivity(onTurnErrorMessage, onTurnErrorMessage, InputHints.ExpectingInput);

        onTurnErrorMessage = 'To continue to run this bot, please fix the bot source code.';
        await context.sendActivity(onTurnErrorMessage, onTurnErrorMessage, InputHints.ExpectingInput);

        await context.sendTraceActivity('OnTurnError Trace', error.toString(), 'https://www.botframework.com/schemas/error', 'TurnError');
    } catch (err) {
        console.error(`\n [onTurnError] Exception caught in sendErrorMessage: ${ err }`);
    }
}

// Function to send EndOfConversation activity to parent with error details
async function sendEoCToParent(context, error) {
    try {
        const endOfConversation = {
            type: ActivityTypes.EndOfConversation,
            code: 'SkillError',
            text: error.toString()
        };
        await context.sendActivity(endOfConversation);
    } catch (err) {
        console.error(`\n [onTurnError] Exception caught in sendEoCToParent: ${ err }`);
    }
}

// Create an instance of the bot
const aiBot = new AIBot();

// Listen for incoming messages
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => aiBot.run(context));
});
