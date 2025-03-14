// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogBot } = require('./DialogBot');

/**
 * Bot class extends DialogBot to handle Teams-specific events and interactions.
 */
class Bot extends DialogBot {
    /**
     * Constructor for the Bot class.
     * @param {ConversationState} conversationState - The state management object for conversation state.
     * @param {UserState} userState - The state management object for user state.
     * @param {Dialog} dialog - The dialog to be used by the bot.
     */
    constructor(conversationState, userState, dialog) {
        super(conversationState, userState, dialog);

        this.onMembersAdded(this.handleMembersAdded.bind(this));
    }

    /**
     * Handles the event when new members are added to the conversation.
     * @param {TurnContext} context - The context object for the turn.
     * @param {function} next - The next middleware function in the pipeline.
     */
    async handleMembersAdded(context, next) {
        const membersAdded = context.activity.membersAdded;
        for (const member of membersAdded) {
            if (member.id !== context.activity.recipient.id) {
                await context.sendActivity("Hello, I'm your new bot!");
            }
        }
        await next();
    }

    /**
     * Handles the Teams Sign-In verification state.
     * @param {TurnContext} context - The context object for the turn.
     * @param {string} state - The state string for the sign-in verification.
     */
    async handleTeamsSigninVerifyState(context, state) {
        await context.sendActivity("Authentication successful");
    }
}

module.exports.Bot = Bot;