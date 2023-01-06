// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require('botbuilder');

class TeamsLinkUnfurlingBot extends TeamsActivityHandler {
    // This handler is used for the processing of "composeExtension/queryLink" activities from Teams.
    // https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/messaging-extensions/search-extensions#receive-requests-from-links-inserted-into-the-compose-message-box
    // By specifying domains under the messageHandlers section in the manifest, the bot can receive
    // events when a user enters in a domain in the compose box.

    handleTeamsAppBasedLinkQuery(context, query) {
        const card = CardFactory.adaptiveCard({
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.3"
        });

        const attachment = CardFactory.adaptiveCard(card);
        attachment.preview = {
            content: {
                title: "Thumbnail Card",
                text: query.url,
                images: [
                    {
                        url: "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
                    },
                ],
            },
            contentType: "application/vnd.microsoft.card.thumbnail",
        }

        const result = {
            attachmentLayout: 'list',
            type: 'result',
            attachments: [attachment],
            responseType: "composeExtension"
        };

        const response = {
            composeExtension: result
        };

        return response;
    }

    async handleTeamsMessagingExtensionQuery(context, query) {
        // Note: The Teams manifest.json for this sample also inclues a Search Query, in order to enable installing from App Studio.
        // const searchQuery = query.parameters[0].value;
        const heroCard = CardFactory.heroCard('This is a Link Unfurling Sample',
            'This sample demonstrates how to handle link unfurling in Teams.  Please review the readme for more information.');
        heroCard.content.subtitle = 'It will unfurl links from *.BotFramework.com';
        const attachment = { ...heroCard, heroCard };

        switch (query.commandId) {
            case 'searchQuery':
                return {
                    composeExtension: {
                        type: 'result',
                        attachmentLayout: 'list',
                        attachments: [
                            attachment
                        ]
                    }
                };
            default:
                throw new Error('NotImplemented');
        }
    }
}

module.exports.TeamsLinkUnfurlingBot = TeamsLinkUnfurlingBot;