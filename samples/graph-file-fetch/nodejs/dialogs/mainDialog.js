// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog } = require('botbuilder-dialogs');
const { LogoutDialog } = require('./logoutDialog');
const { TeamsInfo, CardFactory } = require('botbuilder');
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const Token_State_Property = 'TokenData';

class MainDialog extends LogoutDialog {
    constructor(conversationState) {
        super(MAIN_DIALOG, process.env.connectionName);
        this.conversationDataAccessor = conversationState.createProperty(Token_State_Property);

        this.addDialog(new SsoOAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000
        }));

        this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
            this.loginStep.bind(this)
        ]));

        this.initialDialogId = MAIN_WATERFALL_DIALOG;
    }

    /**
    * The run method handles the incoming activity (in the form of a DialogContext) and passes it through the dialog system.
    * If no dialog is active, it will start the default dialog.
    * @param {*} dialogContext
    */
    async run(context, accessor) {
        const dialogSet = new DialogSet(accessor);
        dialogSet.add(this);
        const dialogContext = await dialogSet.createContext(context);
        const results = await dialogContext.continueDialog();

        if (results.status === DialogTurnStatus.empty) {
            await dialogContext.beginDialog(this.id);
        }
    }

    async promptStep(stepContext) {
        try {
            return await stepContext.beginDialog(OAUTH_PROMPT);
        } catch (err) {
            console.error(err);
        }
    }

    async loginStep(stepContext) {
        const tokenResponse = stepContext.result;

        if (!tokenResponse || !tokenResponse.token) {
            await stepContext.context.sendActivity('Login was not successful please try again.');
        } 
        else {
            if(stepContext.context._activity.conversation.conversationType !== "personal") {
                var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
                currentState.token = tokenResponse.token;

                // Handle different activity types
                if (stepContext.context._activity.type === 'message') {
                    const messageId = stepContext.context._activity.id;
                    const graphClient = await this.getAuthenticatedClient(tokenResponse.token);
                    const conversationType = stepContext.context._activity.conversation.conversationType;
                    let attachmentUrl = "";

                    if (conversationType === "groupChat") {
                        const chatId = stepContext.context._activity.conversation.id;
                        attachmentUrl = await this.getGroupChatAttachment(stepContext.context, graphClient, chatId, messageId);
                    }
                    else if (conversationType === "channel") {
                        const teamsChannelData = stepContext.context._activity.channelData;
                        const channelId = teamsChannelData.channel.id;
                        const teamDetails = await TeamsInfo.getTeamDetails(stepContext.context, stepContext.context._activity.channelData.team.id);
                        const teamId = teamDetails.aadGroupId;
                        
                        attachmentUrl = await this.getTeamsChannelAttachment(stepContext.context, graphClient, teamId, channelId, messageId);
                    }

                    if (attachmentUrl) {
                        try {
                            const card = CardFactory.heroCard(
                                'Download File',
                                null,
                                [
                                    {
                                        type: 'openUrl',
                                        title: 'Download',
                                        value: attachmentUrl
                                    }
                                ]
                            );

                            await stepContext.context.sendActivity({ attachments: [card] });
                        } catch (error) {
                            await stepContext.context.sendActivity(`Error accessing Graph API: ${error.message}`);
                        }
                    } else {
                        await stepContext.context.sendActivity('No attachments found in the message.');
                    }
                } 
                else if (stepContext.context._activity.type === 'invoke') {
                    // Handle invoke activity (like Teams sign-in verification)
                    await stepContext.context.sendActivity('Authentication successful. Please send the file again');
                }
            } else {
                await stepContext.context.sendActivity('Login successfully');
            }
        }

        return await stepContext.endDialog();
    }

    async getAuthenticatedClient(accessToken) {
        const { Client } = require("@microsoft/microsoft-graph-client");
        const client = Client.init({
            authProvider: (done) => {
                done(null, accessToken);
            }
        });
        return client;
    }

    async getGroupChatAttachment(context, graphClient, chatId, messageId) {
        try {
            // Get message with attachments from the chat
            const message = await graphClient
                .api(`/chats/${chatId}/messages/${messageId}`)
                .get();

            if (message.attachments && message.attachments.length > 0) {
                const attachment = message.attachments[0];
                return attachment.contentUrl;
            }

            return null;
        } catch (error) {
            await context.sendActivity(`Error getting attachment: ${error.message}`);
            throw error;
        }
    }

    async getTeamsChannelAttachment(context, graphClient, teamId, channelId, messageId) {
        try {
            // Get message with attachments from the team channel
            const message = await graphClient
                .api(`/teams/${teamId}/channels/${channelId}/messages/${messageId}`)
                .get();

            if (message.attachments && message.attachments.length > 0) {
                const attachment = message.attachments[0];
                return attachment.contentUrl;
            }

            return null;
        } catch (error) {
            await context.sendActivity(`Error getting team attachment: ${error.message}`);
            throw error;
        }
    }
}

module.exports.MainDialog = MainDialog;