// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const restify = require('restify');
const config = require('./config');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    ActivityTypes,
    ChannelServiceRoutes,
    CloudAdapter,
    CloudSkillHandler,
    ConfigurationServiceClientCredentialFactory,
    ConversationState,
    InputHints,
    MemoryStorage,
    SkillConversationIdFactory,
    TurnContext,
    ConfigurationBotFrameworkAuthentication,
} = require('botbuilder');

const { AI, Application, ActionPlanner, OpenAIModel, PromptManager } = require("@microsoft/teams-ai");

const {
    allowedCallersClaimsValidator,
    AuthenticationConfiguration,
    AuthenticationConstants
} = require('botframework-connector');

// Import required bot configuration.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// This bot's main dialog.
const { SkillsConfiguration } = require('./skillsConfiguration');

// Load skills configuration
const skillsConfig = new SkillsConfiguration();
const allowedSkills = Object.values(skillsConfig.skills).map(skill => skill.appId);

// Configure claims validators for allowed skills
const claimsValidators = allowedCallersClaimsValidator(allowedSkills);

// Define valid token issuers if MicrosoftAppTenantId is specified
let validTokenIssuers = [];
const { MicrosoftAppTenantId } = config.tenantId;

if (MicrosoftAppTenantId) {
    // Add the tenant as a valid JWT token issuer for Bot to Skill conversation
    validTokenIssuers = [
        `${AuthenticationConstants.ValidTokenIssuerUrlTemplateV1}${MicrosoftAppTenantId}/`,
        `${AuthenticationConstants.ValidTokenIssuerUrlTemplateV2}${MicrosoftAppTenantId}/v2.0/`,
        `${AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV1}${MicrosoftAppTenantId}/`,
        `${AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV2}${MicrosoftAppTenantId}/v2.0/`
    ];
}

// Define authentication configuration
const authConfig = new AuthenticationConfiguration([],
    claimsValidators,
    validTokenIssuers);

console.log(config.botId);

// Create credentials factory
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
    MicrosoftAppId: config.botId,
    MicrosoftAppPassword: config.botPassword,
    MicrosoftAppType: config.MicrosoftAppType,
    MicrosoftAppTenantId: config.tenantId
});

// Create bot framework authentication
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(config, credentialsFactory, authConfig);

// Create adapter
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    await sendErrorMessage(context, error);
    await endSkillConversation(context);
    await clearConversationState(context);
};

/**
 * Send an error message to the user
 * @param {object} context - The bot context
 * @param {Error} error - The error object
 */
async function sendErrorMessage(context, error) {
    try {
        // Send a message to the user
        let onTurnErrorMessage = 'The bot encountered an error or bug.';
        await context.sendActivity(onTurnErrorMessage, onTurnErrorMessage, InputHints.IgnoringInput);

        onTurnErrorMessage = 'To continue to run this bot, please fix the bot source code.';
        await context.sendActivity(onTurnErrorMessage, onTurnErrorMessage, InputHints.ExpectingInput);

        // Send a trace activity, which will be displayed in Bot Framework Emulator
        await context.sendTraceActivity(
            'OnTurnError Trace',
            `${error}`,
            'https://www.botframework.com/schemas/error',
            'TurnError'
        );
    } catch (err) {
        console.error(`\n [onTurnError] Exception caught in sendErrorMessage: ${err}`);
    }
}

/**
 * Inform the active skill that the conversation is ended
 * @param {object} context - The bot context
 */
async function endSkillConversation(context) {
    try {
        const activeSkill = await conversationState.createProperty("activeSkillProperty").get(context);
        if (activeSkill) {
            const botId = config.botId;
            let endOfConversation = {
                type: ActivityTypes.EndOfConversation,
                code: 'RootSkillError'
            };

            endOfConversation = TurnContext.applyConversationReference(
                endOfConversation, TurnContext.getConversationReference(context.activity), true);

            await conversationState.saveChanges(context, true);
            await skillClient.postActivity(botId, activeSkill.appId, activeSkill.skillEndpoint, skillsConfig.skillHostEndpoint, endOfConversation.conversation.id, endOfConversation);
        }
    } catch (err) {
        console.error(`\n [onTurnError] Exception caught on attempting to send EndOfConversation : ${err}`);
    }
}

