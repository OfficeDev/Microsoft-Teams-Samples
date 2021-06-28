// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsActivityHandler, CardFactory, ActionTypes, TurnContext } = require('botbuilder');
class DialogBot extends TeamsActivityHandler {
    /**
     *
     * @param {ConversationState} conversationState
     * @param {UserState} userState
     * @param {Dialog} dialog
     * @param {ConversationReferences} conversationReferences
     */
    constructor(conversationState, userState, dialog, conversationReferences) {
        super();
        if (!conversationState) throw new Error('[DialogBot]: Missing parameter. conversationState is required');
        if (!userState) throw new Error('[DialogBot]: Missing parameter. userState is required');
        if (!dialog) throw new Error('[DialogBot]: Missing parameter. dialog is required');
        if (!conversationReferences) throw new Error('[conversationReferences]: Missing parameter. dialog is required');

        this.conversationState = conversationState;
        this.userState = userState;
        this.dialog = dialog;
        this.dialogState = this.conversationState.createProperty('DialogState');
        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        this.conversationReferences = conversationReferences;

        this.onMessage(async (context, next) => {
            this.addConversationReference(context.activity);
            console.log('Running dialog with Message Activity.');
            // Run the Dialog with the new message Activity.
            await this.dialog.run(context, this.dialogState);
            await next();
        });
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        this.conversationReferences[conversationReference.conversation.id] = conversationReference;
    }

    /**
     * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
     */
    async run(context) {
        this.addConversationReference(context.activity);
        await super.run(context);
        // Save any state changes. The load happened during the execution of the Dialog.
        await this.conversationState.saveChanges(context, false);
        await this.userState.saveChanges(context, false);
    }

    async DisplayData(context, header, availability, activity) {
        try {
            const card = {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.3",
                "body": [
                    {
                        "type": "TextBlock",
                        "size": "Medium",
                        "weight": "Bolder",
                        "text": header
                    },
                    {
                        "type": "FactSet",
                        "facts": [
                            {
                                "title": "Availabilty:",
                                "value": availability
                            },
                            {
                                "title": "Activity:",
                                "value": activity
                            }
                        ]
                    }
                ]
            }
            const tdata = CardFactory.adaptiveCard(card);
            await context.sendActivity({ attachments: [tdata] });
        }
        catch (e) {
            console.log("error", e);
        }
    }
}
module.exports.DialogBot = DialogBot;
