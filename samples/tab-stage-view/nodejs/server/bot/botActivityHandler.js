// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { AgentApplication, MemoryStorage, MessageFactory } = require('@microsoft/agents-hosting');
const { TeamsAgentExtension } = require('@microsoft/agents-hosting-extensions-teams');
const adaptiveCards = require('../models/adaptiveCard');
const adaptiveCardUnfurling = require('../models/adaptiveCardUnfurling');

/**
 * Creates and configures the Agent Application with Teams extension
 * @returns {AgentApplication} The configured agent application
 */
function createAgentApp() {
    const agent = new AgentApplication({
        storage: new MemoryStorage()
    });

    // Register Teams extension for Teams-specific features like link unfurling
    const teamsExt = new TeamsAgentExtension(agent);

    agent.registerExtension(teamsExt, (tae) => {
        // Handle app based link query (link unfurling)
        tae.messageExtension.onQueryLink(async (context, state, link) => {
            console.log('Link unfurling triggered for URL:', link);
            const card = adaptiveCardUnfurling.adaptiveCardForTabStageView();
            return {
                attachmentLayout: 'list',
                type: 'result',
                attachments: [{
                    contentType: 'application/vnd.microsoft.card.adaptive',
                    content: card,
                    preview: {
                        contentType: 'application/vnd.microsoft.card.thumbnail',
                        content: {
                            title: 'Tab Stage View',
                            text: 'Click to open in stage view',
                            images: []
                        }
                    }
                }]
            };
        });
    });

    // Handle invoke activities for link unfurling (fallback handler)
    agent.onActivity('invoke', async (context, state) => {
        const activity = context.activity;
        console.log('Invoke activity received:', activity.name);
        
        if (activity.name === 'composeExtension/queryLink') {
            console.log('Link unfurling request:', activity.value?.url);
            const card = adaptiveCardUnfurling.adaptiveCardForTabStageView();
            
            const response = {
                composeExtension: {
                    attachmentLayout: 'list',
                    type: 'result',
                    attachments: [{
                        contentType: 'application/vnd.microsoft.card.adaptive',
                        content: card,
                        preview: {
                            contentType: 'application/vnd.microsoft.card.adaptive',
                            content: card
                        }
                    }]
                }
            };
            
            await context.sendActivity({
                type: 'invokeResponse',
                value: {
                    status: 200,
                    body: response
                }
            });
            return;
        }
    });

    // Handle new members added to conversation
    agent.onConversationUpdate('membersAdded', async (context) => {
        const welcomeText = "Hello and welcome!, Please type any bot command to see the stage view feature";
        await context.sendActivity(welcomeText);
    });

    // Handle incoming messages
    agent.onActivity('message', async (context, state) => {
        // Base url without protocol to be used in OpenUrl encoded deeplink
        const baseUrl = process.env.BaseUrl || process.env.BOT_ENDPOINT || '';
        const card = adaptiveCards.adaptiveCardForTabStageView(baseUrl);
        
        const message = MessageFactory.attachment({
            contentType: 'application/vnd.microsoft.card.adaptive',
            content: card
        });
        
        await context.sendActivity(message);
    });

    return agent;
}

module.exports = { createAgentApp };