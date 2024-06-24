const axios = require("axios");
const { TeamsActivityHandler, MessageFactory, CardFactory } = require("botbuilder");
const { checkCompliance } = require('./aiClient');
const { BlobServiceClient } = require('@azure/storage-blob');
const pdf = require('pdf-parse');
const config = require("./config");
const WordExtractor = require("word-extractor");

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
            const downloadedWordContent = await this.streamToBuffer(downloadBlockBlobResponse.readableStreamBody);
            const extractor = new WordExtractor();
            const extracted = await extractor.extract(downloadedWordContent);
            fileContent = extracted.getBody();
          } else if (filename.endsWith('.txt')) {
            fileContent = await this.streamToString(downloadBlockBlobResponse.readableStreamBody);
          } else if (filename.endsWith('.pdf')) {
            const pdfContent = await this.streamToBuffer(downloadBlockBlobResponse.readableStreamBody);
            const pdfData = await pdf(pdfContent);
            fileContent = pdfData.text;
          }
          return fileContent;
        }
      }
    }
  }
  async streamToBuffer(readableStream) {
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

  async generateAdaptiveCardData(complianceResult) {
    let status = '';
    let verifyContent = '';
    const lines = complianceResult.trim().split('\n').map(line => line.trim());
    const items = lines.map(line => {
      const [txtDescription, statusPart] = line.split(':').map(part => part.trim());
      if (statusPart.includes(',')) {
        [status, verifyContent] = statusPart.split(',').map(part => part.trim());
      } else {
        status = statusPart.trim();
      }
      return {
        txtDescription,
        status,
        verifyContent,
        contentStatus: status === 'Yes' ? '🟢' : '🔴',
      };
    });
    return {
      descriptionStatus: items
    };
  }


  async handleTeamsMessagingExtensionQuery(context, query) {

    const msFileName = query.parameters[0].value;

    const downloadedContent = await this.blobGetAllDocumentsName(msFileName);
    console.log('Downloaded Content:', downloadedContent);
    const predefinedDocument = downloadedContent;

    const checkListItems = await this.blobGetAllCheckListNames(msFileName);
    console.log('Check list names:', checkListItems);

    const complianceResult = await checkCompliance(checkListItems, predefinedDocument);

    const resultAdaptiveCardData = await this.generateAdaptiveCardData(complianceResult);

    const card = CardFactory.adaptiveCard({
      "type": "AdaptiveCard",
      "body": [
        {
          "type": "Container",
          "style": "emphasis",
          "items": [
            {
              "type": "ColumnSet",
              "columns": [
                {
                  "type": "Column",
                  "items": [
                    {
                      "type": "TextBlock",
                      "size": "large",
                      "weight": "bolder",
                      "text": "COMPLIANCE CHECKER",
                      "wrap": true
                    }
                  ],
                  "width": "stretch"
                },
                {
                  "type": "Column",
                  "items": [
                    {
                      "type": "TextBlock",
                      "size": "large",
                      "weight": "bolder",
                      "text": `PASSED: ${resultAdaptiveCardData.descriptionStatus.filter(item => item.status === "Yes").length}`,
                      "wrap": true
                    }
                  ],
                  "width": "auto"
                }
              ]
            }
          ],
          "bleed": true
        },
        {
          "type": "Container",
          "spacing": "Large",
          "style": "emphasis",
          "items": [
            {
              "type": "ColumnSet",
              "columns": [
                {
                  "type": "Column",
                  "items": [
                    {
                      "type": "TextBlock",
                      "weight": "Bolder",
                      "text": "CHECKLIST ITEMS",
                      "wrap": true
                    }
                  ],
                  "width": "stretch"
                },
                {
                  "type": "Column",
                  "items": [
                    {
                      "type": "TextBlock",
                      "weight": "Bolder",
                      "text": "STATUS",
                      "wrap": true
                    }
                  ],
                  "width": "auto"
                }
              ]
            }
          ],
          "bleed": true
        },
        ...resultAdaptiveCardData.descriptionStatus.map((item, index) => ({
          "type": "Container",
          "items": [
            {
              "type": "ColumnSet",
              "columns": [
                {
                  "type": "Column",
                  "spacing": "Medium",
                  "items": [
                    {
                      "type": "TextBlock",
                      "text": item.txtDescription,
                      "wrap": true
                    }
                  ],
                  "width": "stretch"
                },
                {
                  "type": "Column",
                  "items": [
                    {
                      "type": "TextBlock",
                      "text": item.status + item.contentStatus,
                      "wrap": true
                    }
                  ],
                  "width": "auto"
                },
                {
                  "type": "Column",
                  "items": [
                    {
                      "type": "ActionSet",
                      "actions": [
                        {
                          "type": "Action.ToggleVisibility",
                          "targetElements": [
                            {
                              "elementId": `textToShowHide_${index}`
                            }
                          ]
                        }
                      ]
                    },
                  ],
                  "width": "auto"
                }
              ]
            },
            {
              "type": "Container",
              "id": `textToShowHide_${index}`,
              "isVisible": false,
              "items": [
                {
                  "type": "TextBlock",
                  "text": item.verifyContent || "No additional details available.",
                  "isSubtle": true,
                  "wrap": true
                }
              ]
            }
          ]
        })),
      ],
      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
      "version": "1.5"
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
