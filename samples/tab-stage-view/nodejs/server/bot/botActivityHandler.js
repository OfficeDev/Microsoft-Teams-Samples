// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, MessageFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            // Base url without protocol to be used in OpenUrl encoded deeplink
            var baseUrl = process.env.baseUrl ? process.env.baseUrl.split(':')[1] : '';
            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForTabStageView(baseUrl))] });
            await next();
        });

        this.onMembersAdded(async (context, next) => {
            var welcomeText = "Hello and welcome!, Please type any bot command to see the stage view feature";
            await context.sendActivity(MessageFactory.text(welcomeText));
            await next();
        });
    }

    handleTeamsAppBasedLinkQuery(context, query) {
        const attachment = CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForTabStageView(query.url));
        attachment.preview = {
            content: {
                title: "Description",
                text: "Title",
                images: [
                    {
                        url: process.env.BaseUrl + "logo.svg",
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

module.exports.BotActivityHandler = BotActivityHandler;