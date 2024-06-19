const axios = require("axios");
const { TeamsActivityHandler, MessageFactory, CardFactory } = require("botbuilder");
const { checkCompliance } = require('./aiClient');
const fs = require('fs');
const path = require('path');
const { BlobServiceClient } = require('@azure/storage-blob');
const mammoth = require('mammoth');
const pdf = require('pdf-parse');
const config = require("./config");

class SearchApp extends TeamsActivityHandler {
  constructor() {
    super();
  }

  async blobGetAllDocumentsName(msFileName) {
    const blobServiceClient = BlobServiceClient.fromConnectionString(config.azure_Storage_Connection_String);
    const containerClient = blobServiceClient.getContainerClient(config.containerName);
    let blobs = containerClient.listBlobsFlat();
    for await (const blob of blobs) {
      const filename = blob.name;
      if (filename.endsWith('.pdf') || filename.endsWith('.docx') || filename.endsWith('.txt')) {
        if (filename.split('.')[0] === msFileName) {
          const blockBlobClient = containerClient.getBlockBlobClient(filename);
          const downloadBlockBlobResponse = await blockBlobClient.download(0);
          let fileContent;
          if (filename.endsWith('.docx')) {
            const downloadedWordContent = await this.streamToBufferDoc(downloadBlockBlobResponse.readableStreamBody);
            const docContent = await mammoth.extractRawText({ buffer: downloadedWordContent });
            fileContent = docContent.value;
          } else if (filename.endsWith('.txt')) {
            fileContent = await this.streamToString(downloadBlockBlobResponse.readableStreamBody);
          } else if (filename.endsWith('.pdf')) {
            const pdfContent = await this.streamToBufferPDF(downloadBlockBlobResponse.readableStreamBody);
            const pdfData = await pdf(pdfContent);
            fileContent = pdfData.text;
          }
          return fileContent;
        }
      }
    }
  }
  async streamToBufferPDF(readableStream) {
    return new Promise((resolve, reject) => {
      const chunks = [];
      readableStream.on('data', (data) => {
        chunks.push(data instanceof Buffer ? data : Buffer.from(data));
      });
      readableStream.on('end', () => {
        resolve(Buffer.concat(chunks));
      });
      readableStream.on('error', reject);
    });
  }

  async streamToString(readableStream) {
    return new Promise((resolve, reject) => {
      const chunks = [];
      readableStream.on("data", (data) => {
        chunks.push(data.toString());
      });
      readableStream.on("end", () => {
        resolve(chunks.join(""));
      });
      readableStream.on("error", reject);
    });
  }

  async streamToBufferDoc(readableStream) {
    return new Promise((resolve, reject) => {
      const chunks = [];
      readableStream.on('data', (data) => {
        chunks.push(data);
      });
      readableStream.on('end', () => {
        resolve(Buffer.concat(chunks));
      });
      readableStream.on('error', reject);
    });
  }
 
  async blobGetAllCheckListNames() {
    try {
      const blobServiceClient = BlobServiceClient.fromConnectionString(config.azure_Storage_Connection_String);
      const containerClient = blobServiceClient.getContainerClient(config.containerName);
      const blockBlobClient = containerClient.getBlockBlobClient("checklist.txt");
      const downloadBlockBlobResponse = await blockBlobClient.download(0);
      const checkListText = await this.streamToString(downloadBlockBlobResponse.readableStreamBody);
      const checkListContents = [];
      const lines = checkListText.split('\r\n');
      lines.forEach((line, index) => {
        checkListContents.push({ name: line, lineNumber: index + 1, content: line });
      });
      return checkListContents;
    } catch (error) {
      console.error('Error fetching checklist:', error);
      throw error;
    }
  }

  async handleTeamsMessagingExtensionQuery(context, query) {

    const msFileName = query.parameters[0].value;

    const downloadedContent = await this.blobGetAllDocumentsName(msFileName);
    const characterCount = downloadedContent.length;
    console.log(`Character count: ${characterCount}`);
    console.log('Downloaded Content:', downloadedContent);
    const predefinedDocument = downloadedContent;

    const checkListItems = await this.blobGetAllCheckListNames(msFileName);
    console.log('Check list names:', checkListItems);

    const complianceResult = await checkCompliance(checkListItems, predefinedDocument);

    const card = CardFactory.adaptiveCard({
      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
      "type": "AdaptiveCard",
      "version": "1.6",
      "body": [
        {
          "type": "Container",
          "style": "emphasis", // Adding a background color
          "items": [
            {
              "type": "TextBlock",
              "text": complianceResult,
              "weight": "bolder",
              "wrap": true,
              "size": "medium",
              "color": "accent", // Adding color
              "horizontalAlignment": "center", // Center alignment
              "spacing": "medium"
            }
          ],
          "padding": {
            "top": "small",
            "bottom": "small",
            "left": "medium",
            "right": "medium"
          }
        }
      ]
    });

    const preview = CardFactory.heroCard(
      "Compliance Check Result",
      complianceResult,
      null,
      null,
      {
        subtitle: "Tap to view details",
        text: complianceResult
      }
    );

    return {
      composeExtension: {
        type: 'result',
        attachmentLayout: 'list',
        attachments: [{ ...card, preview }]
      }
    };


  }

}

module.exports.SearchApp = SearchApp;
