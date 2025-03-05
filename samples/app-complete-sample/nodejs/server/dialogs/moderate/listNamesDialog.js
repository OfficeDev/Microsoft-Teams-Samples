// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsInfo, CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');

const LISTNAMES = 'ListNames';

/**
 * Dialog to list names of members in a Microsoft Teams channel.
 */
class ListNamesDialog extends ComponentDialog {
    /**
     * Creates a new instance of the ListNamesDialog class.
     * @param {string} id - The dialog ID.
     * @param {Object} conversationDataAccessor - The conversation state accessor.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(LISTNAMES, [
            this.beginListNamesDialog.bind(this),
        ]));
    }

    /**
     * Begins the ListNamesDialog.
     * @param {Object} stepContext - The step context.
     * @returns {Promise} - A promise representing the asynchronous operation.
     */
    async beginListNamesDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "ListNamesDialog";

        const members = await TeamsInfo.getMembers(stepContext.context);
        const reply = stepContext.context._activity;

        if (reply.attachments && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        reply.text = JSON.stringify(members);
        const cards = members.map(member => this.createInformationCard(member.givenName + member.surname, member.aadObjectId));
        reply.attachments = cards;

        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    /**
     * Creates an information card for a member.
     * @param {string} name - The member's name.
     * @param {string} aadId - The member's Azure Active Directory ID.
     * @returns {Object} - The information card.
     */
    createInformationCard(name, aadId) {
        const chatUrl = `https://teams.microsoft.com/l/chat/0/0?users=${aadId}`;
        const buttons = [
            { type: ActionTypes.OpenUrl, title: 'Chat', value: chatUrl },
        ];
        return CardFactory.heroCard(name, undefined, buttons);
    }
}

module.exports.ListNamesDialog = ListNamesDialog;