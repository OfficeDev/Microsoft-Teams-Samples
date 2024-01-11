// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, DialogSet, DialogTurnStatus, OAuthPrompt, WaterfallDialog} = require('botbuilder-dialogs');
const { LogoutDialog } = require('./logoutDialog');

const CONFIRM_PROMPT = 'ConfirmPrompt';
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const { SimpleGraphClient } = require('../simpleGraphClient');
const { polyfills } = require('isomorphic-fetch');
const { CardFactory, TurnContext} = require('botbuilder-core');
const { TeamsInfo } = require('botbuilder');

class MainDialog extends LogoutDialog {
    constructor() {
        super(MAIN_DIALOG, process.env.connectionName);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000
        }));
        this.addDialog(new ConfirmPrompt(CONFIRM_PROMPT));
        this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
            this.mentionTag.bind(this),
            this.ensureOAuth.bind(this)
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

    // Sends tag mention adaptive card.
    async tagMentionAdaptiveCard(stepContext, tagName, tagId) {
        const adaptiveCardAttachment = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "speak": "This card mentions a tag:  "+tagName,
            "body": [
              {
                "type": "TextBlock",
                "text": "Mention a Tag: <at>Test Tag</at>"
              }
            ],
            "msteams": {
              "entities": [
                {
                  "type": "mention",
                  "text": "<at>Test Tag</at>",
                  "mentioned": {
                    "id": tagId,
                    "name": tagName,
                    "type": "tag"
                  }
                }
              ]
            }
          }
        await stepContext.context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCardAttachment)]
        });
    }

    // Method to invoke Tag mention functionality flow.
    async mentionTag(stepContext) {
        try {
            const tokenResponse = stepContext.result;
            if (stepContext.context.activity.conversation.conversationType === 'personal' && tokenResponse.token) {
                await stepContext.context.sendActivity('You have successfully logged in. Please install the app in the team scope to use the Tag mention functionality.');
            } else {
                let tagExists = false;
                if (tokenResponse.token) {
                    TurnContext.removeRecipientMention(stepContext.context._activity);
                    const activityText = stepContext.context.activity.text.trim();
                    if (activityText.includes('<at>')) {
                        const tagName = activityText.replace('<at>', '').replace('</at>', '').trim();
                        const tagID = stepContext.context._activity.entities[1].mentioned.id;
                        await this.tagMentionAdaptiveCard(stepContext, tagName, tagID);
                    } else if (activityText !== '') {
                        let client, teamDetails;
                        try {
                            // Fetch data from Microsoft Graph.
                            client = new SimpleGraphClient(tokenResponse.token);
                            teamDetails = await TeamsInfo.getTeamDetails(stepContext.context);
                        } catch (ex) {
                            await stepContext.context.sendActivity('You don\'t have Graph API permissions to fetch tag\'s information. Please use this command to mention a tag: "`@<Bot-name>  @<your-tag>`" to experience tag mention using the bot.');
                        }
                        const result = await client.getTag(teamDetails.aadGroupId);
                        for (const tagDetails of result.value) {
                            if (tagDetails.displayName === activityText) {
                                tagExists = true;
                                await this.tagMentionAdaptiveCard(stepContext, tagDetails.displayName, tagDetails.id);
                                break;
                            }
                        }
                        if (!tagExists) {
                            await stepContext.context.sendActivity('Provided tag name is not available in this team. Please try with another tag name or create a new tag.');
                        }
                    } else {
                        await stepContext.context.sendActivity('Please provide a tag name while mentioning the bot as "`@<Bot-name> <your-tag-name>`" or mention a tag as "`@<Bot-name> @<your-tag>`"');
                    }
                } else {
                    console.log('Response token is null or empty.');
                }
            }
        } catch (ex) {
            console.error('Error occurred while processing your request.', ex.message);
        }
    
        return await stepContext.endDialog();
    }

    async ensureOAuth(stepContext) {
        await stepContext.context.sendActivity('Thank you.');

        const result = stepContext.result;
        if (result) {
            // Call the prompt again because we need the token. The reasons for this are:
            // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
            // about refreshing it. We can always just call the prompt again to get the token.
            // 2. We never know how long it will take a user to respond. By the time the
            // user responds the token may have expired. The user would then be prompted to login again.
            //
            // There is no reason to store the token locally in the bot because we can always just call
            // the OAuth prompt to get the token or get a new token if needed.
            return await stepContext.beginDialog(OAUTH_PROMPT);
        }
        return await stepContext.endDialog();
    }

}

module.exports.MainDialog = MainDialog;
