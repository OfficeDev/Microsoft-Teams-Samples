const axios = require("axios");
const { TeamsActivityHandler, MessageFactory, CardFactory } = require("botbuilder");
const { checkCompliance, prepareChecklistItems } = require('./aiClient');
const { BlobServiceClient } = require('@azure/storage-blob');
const pdf = require('pdf-parse');
const config = require("./config");
const WordExtractor = require("word-extractor");

class SearchApp extends TeamsActivityHandler {
  constructor() {
    super();
  }

  // Method to fetch document content from Azure Blob Storage
  async blobGetAllDocumentsName(msFileName) {
    const blobServiceClient = BlobServiceClient.fromConnectionString(config.azure_Storage_Connection_String);
    const containerClient = blobServiceClient.getContainerClient(config.containerName);
    let blobs = containerClient.listBlobsFlat();
    
    // Iterate through blobs to find the document
    for await (const blob of blobs) {
      const filename = blob.name;
      
      // Check if the file is of type PDF, DOCX, or TXT
      if (filename.endsWith('.pdf') || filename.endsWith('.docx') || filename.endsWith('.txt')) {
        if (filename.split('.')[0] === msFileName) {
          const blockBlobClient = containerClient.getBlockBlobClient(filename);
          const downloadBlockBlobResponse = await blockBlobClient.download(0);
          let fileContent;

          // Process DOCX file
          if (filename.endsWith('.docx')) {
            try {
              const downloadedWordContent = await this.streamToBuffer(downloadBlockBlobResponse.readableStreamBody);
              const extractor = new WordExtractor();
              const extracted = await extractor.extract(downloadedWordContent);
              fileContent = extracted.getBody();
            }
            catch (error) {
              console.error('Error fetching checklist:', error);
              throw error;
            }
          } 
          // Process TXT file
          else if (filename.endsWith('.txt')) {
            fileContent = await this.streamToString(downloadBlockBlobResponse.readableStreamBody);
          } 
          // Process PDF file
          else if (filename.endsWith('.pdf')) {
            const pdfContent = await this.streamToBuffer(downloadBlockBlobResponse.readableStreamBody);
            const pdfData = await pdf(pdfContent);
            fileContent = pdfData.text;
          }
          return fileContent;
        }
      }
    }
  }

  // Helper function to convert readable stream to buffer.
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

  // Helper function to convert readable stream to string.
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

  // Method to fetch checklist names from Azure Blob Storage
  async blobGetAllCheckListNames(checkListFileName) {
    try {
      const blobServiceClient = BlobServiceClient.fromConnectionString(config.azure_Storage_Connection_String);
      const containerClient = blobServiceClient.getContainerClient(config.containerName);
      let blockBlobClient;

      // Determine which checklist file to use
      if (checkListFileName != "") {
        blockBlobClient = containerClient.getBlockBlobClient(config.CheckListFileName);
      } else {
        blockBlobClient = containerClient.getBlockBlobClient(config.CheckListFileName);
      }

      const downloadBlockBlobResponse = await blockBlobClient.download(0);
      const guidelinesContent = await this.streamToBuffer(downloadBlockBlobResponse.readableStreamBody);
      const guidelinesText = await pdf(guidelinesContent);
      const checkListResult = await prepareChecklistItems(guidelinesText.text);
      
      // Process checklist items
      const checkListContents = [];
      const lines = checkListResult.split('\n');
      lines.forEach((line, index) => {
        checkListContents.push({ name: line, lineNumber: index + 1, content: line });
      });
      return checkListContents;
    } catch (error) {
      console.error('Error fetching checklist:', error);
      throw error;
    }
  }

  // Method to generate adaptive card data based on compliance results
  async generateAdaptiveCardData(complianceResult) {
    const lines = complianceResult.trim().split('\n').map(line => line.trim().replace(/^- /, ''));
    const items = lines.map(line => {
      const [txtDescription, statusPart] = line.split(':').map(part => part.trim());
      const status = statusPart.includes('Yes') ? 'Yes' : 'No';
      const verifyContent = statusPart;
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

  // Method to get a parameter value by its name
  async getParameterByName(parameters, name) {
    const param = parameters.find(p => p.name === name);
    return param ? param.value : '';
  }

  // Method to handle messaging extension queries in Teams
  async handleTeamsMessagingExtensionQuery(context, query) {
    const { parameters } = query;
    const msFileName = await this.getParameterByName(parameters, "ComplianceCheckerDoc");
    const checkListFileName = await this.getParameterByName(parameters, "CheckListFileName");
    const downloadedContent = await this.blobGetAllDocumentsName(msFileName);
    console.log('Downloaded Content:', downloadedContent);
    const predefinedDocument = downloadedContent;
    const checkListItems = await this.blobGetAllCheckListNames(checkListFileName);
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
                          "title": "▲▼",
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
                  "text": item.verifyContent,
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
