// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler,MessageFactory } = require('botbuilder');
const FILES_DIR = 'public';
const path = require('path');
const axios = require('axios');
const fs = require('fs');
class DialogBot extends TeamsActivityHandler {
    /**
    *
    * @param {ConversationState} conversationState
    * @param {UserState} userState
    * @param {Dialog} dialog
    */
    constructor(conversationState, userState, dialog) {
        super();

        if (!conversationState) {
            throw new Error('[DialogBot]: Missing parameter. conversationState is required');
        }

        if (!userState) {
            throw new Error('[DialogBot]: Missing parameter. userState is required');
        }

        if (!dialog) {
            throw new Error('[DialogBot]: Missing parameter. dialog is required');
        }

        this.conversationState = conversationState;
        this.userState = userState;
        this.dialog = dialog;
        this.dialogState = this.conversationState.createProperty('DialogState');

        this.onMessage(async (context, next) => {
            console.log('Running dialog with Message Activity.');
            var activity = this.removeMentionText(context._activity);

            if(activity.text.trim() =="getchat" || activity.text.trim() =="logout" || activity.text.trim() =="login")
            // Run the Dialog with the new message Activity.
            await this.dialog.run(context, this.dialogState);

            await next();
        });
    }

    /**
    * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
    */
    async run(context) {
        await super.run(context);

        // Save any state changes. The load happened during the execution of the Dialog.
        await this.conversationState.saveChanges(context, false);
        await this.userState.saveChanges(context, false);
    }

    /**
    * Invoked when a file consent card activity is received.
    */
    async handleTeamsFileConsentAccept(context, fileConsentCardResponse) {
        try {
            const fname = path.join(FILES_DIR, fileConsentCardResponse.context.filename);
            const fileInfo = fs.statSync(fname);
            const fileContent = Buffer.from(fs.readFileSync(fname, 'binary'), 'binary');
            await axios.put(
                fileConsentCardResponse.uploadInfo.uploadUrl,
                fileContent, {
                    headers: {
                        'Content-Type': 'image/png',
                        'Content-Length': fileInfo.size,
                        'Content-Range': `bytes 0-${ fileInfo.size - 1 }/${ fileInfo.size }`
                    }
                });
            await this.fileUploadCompleted(context, fileConsentCardResponse);
        } catch (e) {
            await this.fileUploadFailed(context, e.message);
        }
    }

    /**
    * Invoked when a file consent card is declined by the user.
    */
    async handleTeamsFileConsentDecline(context, fileConsentCardResponse) {
        const reply = MessageFactory.text(`Declined. We won't upload file <b>${ fileConsentCardResponse.context.filename }</b>.`);
        reply.textFormat = 'xml';
        await context.sendActivity(reply);
    }

    // Handle file upload.
    async fileUploadCompleted(context, fileConsentCardResponse) {
        const downloadCard = {
            uniqueId: fileConsentCardResponse.uploadInfo.uniqueId,
            fileType: fileConsentCardResponse.uploadInfo.fileType
        };

        const asAttachment = {
            content: downloadCard,
            contentType: 'application/vnd.microsoft.teams.card.file.info',
            name: fileConsentCardResponse.uploadInfo.name,
            contentUrl: fileConsentCardResponse.uploadInfo.contentUrl
        };

        const reply = MessageFactory.text(`<b>File uploaded.</b> Your file <b>${ fileConsentCardResponse.uploadInfo.name }</b> is ready to download`);
        reply.textFormat = 'xml';
        reply.attachments = [asAttachment];

        await context.sendActivity(reply);
    }

    // Handle file upload failure.
    async fileUploadFailed(context, error) {
        const reply = MessageFactory.text(`<b>File upload failed.</b> Error: <pre>${ error }</pre>`);
        reply.textFormat = 'xml';

        await context.sendActivity(reply);
    }

    // Remove mention text from the activity. 
    removeMentionText(activity) {
        var updatedActivity = activity;

        if (activity.entities[0].type == "mention") {
            updatedActivity.text = activity.text.replace(activity.entities[0].text, "");
            return updatedActivity;
        }

        return activity;
    }
}

module.exports.DialogBot = DialogBot;