// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('@microsoft/agents-hosting-extensions-teams');

class TeamsLinkUnfurlingBot extends TeamsActivityHandler {
    // handleTeamsAppBasedLinkQuery: link unfurl -> adaptive card + preview (fallback to "Unknown URL").
    async handleTeamsAppBasedLinkQuery(context, query) {
        const url = query?.url || 'Unknown URL';
        const adaptiveCardContent = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                { "type": "TextBlock", "text": "Link unfurled", "weight": "Bolder" },
                { "type": "TextBlock", "text": url, "wrap": true }
            ],
            "actions": [
                { "type": "Action.OpenUrl", "title": "Open link", "url": url }
            ]
        };

        const attachment = { contentType: 'application/vnd.microsoft.card.adaptive', content: adaptiveCardContent };
    // Add preview (required for reliable unfurl rendering in Teams)
        attachment.preview = {
            content: {
                title: 'Link unfurled',
                text: url,
                images: [ { url: 'https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png' } ]
            },
            contentType: 'application/vnd.microsoft.card.thumbnail'
        };
        return { composeExtension: { attachmentLayout: 'list', type: 'result', attachments: [attachment], responseType: 'composeExtension' } };
    }

    // handleTeamsMessagingExtensionQuery: search command -> help (empty), sample results, or error card.
    async handleTeamsMessagingExtensionQuery(context, query) {
        const searchParam = (query?.parameters || []).find(p => p.name === 'searchQuery');
        const searchText = (searchParam?.value || '').toString().trim();
        // Search command: empty -> help card; non-empty -> static sample results; errors -> error card.

        try {
            switch (query.commandId) {
                case 'searchQuery': {
                    if (!searchText) {
                        // Help card when query is empty.
                        const helpCard = {
                            contentType: 'application/vnd.microsoft.card.hero',
                            content: {
                                title: 'Enter a search term',
                                subtitle: 'No query provided',
                                text: 'Type a keyword after selecting the Search messaging extension command.'
                            }
                        };
                        return { composeExtension: { type: 'result', attachmentLayout: 'list', attachments: [helpCard] } };
                    }

                    // Simulate search results (static examples). In real implementation, call external service here.
                    const results = [1,2,3].map(i => ({
                        contentType: 'application/vnd.microsoft.card.hero',
                        content: {
                            title: `Result ${i} for: ${searchText}`,
                            subtitle: 'Sample search result',
                            text: `This is a placeholder item #${i}. Replace with real data source.`
                        }
                    }));

                    const response = {
                        composeExtension: {
                            type: 'result',
                            attachmentLayout: 'list',
                            attachments: results
                        }
                    };
                    return response;
                }
                default:
                    throw new Error('NotImplemented');
            }
        } catch (err) {
            // Provide a graceful error card instead of throwing raw error for better UX.
            const errorCard = {
                contentType: 'application/vnd.microsoft.card.hero',
                content: {
                    title: 'Error',
                    subtitle: 'Query failed',
                    text: `An error occurred processing your request. ${err.message}`
                }
            };
            return { composeExtension: { type: 'result', attachmentLayout: 'list', attachments: [errorCard] } };
        }
    }
}

module.exports.TeamsLinkUnfurlingBot = TeamsLinkUnfurlingBot;