// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const DEEPLINKTAB = 'DeepLinkTab';
const TabEntityID = "statictab";
const TabConfigEntityID = "configTab";
let isChannelUser;
let channelId;
let tabUrl;
let buttonCaption;
let deepLinkCardTitle;
let botId;

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
        botId = process.env.MicrosoftAppId;
        this.getChannelID(stepContext);
        const message = this.createDeepLinkMessage(stepContext);
        await stepContext.context.sendActivity(message);
        return await stepContext.endDialog();
    }

    /**
     * Creates and returns a deep link message.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Partial<Activity>} The deep link message.
     */
    createDeepLinkMessage(stepContext) {
        const reply = stepContext.context._activity;

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const card = this.createDeepLinkCard();
        reply.attachments = [card];
        return reply;
    }

    /**
     * Creates and returns a deep link card.
     * @returns {Attachment} The deep link card attachment.
     */
    createDeepLinkCard() {
        if (isChannelUser) {
            tabUrl = this.getConfigTabDeepLinkURL(channelId);
            buttonCaption = "Config Tab Deep Link";
            deepLinkCardTitle = "Please click below to navigate config tab";
        } else {
            tabUrl = this.getStaticTabDeepLinkURL();
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
     * @returns {string} The static tab deep link URL.
     */
    getStaticTabDeepLinkURL() {
        return `https://teams.microsoft.com/l/entity/28:${botId}/${TabEntityID}?conversationType=chat`;
    }

    /**
     * Returns the config tab deep link URL.
     * @param {string} channelId - The channel ID.
     * @returns {string} The config tab deep link URL.
     */
    getConfigTabDeepLinkURL(channelId) {
        channelId = channelId.replace("19:", "19%3a").replace("@thread.skype", "%40thread.skype");
        return `https://teams.microsoft.com/l/entity/${botId}/${TabConfigEntityID}?context=%7B%22channelId%22%3A%22${channelId}%22%7D`;
    }

    /**
     * Sets the channel ID and determines if the user is in a channel.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    getChannelID(stepContext) {
        isChannelUser = false;

        if (stepContext.context._activity.channelData != null) {
            channelId = stepContext.context._activity.channelData.teamsChannelId;

            if (channelId != null) {
                isChannelUser = true;
            }
        }
    }
}

exports.DeepLinkStaticTabDialog = DeepLinkStaticTabDialog;