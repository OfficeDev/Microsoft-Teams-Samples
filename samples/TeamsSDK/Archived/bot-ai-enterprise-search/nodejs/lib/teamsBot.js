"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.TeamsBot = void 0;
const botbuilder_1 = require("botbuilder");
const welcome_json_1 = __importDefault(require("./adaptiveCards/welcome.json"));
const adaptivecards_tools_1 = require("@microsoft/adaptivecards-tools");
const index_1 = require("./index");
const fs = require('fs');
const path = require('path');
const axios = require('axios');
const os = require('os');
const { writeFile } = require('./fileService');
class TeamsBot extends botbuilder_1.TeamsActivityHandler {
    constructor() {
        super();
        this.onMessage(async (context, next) => {
            console.log("Running with Message Activity.");
            let txt = context.activity.text;
            const removedMentionText = botbuilder_1.TurnContext.removeRecipientMention(context.activity);
            if (removedMentionText) {
                // Remove the line break
                txt = removedMentionText.toLowerCase().replace(/\n|\r/g, "").trim();
            }
            const attachments = context.activity.attachments;
            const imageRegex = /image\/.*/;
            let fileName = "";
            let isFileUploadedSuccessfully = false;
            let isUserQuery = false;
            let isImage = false;
            if (attachments && attachments[0] && attachments[0].contentType === 'application/vnd.microsoft.teams.file.download.info') {
                const file = attachments[0];
                const downloadUrl = file.content.downloadUrl;
                fileName = file.name;
                const localFilePath = path.join(os.tmpdir(), fileName);
                console.log("localFilePath: " + localFilePath);
                if (file.name.includes(".pdf")) {
                    try {
                        await writeFile(file.content.downloadUrl, localFilePath);
                        // Create embeddings for pdf file contents.
                        var fileContents = await this.ReadPdfContents(localFilePath);
                        var contents = fileContents.replace(/[\n\r]+/g, '');
                        if (contents != null && contents != "" && contents != "undefined") {
                            // Save file as blob in storage container.
                            await (0, index_1.uploadPdfFileToBlobAsync)(localFilePath, fileName, fileContents);
                            isFileUploadedSuccessfully = true;
                        }
                    }
                    catch (ex) {
                        console.log(ex);
                    }
                    // Delete the file from project folder, once the file is successfully uploaded to blob and embeddings are created.
                    fs.unlinkSync(localFilePath);
                }
                else if (file.name.includes(".txt")) {
                    try {
                        // Fetch text file contents.
                        const fileContentsAsString = await this.fetchTextFileContentAsString(downloadUrl);
                        // Save file as blob in Azure storage container.
                        await (0, index_1.uploadTextFileToBlobAsync)(fileContentsAsString, fileName);
                        // Create embeddings for text file contents.
                        isFileUploadedSuccessfully = true;
                    }
                    catch (ex) {
                        console.log(ex);
                    }
                }
                else {
                    await context.sendActivity("**File upload is not supported for this file type. Supported types are: PDF/Text file.**");
                    isImage = true;
                }
            }
            else if (attachments && attachments[0] && imageRegex.test(attachments[0].contentType)) {
                await context.sendActivity("**Image upload is not supported. Please upload text or pdf file.**");
                isImage = true;
            }
            else {
                await context.sendActivity("<i>Your query: " + context.activity.text + "</i>");
                await context.sendActivity("<i>Please wait while I look up answer to your query...</i>");
                isUserQuery = true;
                await (0, index_1.generateEmbeddingForUserPromptAsync)(context, context.activity.text);
            }
            if (isFileUploadedSuccessfully) {
                const reply = botbuilder_1.MessageFactory.text(`<i><b>${fileName}</b> received and saved successfully.</i>`);
                reply.textFormat = 'xml';
                await context.sendActivity(reply);
            }
            else if (isUserQuery) {
            }
            else if (isImage) {
                // No action required.
            }
            else {
                const reply = botbuilder_1.MessageFactory.text(`Failed to save your file: <b>${fileName}</b>. Please try to upload it again or please try with another file.`);
                reply.textFormat = 'xml';
                await context.sendActivity(reply);
            }
            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let cnt = 0; cnt < membersAdded.length; cnt++) {
                if (membersAdded[cnt].id) {
                    const card = adaptivecards_tools_1.AdaptiveCards.declareWithoutData(welcome_json_1.default).render();
                    await context.sendActivity({ attachments: [botbuilder_1.CardFactory.adaptiveCard(card)] });
                    break;
                }
            }
            await next();
        });
    }
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
        const reply = botbuilder_1.MessageFactory.text(`<b>File uploaded.</b> Your file <b>${fileConsentCardResponse.uploadInfo.name}</b> is ready to download`);
        reply.textFormat = 'xml';
        reply.attachments = [asAttachment];
        await context.sendActivity(reply);
    }
    // Function to fetch text file content as a string from a web URL.
    async fetchTextFileContentAsString(url) {
        try {
            const response = await axios.get(url, { responseType: 'string', });
            return response.data;
        }
        catch (error) {
            throw new Error(`Failed to fetch file content: ${error}`);
        }
    }
    ;
    // Function to fetch text file content as a string from a web URL.
    async ReadPdfContents(localFilePath) {
        try {
            const pdfParse = require('pdf-parse');
            const readFileSync = fs.readFileSync(localFilePath);
            const pdfExtract = await pdfParse(readFileSync);
            return pdfExtract.text;
        }
        catch (error) {
            throw new Error(`Failed to fetch file content: ${error}`);
        }
    }
}
exports.TeamsBot = TeamsBot;
//# sourceMappingURL=teamsBot.js.map