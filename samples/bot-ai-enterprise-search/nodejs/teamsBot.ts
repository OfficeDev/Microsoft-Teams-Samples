import {
  TeamsActivityHandler,
  CardFactory,
  TurnContext,
  AdaptiveCardInvokeValue,
  AdaptiveCardInvokeResponse,
  MessageFactory,
} from "botbuilder";
import rawWelcomeCard from "./adaptiveCards/welcome.json";
import rawLearnCard from "./adaptiveCards/learn.json";
import { AdaptiveCards } from "@microsoft/adaptivecards-tools";
import { uploadTextFileToBlobAsync, uploadPdfFileToBlobAsync, generateEmbeddingForUserPromptAsync } from "./index";

const fs = require('fs');
const path = require('path');
const axios = require('axios');
const os = require('os');
//const { MicrosoftAppCredentials } = require('botframework-connector');
const { geneFileName, getFileSize, writeFile } = require('./fileService');
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

       // localFilePath = path.join('./Files/', file.name); //path.join(FILES_DIR, file.name);
       localFilePath = path.join(os.tmpdir(), fileName); 
       console.log("localFilePath: " + localFilePath);
       var isFileUploadedSuccessfully = false;
        var isUserQuery = false;
        var isImage = false;
        var result;

        if (file.name.includes(".pdf")) {
          try {

            await writeFile(file.content.downloadUrl, config, localFilePath);

            // Create embeddings for pdf file contents.
            var contents = await this.ReadPdfContents(localFilePath);

            // Save file as blob in storage container.
            await uploadPdfFileToBlobAsync(localFilePath, fileName, contents);
            isFileUploadedSuccessfully = true;
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
      }
      else if (isImage) {
        // No action required.
      }
      else {
        const reply = MessageFactory.text(`Failed to save your file: <b>${fileName}</b>. Please try to upload it again.`);
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

  // Invoked when an action is taken on an Adaptive Card. The Adaptive Card sends an event to the Bot and this
  // method handles that event.
  async onAdaptiveCardInvoke(
    context: TurnContext,
    invokeValue: AdaptiveCardInvokeValue
  ): Promise<AdaptiveCardInvokeResponse> {
    // The verb "userlike" is sent from the Adaptive Card defined in adaptiveCards/learn.json
    if (invokeValue.action.verb === "userlike") {
      this.likeCountObj.likeCount++;
      const card = AdaptiveCards.declare<DataInterface>(rawLearnCard).render(this.likeCountObj);
      await context.updateActivity({
        type: "message",
        id: context.activity.replyToId,
        attachments: [CardFactory.adaptiveCard(card)],
      });
      return { statusCode: 200, type: undefined, value: undefined };
    }
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
      // console.log('File content: ', pdfExtract.text)
      // console.log('Total pages: ', pdfExtract.numpages)
      // console.log('All content: ', pdfExtract.info)
      return pdfExtract.text;

    } catch (error) {
      throw new Error(`Failed to fetch file content: ${error}`);
    }
  };
}
