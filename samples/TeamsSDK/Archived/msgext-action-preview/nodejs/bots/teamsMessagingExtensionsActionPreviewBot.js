// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, MessageFactory } = require('botbuilder');
const { AdaptiveCardHelper } = require('../adaptiveCardHelper');
const { CardResponseHelpers } = require('../cardResponseHelpers');

class TeamsMessagingExtensionsActionPreviewBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.onMessage(async (context, next) => {
            if (context.activity.value) {
                // This was a message from the card.
                const { Answer: answer, Choices: choices } = context.activity.value;
                await context.sendActivity(MessageFactory.text(`${ context.activity.from.name } answered '${ answer }' and chose '${ choices }'.`));
            } else {
                // This is a regular text message.
                await context.sendActivity(MessageFactory.text('Hello from the TeamsMessagingExtensionsActionPreviewBot.'));
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    handleTeamsMessagingExtensionFetchTask(context, action) {
        const adaptiveCard = AdaptiveCardHelper.createAdaptiveCardEditor();
        return CardResponseHelpers.toTaskModuleResponse(adaptiveCard);
    }

    handleTeamsMessagingExtensionSubmitAction(context, action) {
        const submittedData = action.data;
        const adaptiveCard = AdaptiveCardHelper.createAdaptiveCardAttachment(submittedData);
        return CardResponseHelpers.toMessagingExtensionBotMessagePreviewResponse(adaptiveCard);
    }

    handleTeamsMessagingExtensionBotMessagePreviewEdit(context, action) {
        // The data has been returned to the bot in the action structure.
        const submitData = AdaptiveCardHelper.toSubmitExampleData(action);

        // This is a preview edit call and so this time we want to re-create the adaptive card editor.
        const adaptiveCard = AdaptiveCardHelper.createAdaptiveCardEditor(submitData.Question, (submitData.MultiSelect.toLowerCase() === 'true'), submitData.Option1, submitData.Option2, submitData.Option3);

        return CardResponseHelpers.toTaskModuleResponse(adaptiveCard);
    }

    async handleTeamsMessagingExtensionBotMessagePreviewSend(context, action) {
        // The data has been returned to the bot in the action structure.
        const submitData = AdaptiveCardHelper.toSubmitExampleData(action);

        // This is a send so we are done and we will create the adaptive card attachment.
        const adaptiveCard = AdaptiveCardHelper.createAdaptiveCardAttachment(submitData);

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [adaptiveCard]
            }
        };
    }

    async handleTeamsMessagingExtensionCardButtonClicked(context, obj) {
        // If the adaptive card was added to the compose window (by either the handleTeamsMessagingExtensionSubmitAction or
        // handleTeamsMessagingExtensionBotMessagePreviewSend handler's return values) the submit values will come in here.
        const reply = MessageFactory.text('handleTeamsMessagingExtensionCardButtonClicked Value: ' + JSON.stringify(context.activity.value));
        await context.sendActivity(reply);
    }
}

module.exports.TeamsMessagingExtensionsActionPreviewBot = TeamsMessagingExtensionsActionPreviewBot;
