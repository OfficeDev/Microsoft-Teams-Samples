// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { TeamsActivityHandler, CardFactory } from 'botbuilder';

export class TeamsLinkUnfurlingBot extends TeamsActivityHandler {
    handleTeamsAppBasedLinkQuery(context, query) {
        const attachment = CardFactory.adaptiveCard({
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.3"
        });

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
        };

        return {
            composeExtension: {
                attachmentLayout: 'list',
                type: 'result',
                attachments: [attachment],
                responseType: 'composeExtension'
            }
        };
    }

    async handleTeamsMessagingExtensionQuery(context, query) {
        const heroCard = CardFactory.heroCard(
            'This is a Link Unfurling Sample',
            'This sample demonstrates how to handle link unfurling in Teams. Please review the readme for more information.');
        heroCard.content.subtitle = 'It will unfurl links from *.BotFramework.com';

        switch (query.commandId) {
            case 'searchQuery':
                return {
                    composeExtension: {
                        type: 'result',
                        attachmentLayout: 'list',
                        attachments: [heroCard]
                    }
                };
            default:
                throw new Error('NotImplemented');
        }
    }
}