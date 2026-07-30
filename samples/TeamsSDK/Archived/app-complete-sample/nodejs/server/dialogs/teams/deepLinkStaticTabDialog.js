// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const DEEPLINKTAB = 'DeepLinkTab';
const TabEntityID = "statictab";
const TabConfigEntityID = "configTab";

/**
 * DeepLinkStaticTabDialog class extends ComponentDialog to handle deep link to static tab interactions.
 */
class DeepLinkStaticTabDialog extends ComponentDialog {
    /**
     * Constructor for the DeepLinkStaticTabDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(DEEPLINKTAB, [
            this.beginDeepLinkStaticTabDialog.bind(this),
        ]));
    }

    /**
     * Begins the deep link static tab dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginDeepLinkStaticTabDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "DeeplinkDialog";
        const botId = process.env.MicrosoftAppId;
        const { isChannelUser, channelId } = this.getChannelInfo(stepContext);
        const message = this.createDeepLinkMessage(stepContext, isChannelUser, channelId, botId);
        await stepContext.context.sendActivity(message);
        return await stepContext.endDialog();
    }

    /**
     * Creates and returns a deep link message.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @param {boolean} isChannelUser - Whether the user is in a channel.
     * @param {string} channelId - The channel ID.
     * @param {string} botId - The bot application ID.
     * @returns {Partial<Activity>} The deep link message.
     */
    createDeepLinkMessage(stepContext, isChannelUser, channelId, botId) {
        const reply = stepContext.context._activity;

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const card = this.createDeepLinkCard(isChannelUser, channelId, botId);
        reply.attachments = [card];
        return reply;
    }

    /**
     * Creates and returns a deep link card.
     * @param {boolean} isChannelUser - Whether the user is in a channel.
     * @param {string} channelId - The channel ID.
     * @param {string} botId - The bot application ID.
     * @returns {Attachment} The deep link card attachment.
     */
    createDeepLinkCard(isChannelUser, channelId, botId) {
        let tabUrl, buttonCaption, deepLinkCardTitle;

        if (isChannelUser) {
            tabUrl = this.getConfigTabDeepLinkURL(channelId, botId);
            buttonCaption = "Config Tab Deep Link";
            deepLinkCardTitle = "Please click below to navigate config tab";
        } else {
            tabUrl = this.getStaticTabDeepLinkURL(botId);
            buttonCaption = "Static Tab Deep Link";
            deepLinkCardTitle = "Please click below to navigate static tab";
        }

        const buttons = [
            { type: ActionTypes.OpenUrl, title: buttonCaption, value: tabUrl }
        ];

        return CardFactory.heroCard(deepLinkCardTitle, undefined, buttons);
    }

    /**
     * Returns the static tab deep link URL.
     * @param {string} botId - The bot application ID.
     * @returns {string} The static tab deep link URL.
     */
    getStaticTabDeepLinkURL(botId) {
        return `https://teams.microsoft.com/l/entity/28:${botId}/${TabEntityID}?conversationType=chat`;
    }

    /**
     * Returns the config tab deep link URL.
     * @param {string} channelId - The channel ID.
     * @param {string} botId - The bot application ID.
     * @returns {string} The config tab deep link URL.
     */
    getConfigTabDeepLinkURL(channelId, botId) {
        channelId = channelId.replace("19:", "19%3a").replace("@thread.skype", "%40thread.skype");
        return `https://teams.microsoft.com/l/entity/${botId}/${TabConfigEntityID}?context=%7B%22channelId%22%3A%22${channelId}%22%7D`;
    }

    /**
     * Determines if the user is in a channel and returns its ID.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {{ isChannelUser: boolean, channelId: string|null }}
     */
    getChannelInfo(stepContext) {
        let isChannelUser = false;
        let channelId = null;

        if (stepContext.context._activity.channelData != null) {
            channelId = stepContext.context._activity.channelData.teamsChannelId;

            if (channelId != null) {
                isChannelUser = true;
            }
        }

        return { isChannelUser, channelId };
    }
}

exports.DeepLinkStaticTabDialog = DeepLinkStaticTabDialog;