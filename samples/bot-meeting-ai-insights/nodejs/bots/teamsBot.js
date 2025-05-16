// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogBot } = require('./dialogBot');

/**
 * TeamsBot class that extends DialogBot to handle Teams-specific activities.
 */
class TeamsBot extends DialogBot {
    /**
     * Creates an instance of TeamsBot.
     * @param {ConversationState} conversationState - The state management object for conversation state.
     * @param {UserState} userState - The state management object for user state.
     * @param {Dialog} dialog - The dialog to be run by the bot.
     */
    constructor(conversationState, userState, dialog) {
        super(conversationState, userState, dialog);

        this.onMembersAdded(this.handleMembersAdded.bind(this));
    }

    /**
     * Handles members being added to the conversation.
     * @param {TurnContext} context - The context object for the turn.
     * @param {function} next - The next middleware function in the pipeline.
     */
    async handleMembersAdded(context, next) {
        const membersAdded = context.activity.membersAdded;
        for (const member of membersAdded) {
            if (member.id !== context.activity.recipient.id) {
                await context.sendActivity('Welcome to TeamsBot. Type anything to get logged in. Type \'logout\' to sign-out.');
            }
        }
        await next();
    }

    /**
     * Receives invoke activities with Activity name of 'signin/verifyState'.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} state - The state object.
     */
    async handleTeamsSigninVerifyState(context, state) {
        console.log('Running dialog with signin/verifyState from an Invoke Activity.');
        await this.dialog.run(context, this.dialogState);
    }

    /**
     * Receives invoke activities with Activity name of 'signin/tokenExchange'.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} state - The state object.
     */
    async handleTeamsSigninTokenExchange(context, state) {
        console.log('Running dialog with signin/tokenExchange from an Invoke Activity.');
        await this.dialog.run(context, this.dialogState);
    }
}

module.exports.TeamsBot = TeamsBot;
