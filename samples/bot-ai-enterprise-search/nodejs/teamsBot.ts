import {
  TeamsActivityHandler,
  CardFactory,
  TurnContext,
  MessageFactory,
} from "botbuilder";
import rawWelcomeCard from "./adaptiveCards/welcome.json";
import { AdaptiveCards } from "@microsoft/adaptivecards-tools";
import { uploadTextFileToBlobAsync, uploadPdfFileToBlobAsync, generateEmbeddingForUserPromptAsync } from "./index";

const fs = require('fs');
const path = require('path');
const axios = require('axios');
const { writeFile } = require('./fileService');
const FILES_DIR = 'files';

export interface DataInterface {
  likeCount: number;
}

export class TeamsBot extends TeamsActivityHandler {
  // record the likeCount
  likeCountObj: { likeCount: number };

  constructor() {
    super();

    this.likeCountObj = { likeCount: 0 };

    this.onMessage(async (context, next) => {
      console.log("Running with Message Activity.");

      let txt = context.activity.text;
      const removedMentionText = TurnContext.removeRecipientMention(context.activity);
      if (removedMentionText) {
        // Remove the line break
        txt = removedMentionText.toLowerCase().replace(/\n|\r/g, "").trim();
      }

      const attachments = context.activity.attachments;
      const imageRegex = /image\/.*/;
      var downloadUrl = "";
      var fileName = "";
      var localFilePath = "";

      if (attachments && attachments[0] && attachments[0].contentType === 'application/vnd.microsoft.teams.file.download.info') {
        const file = attachments[0];
        const config = {
          responseType: 'stream'
        };

        downloadUrl = file.content.downloadUrl;
        fileName = file.name;

        localFilePath = path.join(FILES_DIR, file.name);
        var isFileUploadedSuccessfully = false;
        var isUserQuery = false;
        var isImage = false;

        if (file.name.includes(".pdf")) {
          try {
            await writeFile(file.content.downloadUrl, config, localFilePath);

            // Create embeddings for pdf file contents.
            var fileContents = await this.ReadPdfContents(localFilePath);
            var contents = fileContents.replace(/[\n\r]+/g, '');

            if (contents != null && contents != "" && contents != "undefined") {
              // Save file as blob in storage container.
              await uploadPdfFileToBlobAsync(localFilePath, fileName, fileContents);
              isFileUploadedSuccessfully = true;
            }
          } catch (ex) {
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
            await uploadTextFileToBlobAsync(fileContentsAsString, fileName);

            // Create embeddings for text file contents.
            isFileUploadedSuccessfully = true;
          } catch (ex) {
            console.log(ex);
          }
        }
        else {
          await context.sendActivity("**File upload is not supported for this file type. Supported types are: PDF/Text file.**");
          isImage = true;
        }
      } else if (attachments && attachments[0] && imageRegex.test(attachments[0].contentType)) {
        // await this.processInlineImage(context);
        await context.sendActivity("**Image upload is not supported. Please upload text or pdf file.**");
        isImage = true;
      } else {
        await context.sendActivity("<i>Your query: " + context.activity.text + "</i>");
        await context.sendActivity("<i>Please wait while I look up answer to your query...</i>");
        isUserQuery = true;
        await generateEmbeddingForUserPromptAsync(context, context.activity.text);
      }

      if (isFileUploadedSuccessfully) {
        const reply = MessageFactory.text(`<i><b>${fileName}</b> received and saved successfully.</i>`);
        reply.textFormat = 'xml';
        await context.sendActivity(reply);
      }
      else if (isUserQuery) {
        const reply = MessageFactory.text(`<i>I hope I have answered your query. Let me know if you have any further questions.</i>`);
        reply.textFormat = 'xml';
        await context.sendActivity(reply);
      }
      else if (isImage) {
        // No action required.
      }
      else {
        const reply = MessageFactory.text(`Failed to save your file: <b>${fileName}</b>. Please try to upload it again or please try with another file.`);
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
          const card = AdaptiveCards.declareWithoutData(rawWelcomeCard).render();
          await context.sendActivity({ attachments: [CardFactory.adaptiveCard(card)] });
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

    const reply = MessageFactory.text(`<b>File uploaded.</b> Your file <b>${fileConsentCardResponse.uploadInfo.name}</b> is ready to download`);
    reply.textFormat = 'xml';
    reply.attachments = [asAttachment];
    await context.sendActivity(reply);
  }

  // Function to fetch text file content as a string from a web URL.
  async fetchTextFileContentAsString(url: string): Promise<string> {
    try {
      const response = await axios.get(url, { responseType: 'string', });
      return response.data;
    } catch (error) {
      throw new Error(`Failed to fetch file content: ${error}`);
    }
  };

  // Function to fetch text file content as a string from a web URL.
  async ReadPdfContents(localFilePath: any): Promise<any> {
    try {
      // Include fs module.
      var fs = require('fs');
      const pdfParse = require('pdf-parse')
      let readFileSync = fs.readFileSync(localFilePath)
      let pdfExtract = await pdfParse(readFileSync)

      return pdfExtract.text;
    } catch (error) {
      throw new Error(`Failed to fetch file content: ${error}`);
    }
  };
}
