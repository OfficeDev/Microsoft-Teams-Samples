// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory } = require('@microsoft/agents-hosting');
const { TeamsActivityHandler } = require('@microsoft/agents-hosting-extensions-teams');
const axios = require('axios');
const querystring = require('querystring');

/* Building a messaging extension search command is a two step process.
    (1) Define how the messaging extension will look and be invoked in the client.
        This can be done from the Configuration tab, or the Manifest Editor.
        Learn more: https://aka.ms/teams-me-design-search.
    (2) Define how the bot service will respond to incoming search commands.
        Learn more: https://aka.ms/teams-me-respond-search.
    
    NOTE:   Ensure the bot endpoint that services incoming messaging extension queries is
            registered with Bot Framework.
            Learn more: https://aka.ms/teams-register-bot. 
*/

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
    }

    // handleTeamsMessagingExtensionQuery: search command -> help (empty), sample results, or error card
    async handleTeamsMessagingExtensionQuery(context, query) {
        const searchQuery = query.parameters[0].value;
        const response = await axios.get(`http://registry.npmjs.com/-/v1/search?${querystring.stringify({ text: searchQuery, size: 8 })}`);

        const attachments = [];
        response.data.objects.forEach(obj => {
            const heroCard = CardFactory.heroCard(obj.package.name);
            const preview = CardFactory.heroCard(obj.package.name); // Preview cards are optional for Hero card. You need them for Adaptive Cards.
            preview.content.tap = { type: 'invoke', value: { description: obj.package.description } };
            const attachment = { ...heroCard, preview };
            attachments.push(attachment);
        });

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: attachments
            }
        };
    }

    // Invoked when the user selects an item from the search result list returned above.
    async handleTeamsMessagingExtensionSelectItem(context, obj) {
        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [CardFactory.thumbnailCard(obj.description)]
            }
        };
    }

    // handleTeamsAppBasedLinkQuery: link unfurl -> adaptive card + preview (fallback to "Unknown URL")
    async handleTeamsAppBasedLinkQuery(context, query) {
        const attachment = CardFactory.thumbnailCard('Thumbnail Card',
            query.url,
            ['https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png']);
    
        const result = {
            attachmentLayout: 'list',
            type: 'result',
            attachments: [attachment]
        };
    
        return {
            composeExtension: result
        };
    }
}

module.exports.BotActivityHandler = BotActivityHandler;

