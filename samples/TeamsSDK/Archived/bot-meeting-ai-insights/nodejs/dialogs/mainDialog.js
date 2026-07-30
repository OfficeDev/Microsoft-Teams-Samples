// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, DialogSet, DialogTurnStatus, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios');

const CONFIRM_PROMPT = 'ConfirmPrompt';
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';

/**
 * MainDialog class that extends LogoutDialog to handle the main dialog flow.
 */
class MainDialog extends LogoutDialog {
    /**
     * Creates an instance of MainDialog.
     * @param {string} id - The dialog ID.
     * @param {string} connectionName - The connection name for the OAuth provider.
     */
    constructor() {
        super(MAIN_DIALOG, process.env.connectionName);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000,
        }));
        this.addDialog(new ConfirmPrompt(CONFIRM_PROMPT));
        this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
            this.loginStep.bind(this)
        ]));

        this.initialDialogId = MAIN_WATERFALL_DIALOG;
    }

    /**
     * The run method handles the incoming activity (in the form of a DialogContext) and passes it through the dialog system.
     * If no dialog is active, it will start the default dialog.
     * @param {TurnContext} context - The context object for the turn.
     * @param {StatePropertyAccessor} accessor - The state property accessor for the dialog state.
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

    /**
     * Prompts the user to sign in.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    /**
     * Handles the login step and retrieves AI insights.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    async loginStep(stepContext) {
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            const ssoToken = tokenResponse.token;

            if (ssoToken) {
                const userId = process.env.userId;
                const meetingUrl = process.env.meetingJoinUrl;
                const onlineMeetingId = await this.getMeetingId(ssoToken, meetingUrl);
                const aiInsightId = await this.getAiInsightId(ssoToken, userId, onlineMeetingId);

                if (aiInsightId) {
                    const aiInsight = await this.getAiInsightDetails(ssoToken, userId, onlineMeetingId, aiInsightId);                    if (aiInsight) {
                        let formattedMessage = '';
                        if (Array.isArray(aiInsight)) {
                            formattedMessage = aiInsight.map(insight => {
                                return `## ${insight.title}\n${insight.text}\n`;
                            }).join('\n');
                        }
                        await stepContext.context.sendActivity(formattedMessage || 'No insights found in the expected format.');
                    } else {
                        await stepContext.context.sendActivity('Failed to retrieve AI Insight details.');
                    }
                } else {
                    await stepContext.context.sendActivity('Failed to retrieve AI Insight ID.');
                }
            } else {
                await stepContext.context.sendActivity('Failed to retrieve access token for downstream API.');
            }
            return await stepContext.endDialog();
        }
        await stepContext.context.sendActivity('Login was not successful, please try again.');
        return await stepContext.endDialog();
    }

    async getMeetingId(accessToken, meetingUrl) {
        try {
            const url = `https://graph.microsoft.com/v1.0/me/onlineMeetings?$filter=JoinWebUrl eq '${meetingUrl}'`;

            const response = await axios.get(url, {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            });
            return response.data.value[0].id;
        } catch (error) {
            console.error('Error retrieving AI Insight ID:', error);
            return null;
        }
    }

    /**
     * Calls the Graph API to get the AI Insight ID.
     * @param {string} accessToken - The access token for the Graph API.
     * @param {string} userId - The user ID.
     * @param {string} onlineMeetingId - The online meeting ID.
     * @returns {Promise<string>} - The AI Insight ID.
     */
    async getAiInsightId(accessToken, userId, onlineMeetingId) {
        try {
            const url = `https://graph.microsoft.com/beta/copilot/users/${userId}/onlineMeetings/${onlineMeetingId}/aiInsights`;

            const response = await axios.get(url, {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            });
            return response.data.value[0].id;
        } catch (error) {
            console.error('Error retrieving AI Insight ID:', error);
            return null;
        }
    }

    /**
     * Calls the Graph API to get the AI Insight details.
     * @param {string} accessToken - The access token for the Graph API.
     * @param {string} userId - The user ID.
     * @param {string} onlineMeetingId - The online meeting ID.
     * @param {string} aiInsightId - The AI Insight ID.
     */
    async getAiInsightDetails(accessToken, userId, onlineMeetingId, aiInsightId) {
        try {
            const url = `https://graph.microsoft.com/beta/copilot/users/${userId}/onlineMeetings/${onlineMeetingId}/aiInsights/${aiInsightId}`;

            const response = await axios.get(url, {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            });

            return response.data.meetingNotes;
        } catch (error) {
            console.error('Error retrieving AI Insight details:', error);
            return null;
        }
    }
}

module.exports.MainDialog = MainDialog;