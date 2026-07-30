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

        // Called when a new user is joins the team, or when the application is first installed
        this.onTeamsMembersAddedEvent(async (membersAdded, teamInfo, context, next) => {
            console.log(membersAdded);
            if (membersAdded[0].id !== context.activity.recipient.id && context.activity.conversation.conversationType !== 'personal') {
                await context.sendActivity(`Welcome to the team ${member.givenName} ${member.surname}.`);
            }
            await next();
        }); 
    }

    // This is triggered during the app installation update activity
    async onInstallationUpdateActivity(Context) {
        if (Context._activity.conversation.conversationType === 'channel') {
            await Context.sendActivity(`Welcome to Tag mention Teams bot app. Please follow the below commands for mentioning the tags: \r\n\r\n\r\n1. Command: " \`@<Bot-name> <your-tag-name>\` " - It will work only if you have Graph API permissions to fetch the tags and bot will mention the tag accordingly in team's channel scope.\r\n\r\n\r\n2. Command " \`@<Bot-name> @<your-tag>\` " - It will work without Graph API permissions but you need to provide the tag as command to experience tag mention using bot.`);
        } else {
            await Context.sendActivity('Welcome to Tag mention demo bot. Type anything to get logged in. Type \'logout\' to sign-out');
        }
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
