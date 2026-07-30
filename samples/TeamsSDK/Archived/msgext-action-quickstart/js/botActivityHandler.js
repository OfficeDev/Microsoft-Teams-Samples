// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require('botbuilder');

class BotActivityHandler extends TeamsActivityHandler {
    /*
        Building a messaging extension action command is a two-step process:
        (1) Define how the messaging extension looks and is invoked in the client
            (via the app manifest / Developer Portal).
            Learn more: https://aka.ms/teams-me-design-action.
        (2) Define how the bot service responds to incoming action commands.
            Learn more: https://aka.ms/teams-me-respond-action.
    */

    // Invoked when the service receives an incoming action submit.
    handleTeamsMessagingExtensionSubmitAction(context, action) {
        switch (action.commandId) {
            case 'createCard':
                return createCardCommand(action);
            case 'shareMessage':
                return shareMessageCommand(action);
            default:
                throw new Error('NotImplemented');
        }
    }

    // Messaging Extension - Link Unfurling.
    handleTeamsAppBasedLinkQuery(context, query) {
        const attachment = CardFactory.thumbnailCard(
            'Thumbnail Card',
            query.url,
            ['https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png']
        );

        return {
            composeExtension: {
                attachmentLayout: 'list',
                type: 'result',
                attachments: [attachment]
            }
        };
    }
}

// 'Create Card' compose-box command - builds a hero card from user input.
function createCardCommand(action) {
    const { title, subTitle, text } = action.data;
    const heroCard = CardFactory.heroCard(title, text);
    heroCard.content.subtitle = subTitle;

    const attachment = {
        contentType: heroCard.contentType,
        content: heroCard.content,
        preview: heroCard
    };

    return {
        composeExtension: {
            type: 'result',
            attachmentLayout: 'list',
            attachments: [attachment]
        }
    };
}

// 'Share Message' command - shares a forwarded message as a hero card,
// optionally including an image (demonstrates passing custom parameters).
function shareMessageCommand(action) {
    const userName = action.messagePayload?.from?.user?.displayName ?? 'unknown';

    const images = action.data.includeImage === 'true'
        ? ['https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU']
        : [];

    const heroCard = CardFactory.heroCard(
        `${userName} originally sent this message:`,
        action.messagePayload.body.content,
        images
    );

    const attachmentCount = action.messagePayload.attachments?.length ?? 0;
    if (attachmentCount > 0) {
        // This sample does not forward MessagePayload attachments; that is left
        // as an exercise for the reader.
        heroCard.content.subtitle = `(${attachmentCount} Attachments not included)`;
    }

    const attachment = {
        contentType: heroCard.contentType,
        content: heroCard.content,
        preview: heroCard
    };

    return {
        composeExtension: {
            type: 'result',
            attachmentLayout: 'list',
            attachments: [attachment]
        }
    };
}

module.exports.BotActivityHandler = BotActivityHandler;

