// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogBot } = require('./dialogBot');

class TeamsBot extends DialogBot {
    /**
     *
     * @param {ConversationState} conversationState
     * @param {UserState} userState
     * @param {Dialog} dialog
     */
    constructor(conversationState, userState, dialog) {
        super(conversationState, userState, dialog);

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let cnt = 0; cnt < membersAdded.length; cnt++) {
                if (membersAdded[cnt].id !== context.activity.recipient.id) {
                    await context.sendActivity('Welcome to TeamsBot. Type anything to get logged in. Type \'logout\' to sign-out.');
                }
            }

            await next();
        });
    }
	
    async handleTeamsSigninVerifyState(context, query) {
        console.log('Running dialog with signin/verifystate from an Invoke Activity.');
        await this.dialog.run(context, this.dialogState);
    }

    async handleTeamsSigninTokenExchange(context, query) {
        console.log('Running dialog with signin/tokenExchange from an Invoke Activity.');
        await this.dialog.run(context, this.dialogState);
    }

}

module.exports.TeamsBot = TeamsBot;
