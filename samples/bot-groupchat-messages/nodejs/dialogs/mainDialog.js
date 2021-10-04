// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog } = require('botbuilder-dialogs');
const { LogoutDialog } = require('./logoutDialog');
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { SimpleGraphClient } = require('../simpleGraphClient');
const { polyfills } = require('isomorphic-fetch');
const { TurnContext } = require('botbuilder');
const FILES_DIR = 'public';
const fs = require('fs');
const path = require('path');

class MainDialog extends LogoutDialog {
    constructor() {
        super(MAIN_DIALOG, process.env.connectionName);

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
        // Get the token from the previous step. Note that we could also have gotten the
        // token directly from the prompt itself. There is an example of this in the next method.
        const tokenResponse = stepContext.result;

        if (!tokenResponse || !tokenResponse.token) {
            await stepContext.context.sendActivity('Login was not successful please try again.');
        } 
        else {

            if(stepContext.context._activity.conversation.conversationType !='personal'){
                const client = new SimpleGraphClient(tokenResponse.token);
                const me = await client.getMessages(stepContext.context._activity.conversation.id);
                this.createFile(me.value);
                await this.sendFileConsentCardAsync(stepContext.context);
                return await stepContext.endDialog();
            }
            
            await stepContext.context.sendActivity("Login successful");
            await stepContext.context.sendActivity("Please type 'getchat' command in the groupchat where the bot is added to fetch messages.");
        }

        return await stepContext.endDialog();
    }

    // Create archive messages file.
    createFile(messages) {
        fs.readFile('../nodejs/public/chat.txt', 'utf-8', function(err, data) {
            if (err) throw err;
         
            var newValue = "";
         if(data.toString() != ""){
            fs.writeFile('../nodejs/public/chat.txt', newValue, 'utf-8', function(err, data) {
                if (err) throw err;
                console.log('Done!');
            })
         }

         const stream = fs.createWriteStream('../nodejs/public/chat.txt', { flags: 'a' });
         messages.map((element) => {
             if (element.messageType == 'message') {
                 stream.write(`from:${element.from.user != null ? element.from.user.displayName : element.from.application.displayName}\n`);
                 stream.write(`from:${element.body.content}\n`);
                 stream.write(`at:${element.lastModifiedDateTime}\n`)
             }
         });
        })    
    }

    // Send file consent card.
    async sendFileConsentCardAsync(context) {
        const filename = 'chat.txt';
        const stats = fs.statSync(path.join(FILES_DIR, filename));
        const fileSize = stats.size;
        const members = [{
            "aadObjectId": context._activity.from.aadObjectId,
            "name": context._activity.from.name,
            "id": context._activity.from.id
        }];

        await Promise.all(members.map(async (member) => {
            const message = this.sendFileCard(filename, fileSize);

            const ref = TurnContext.getConversationReference(context._activity);
            ref.user = member;

            await context.adapter.createConversation(ref, async (context) => {
                const ref = TurnContext.getConversationReference(context._activity);

                await context.adapter.continueConversation(ref, async (context) => {
                    await context.sendActivity(message);
                });
            });
        }));
    }

    // Create file consent card.
    sendFileCard(filename, filesize) {
        const consentContext = { filename: filename };

        const fileCard = {
            description: 'This is the file I want to send you',
            sizeInBytes: filesize,
            acceptContext: consentContext,
            declineContext: consentContext
        };

        const asAttachment = {
            content: fileCard,
            contentType: 'application/vnd.microsoft.teams.card.file.consent',
            name: filename
        };

        const reply = { attachments: [asAttachment] };
        return reply;
    }
}

module.exports.MainDialog = MainDialog;