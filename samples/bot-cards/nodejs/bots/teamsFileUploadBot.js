// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const fs = require('fs');
const path = require('path');
const axios = require('axios');
const { TurnContext, MessageFactory, TeamsActivityHandler } = require('botbuilder');
const { MicrosoftAppCredentials } = require('botframework-connector');
const { generateFileName, getFileSize, writeFile } = require('../services/fileService');

const FILES_DIR = 'files';

/**
 * TeamsFileUploadBot class handles file upload interactions with Teams.
 */
class TeamsFileUploadBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);
            const attachment = context.activity.attachments && context.activity.attachments[0];

            if (attachment) {
                const imageRegex = /image\/.*/;

                if (attachment.contentType === 'application/vnd.microsoft.teams.file.download.info') {
                    // Handle file download info
                    await this.handleFileDownload(attachment, context);
                } else if (imageRegex.test(attachment.contentType)) {
                    // Handle inline image
                    await this.processInlineImage(context);
                } else {
                    // Send a default file card if no relevant attachment is found
                    await this.sendFileCard(context);
                }
            }
            await next();
        });
    }

    /**
     * Handles file download and saves the file.
     * @param {object} file - The file attachment.
     * @param {object} context - The bot context.
     */
    async handleFileDownload(file, context) {
        const config = {
            responseType: 'stream'
        };
        const filePath = path.join(FILES_DIR, file.name);
        await writeFile(file.content.downloadUrl, config, filePath);

        const reply = MessageFactory.text(`<b>${file.name}</b> received and saved.`);
        reply.textFormat = 'xml';
        await context.sendActivity(reply);
    }

    /**
     * Sends a file card for user consent.
     * @param {object} context - The bot context.
     */
    async sendFileCard(context) {
        const filename = 'teams-logo.png';
        const stats = fs.statSync(path.join(FILES_DIR, filename));
        const fileSize = stats.size;

        const consentContext = { filename: filename };
        const fileCard = {
            description: 'This is the file I want to send you',
            sizeInBytes: fileSize,
            acceptContext: consentContext,
            declineContext: consentContext
        };

        const asAttachment = {
            content: fileCard,
            contentType: 'application/vnd.microsoft.teams.card.file.consent',
            name: filename
        };
        await context.sendActivity({ attachments: [asAttachment] });
    }

    /**
     * Handles file consent acceptance and uploads the file.
     * @param {object} context - The bot context.
     * @param {object} fileConsentCardResponse - The consent response.
     */
    async handleTeamsFileConsentAccept(context, fileConsentCardResponse) {
        try {
            const fname = path.join(FILES_DIR, fileConsentCardResponse.context.filename);
            const fileInfo = fs.statSync(fname);
            const fileContent = fs.createReadStream(fname); // Use stream instead of reading into memory

            await axios.put(
                fileConsentCardResponse.uploadInfo.uploadUrl,
                fileContent, {
                    headers: {
                        'Content-Type': 'image/png',
                        'Content-Length': fileInfo.size,
                        'Content-Range': `bytes 0-${fileInfo.size - 1}/${fileInfo.size}`
                    }
                });

            await this.fileUploadCompleted(context, fileConsentCardResponse);
        } catch (e) {
            await this.fileUploadFailed(context, e.message);
        }
    }

    /**
     * Handles file consent decline and informs the user.
     * @param {object} context - The bot context.
     * @param {object} fileConsentCardResponse - The consent response.
     */
    async handleTeamsFileConsentDecline(context, fileConsentCardResponse) {
        const reply = MessageFactory.text(`The file <b>${fileConsentCardResponse.context.filename}</b> has been declined and will not be uploaded.`);
        reply.textFormat = 'xml';
        await context.sendActivity(reply);
    }

    /**
     * Notifies the user when the file upload is completed.
     * @param {object} context - The bot context.
     * @param {object} fileConsentCardResponse - The consent response.
     */
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

        const reply = MessageFactory.text(`<b>Your file ${fileConsentCardResponse.uploadInfo.name}</b> has been successfully uploaded and is ready to download.`);
        reply.textFormat = 'xml';
        reply.attachments = [asAttachment];
        await context.sendActivity(reply);
    }

    /**
     * Handles failed file upload and notifies the user.
     * @param {object} context - The bot context.
     * @param {string} error - The error message.
     */
    async fileUploadFailed(context, error) {
        const reply = MessageFactory.text(`<b>File upload failed.</b> Error: <pre>${error}</pre>`);
        reply.textFormat = 'xml';
        await context.sendActivity(reply);
    }

    /**
     * Processes an inline image by saving it and notifying the user.
     * @param {object} context - The bot context.
     */
    async processInlineImage(context) {
        const file = context.activity.attachments[0];
        const credentials = new MicrosoftAppCredentials(process.env.MicrosoftAppId, process.env.MicrosoftAppPassword);
        const botToken = await credentials.getToken();

        const config = {
            headers: { Authorization: `Bearer ${botToken}` },
            responseType: 'stream'
        };

        const fileName = await generateFileName(FILES_DIR);
        const filePath = path.join(FILES_DIR, fileName);
        await writeFile(file.contentUrl, config, filePath);

        const fileSize = await getFileSize(filePath);
        const reply = MessageFactory.text(`Image <b>${fileName}</b> of size <b>${fileSize}</b> bytes received and saved.`);
        const inlineAttachment = this.getInlineAttachment(fileName);

        reply.attachments = [inlineAttachment];
        await context.sendActivity(reply);
    }

    /**
     * Creates an inline attachment for the image.
     * @param {string} fileName - The name of the file.
     * @returns {object} The inline attachment object.
     */
    getInlineAttachment(fileName) {
        const imageData = fs.readFileSync(path.join(FILES_DIR, fileName));
        const base64Image = Buffer.from(imageData).toString('base64');

        return {
            name: fileName,
            contentType: 'image/png',
            contentUrl: `data:image/png;base64,${base64Image}`
        };
    }
}

module.exports.TeamsFileUploadBot = TeamsFileUploadBot;
