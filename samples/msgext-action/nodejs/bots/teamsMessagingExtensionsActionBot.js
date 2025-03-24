// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

require('dotenv').config();
const { TeamsActivityHandler, CardFactory, TeamsInfo, MessageFactory } = require('botbuilder');
const baseUrl = process.env.BaseUrl;

/**
 * TeamsMessagingExtensionsActionBot handles messaging extension actions in Microsoft Teams.
 */
class TeamsMessagingExtensionsActionBot extends TeamsActivityHandler {
    constructor() {
        super();
    }

    /**
     * Handles the submit action from the messaging extension.
     * @param {TurnContext} context - The context object for the turn.
     * @param {MessagingExtensionAction} action - The action object.
     */
    async handleTeamsMessagingExtensionSubmitAction(context, action) {
        switch (action.commandId) {
            case 'createCard':
                return createCardCommand(context, action);
            case 'shareMessage':
                return shareMessageCommand(context, action);
            case 'webView':
                return await webViewResponse(action);
        }
    }

    /**
     * Handles the fetch task action from the messaging extension.
     * @param {TurnContext} context - The context object for the turn.
     * @param {MessagingExtensionAction} action - The action object.
     */
    async handleTeamsMessagingExtensionFetchTask(context, action) {
        switch (action.commandId) {
            case 'webView':
                return empDetails();
            case 'Static HTML':
                return dateTimeInfo();
            default:
                try {
                    const member = await this.getSingleMember(context);
                    return {
                        task: {
                            type: 'continue',
                            value: {
                                card: getAdaptiveCardAttachment(),
                                height: 400,
                                title: `Hello ${member}`,
                                width: 300
                            }
                        }
                    };
                } catch (e) {
                    if (e.code === 'BotNotInConversationRoster') {
                        return {
                            task: {
                                type: 'continue',
                                value: {
                                    card: getJustInTimeCardAttachment(),
                                    height: 400,
                                    title: 'Adaptive Card - App Installation',
                                    width: 300
                                }
                            }
                        };
                    }
                    throw e;
                }
        }
    }

    /**
     * Gets a single member from the conversation.
     * @param {TurnContext} context - The context object for the turn.
     */
    async getSingleMember(context) {
        try {
            const member = await TeamsInfo.getMember(context, context.activity.from.id);
            return member.name;
        } catch (e) {
            if (e.code === 'MemberNotFoundInConversation') {
                await context.sendActivity(MessageFactory.text('Member not found.'));
                return e.code;
            }
            throw e;
        }
    }
}

/**
 * Creates a just-in-time card attachment.
 */
function getJustInTimeCardAttachment() {
    return CardFactory.adaptiveCard({
        actions: [
            {
                type: 'Action.Submit',
                title: 'Continue',
                data: { msteams: { justInTimeInstall: true } }
            }
        ],
        body: [
            {
                text: 'Looks like you have not used Action Messaging Extension app in this team/chat. Please click **Continue** to add this app.',
                type: 'TextBlock',
                wrap: true
            }
        ],
        type: 'AdaptiveCard',
        version: '1.0'
    });
}

/**
 * Creates an adaptive card attachment.
 */
function getAdaptiveCardAttachment() {
    return CardFactory.adaptiveCard({
        actions: [{ type: 'Action.Submit', title: 'Close' }],
        body: [
            {
                text: 'This app is installed in this conversation. You can now use it to do some great stuff!!!',
                type: 'TextBlock',
                isSubtle: false,
                wrap: true
            }
        ],
        type: 'AdaptiveCard',
        version: '1.0'
    });
}

/**
 * Handles the create card command.
 * @param {TurnContext} context - The context object for the turn.
 * @param {MessagingExtensionAction} action - The action object.
 */
function createCardCommand(context, action) {
    const data = action.data;
    const heroCard = CardFactory.heroCard(data.title, data.text);
    heroCard.content.subtitle = data.subTitle;
    const attachment = { contentType: heroCard.contentType, content: heroCard.content, preview: heroCard };

    return {
        composeExtension: {
            type: 'result',
            attachmentLayout: 'list',
            attachments: [attachment]
        }
    };
}

/**
 * Handles the share message command.
 * @param {TurnContext} context - The context object for the turn.
 * @param {MessagingExtensionAction} action - The action object.
 */
function shareMessageCommand(context, action) {
    let userName = 'unknown';
    if (action.messagePayload.from && action.messagePayload.from.user && action.messagePayload.from.user.displayName) {
        userName = action.messagePayload.from.user.displayName;
    }

    let images = [];
    const includeImage = action.data.includeImage;
    if (includeImage === 'true') {
        images = ['https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU'];
    }
    const heroCard = CardFactory.heroCard(`${userName} originally sent this message:`, action.messagePayload.body.content, images);

    if (action.messagePayload.attachments && action.messagePayload.attachments.length > 0) {
        heroCard.content.subtitle = `(${action.messagePayload.attachments.length} Attachments not included)`;
    }

    const attachment = { contentType: heroCard.contentType, content: heroCard.content, preview: heroCard };

    return {
        composeExtension: {
            type: 'result',
            attachmentLayout: 'list',
            attachments: [attachment]
        }
    };
}

/**
 * Returns employee details task module.
 */
function empDetails() {
    return {
        task: {
            type: 'continue',
            value: {
                width: 350,
                height: 300,
                title: 'Task module WebView',
                url: `${baseUrl}/customForm`
            }
        }
    };
}

/**
 * Returns date and time info task module.
 */
function dateTimeInfo() {
    return {
        task: {
            type: 'continue',
            value: {
                width: 450,
                height: 125,
                title: 'Task module Static HTML',
                url: `${baseUrl}/staticPage`
            }
        }
    };
}

/**
 * Handles the web view response.
 * @param {MessagingExtensionAction} action - The action object.
 */
async function webViewResponse(action) {
    const data = action.data;
    const heroCard = CardFactory.heroCard(`ID: ${data.EmpId}`, `E-Mail: ${data.EmpEmail}`);
    heroCard.content.subtitle = `Name: ${data.EmpName}`;
    const attachment = { contentType: heroCard.contentType, content: heroCard.content, preview: heroCard };
    return {
        composeExtension: {
            type: 'result',
            attachmentLayout: 'list',
            attachments: [attachment]
        }
    };
}

module.exports.TeamsMessagingExtensionsActionBot = TeamsMessagingExtensionsActionBot;
