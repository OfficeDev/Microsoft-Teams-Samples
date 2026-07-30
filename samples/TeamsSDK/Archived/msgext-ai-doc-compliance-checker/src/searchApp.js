/**
 * SearchApp Class
 * Handles Microsoft Teams message extension activities including compliance checking,
 * checklist preparation, and document parsing (PDF and Word).
 */

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

  // Fetch document content from Azure Blob Storage
  async blobGetAllDocumentsName(msFileName) {
    const blobServiceClient = BlobServiceClient.fromConnectionString(config.azure_Storage_Connection_String);
    const containerClient = blobServiceClient.getContainerClient(config.azure_containerName);
    const blobs = containerClient.listBlobsFlat();

    for await (const blob of blobs) {
      const filename = blob.name;

      // Check if file is PDF, DOCX, or TXT
      if (!filename.match(/\.(pdf|docx|txt)$/i)) continue;

      if (filename.split('.')[0] === msFileName) {
        const blockBlobClient = containerClient.getBlockBlobClient(filename);
        const downloadBlockBlobResponse = await blockBlobClient.download(0);

        let fileContent;

        try {
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
        } catch (error) {
          console.error('Error processing document:', error);
          throw error;
        }
      }
    }
  }

  // Convert readable stream to buffer
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

  // Convert readable stream to string
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

  // Fetch checklist from Azure Blob Storage
  async blobGetAllCheckListNames(checkListFileName) {
    try {
      const blobServiceClient = BlobServiceClient.fromConnectionString(config.azure_Storage_Connection_String);
      const containerClient = blobServiceClient.getContainerClient(config.azure_containerName);
      const fileName = checkListFileName || config.CheckListFileName;
      const blockBlobClient = containerClient.getBlockBlobClient(fileName);
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

  // Generate adaptive card data based on compliance results
  async generateAdaptiveCardData(complianceResult) {
    const lines = complianceResult.trim().split('\n').map(line => line.trim().replace(/^- /, ''));
    return {
      descriptionStatus: lines.map(line => {
        const [txtDescription, statusPart] = line.split(':').map(part => part.trim());
        const status = statusPart?.includes('Yes') ? 'Yes' : 'No';
        return {
          txtDescription,
          status,
          verifyContent: statusPart,
          contentStatus: status === 'Yes' ? '🟢' : '🔴',
        };
      })
    };
  }

  // Get parameter value by name
  async getParameterByName(parameters, name) {
    const param = parameters.find(p => p.name === name);
    return param ? param.value : '';
  }

  // Handle messaging extension queries in Teams
  async handleTeamsMessagingExtensionQuery(context, query) {
    const { parameters } = query;

    const msFileName = await this.getParameterByName(parameters, "ProposalDocument");
    const checkListFileName = await this.getParameterByName(parameters, "PolicyGuidelineDocument");

    const downloadedContent = await this.blobGetAllDocumentsName(msFileName);
    const predefinedDocument = downloadedContent;
    const checkListItems = await this.blobGetAllCheckListNames(checkListFileName);

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
                          "title": "▼",
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
