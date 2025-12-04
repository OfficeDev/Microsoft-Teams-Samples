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
        // Extract search query from parameters with validation
        let searchQuery = '';
        try {
            if (query && query.parameters && Array.isArray(query.parameters) && query.parameters.length > 0) {
                const param = query.parameters[0];
                if (param && param.value) {
                    searchQuery = param.value.trim();
                }
            }
        } catch (err) {
            console.error('Error extracting search query:', err);
        }
        
        // Use default search term if not provided or too short (npm API requires min 2 chars)
        if (!searchQuery || searchQuery.length < 2) {
            searchQuery = 'react';
        }

        try {
            const response = await axios.get(`http://registry.npmjs.com/-/v1/search?${querystring.stringify({ text: searchQuery, size: 8 })}`, {
                timeout: 10000
            });

            const attachments = [];
            response.data.objects.forEach(obj => {
                const packageName = obj.package.name;
                const description = obj.package.description || 'No description available';
                const homepage = obj.package.links.homepage || 'https://www.npmjs.com';

                // Create Hero Card (better Outlook compatibility than Adaptive Cards)
                const heroCard = CardFactory.heroCard(
                    packageName,
                    description,
                    null,
                    [
                        {
                            type: 'openUrl',
                            title: 'View on NPM',
                            value: `https://www.npmjs.com/package/${packageName}`
                        },
                        {
                            type: 'openUrl',
                            title: 'Homepage',
                            value: homepage
                        }
                    ]
                );

                // Create preview card
                const preview = CardFactory.heroCard(packageName, description);
                preview.content.tap = { type: 'invoke', value: { title: packageName, description: description } };
                
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
        } catch (error) {
            console.error('Error searching npm packages:', error);
            
            // Return error card on API failure
            const errorCard = CardFactory.heroCard(
                'Search Error',
                'Unable to search npm packages. Please try again with a different query (minimum 2 characters).'
            );
            
            return {
                composeExtension: {
                    type: 'result',
                    attachmentLayout: 'list',
                    attachments: [errorCard]
                }
            };
        }
    }

    // Invoked when the user selects an item from the search result list returned above.
    async handleTeamsMessagingExtensionSelectItem(context, obj) {
        // Create thumbnail card with selected item details
        const thumbnailCard = CardFactory.thumbnailCard(
            obj.title || 'Package Details',
            obj.description || 'No description available'
        );

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [thumbnailCard]
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