/**
 * Delete the conversationState for the current conversation
 * @param {object} context - The bot context
 */
async function clearConversationState(context) {
    try {
        await conversationState.delete(context);
    } catch (err) {
        console.error(`\n [onTurnError] Exception caught on attempting to Delete ConversationState : ${err}`);
    }
}

// Define a state store for your bot
const memoryStorage = new MemoryStorage();
const conversationState = new ConversationState(memoryStorage);

// Create the conversationIdFactory
const conversationIdFactory = new SkillConversationIdFactory(new MemoryStorage());

// Create the skill client.
const skillClient = botFrameworkAuthentication.createBotFrameworkClient();

// Create HTTP server with increased maxParamLength to handle large conversationIds
const server = restify.createServer({ maxParamLength: 1000 });
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`\n${server.name} listening to ${server.url}`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator select "Open Bot"');
});

// Create AI components
const model = new OpenAIModel({
    azureApiKey: config.azureOpenAIKey,
    azureDefaultDeployment: config.azureOpenAIDeploymentName,
    azureEndpoint: config.azureOpenAIEndpoint,
    azureApiVersion: '2024-02-15-preview',
    useSystemMessages: true,
    logRequests: true,
});

const prompts = new PromptManager({
    promptsFolder: path.join(__dirname, "./prompts"),
});

const planner = new ActionPlanner({
    model,
    prompts,
    defaultPrompt: 'sequence'
});

// Define storage and application
const storage = new MemoryStorage();
const app = new Application({
    storage,
    ai: {
        planner,
    },
});

// We use a single skill in this example.
const targetSkillId = 'EchoSkillBot';
const targetSkill = skillsConfig.skills[targetSkillId];

const targetAISkillId = 'OpenAiSkillBot';
const targetAISkill = skillsConfig.skills[targetAISkillId];

// Create state property to track the active skill
activeSkillProperty = conversationState.createProperty("activeSkillProperty");

// Register action handlers for Echo message.
app.ai.action('EchoBot', async (context, state) => {
    try {
        // Send the activity to the skill
        await sendToSkill(context, targetSkill);
        return AI.StopCommandName;
    } catch (error) {
        console.log(error);
    }
});

// Register action handlers for Translation message
app.ai.action('TranslationBot', async (context, state) => {
    try {
        // Send the activity to the skill
        await sendToSkill(context, targetAISkill);
        return AI.StopCommandName;
    } catch (error) {
        console.log(error);
    }
});

/**
 * Send the activity to the target skill
 * @param {object} context - The bot context
 * @param {object} targetSkill - The target skill configuration
 */
async function sendToSkill(context, targetSkill) {
    // NOTE: Always SaveChanges() before calling a skill so that any activity generated by the skill
    // will have access to current accurate state.
    await conversationState.saveChanges(context, true);

    // Create a conversationId to interact with the skill and send the activity
    const skillConversationId = await conversationIdFactory.createSkillConversationIdWithOptions({
        fromBotOAuthScope: context.turnState.get(context.adapter.OAuthScopeKey),
        fromBotId: config.botId,
        activity: context.activity,
        botFrameworkSkill: targetSkill
    });

    // Route the activity to the skill
    const response = await skillClient.postActivity(
        config.botId,
        targetSkill.appId,
        targetSkill.skillEndpoint,
        skillsConfig.skillHostEndpoint,
        skillConversationId,
        context.activity
    );

    // Check response status
    if (!(response.status >= 200 && response.status <= 299)) {
        throw new Error(`[SuperBot]: Error invoking the skill id: "${targetSkill.id}" at "${targetSkill.skillEndpoint}" (status is ${response.status}). \r\n ${response.body}`);
    }
}

// Listen for incoming activities and route them to your bot main dialog
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => app.run(context));
});

// Configure skill endpoint routes
const handler = new CloudSkillHandler(adapter, (context) => app.run(context), conversationIdFactory, botFrameworkAuthentication);
const skillEndpoint = new ChannelServiceRoutes(handler);
skillEndpoint.register(server, '/api/skills');
