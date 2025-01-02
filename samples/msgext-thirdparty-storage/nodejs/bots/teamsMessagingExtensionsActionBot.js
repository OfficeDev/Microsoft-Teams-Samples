// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

require('dotenv').config();
const { TeamsActivityHandler, CardFactory, TeamsInfo, MessageFactory } = require('botbuilder');
const baseurl = process.env.BaseUrl;

class TeamsMessagingExtensionsActionBot extends TeamsActivityHandler {
    constructor() {
        super();
    }

    // Method to get the image icon for the file based on its extension
    getFileIcon(fileName) {
        if (fileName.endsWith(".pdf")) {
            return `${ baseurl }/icons/PDFIcons.png`; // Replace with actual PDF icon URL
        } else if (fileName.endsWith(".doc") || fileName.endsWith(".docx")) {
            return `${ baseurl }/icons/WordIcons.png`; // Replace with actual Word icon URL
        } else if (fileName.endsWith(".xls") || fileName.endsWith(".xlsx")) {
            return `${ baseurl }/icons/ExcelIcon.png`; // Replace with actual Excel icon URL
        } else if (fileName.endsWith(".png")) {
            return `${ baseurl }/icons/ImageIcon.png`; // Replace with actual PNG icon URL
        } else if (fileName.endsWith(".jpg") || fileName.endsWith(".jpeg")) {
            return `${ baseurl }/icons/ImageIcon.png`; // Replace with actual JPG icon URL
        } else {
            return `${ baseurl }/icons/ImageIcon.png`; // Default icon URL
        }
    }

    async handleTeamsMessagingExtensionSubmitAction(context, action) {
        try {
            const userInput = action.data;
    
            // Validate that userInput is an array
            if (!Array.isArray(userInput)) {
                throw new Error("Invalid input: Expected an array of files.");
            }
    
            const card = CardFactory.adaptiveCard({
                type: "AdaptiveCard",
                version: "1.4",
                body: userInput.map(file => ({
                    type: "ColumnSet",
                    columns: [
                        {
                            type: "Column",
                            width: "auto",
                            items: [
                                {
                                    type: "Image",
                                    url: this.getFileIcon(file.name), // Get the file icon URL
                                    size: "Small" // Adjust the size of the icon
                                }
                            ]
                        },
                        {
                            type: "Column",
                            width: "stretch",
                            items: [
                                {
                                    type: "TextBlock",
                                    text: file.name,
                                    wrap: true,
                                    weight: "Default",
                                    size: "Medium"
                                }
                            ]
                        }
                    ]
                }))
            });
    
            return {
                composeExtension: {
                    type: "result",
                    attachmentLayout: "list",
                    attachments: [card]
                }
            };
        } catch (error) {
            // Log the error for debugging
            console.error("Error handling Teams messaging extension action:", error);
    
            // Return an error response
            return {
                composeExtension: {
                    type: "message",
                    text: `An error occurred: ${error.message}`
                }
            };
        }
    }

    async handleTeamsMessagingExtensionFetchTask(context, action) {
        const value = context.activity.value;
    
        // Check for specific conditions
        if ((value.messagePayload?.replyToId === '' || value.messagePayload?.replyToId == null) && value.commandContext === 'thirdParty') {
            return {
                task: {
                    type: 'continue',
                    value: {
                        width: 700,
                        height: 450,
                        title: 'Task module WebView',
                        url: `${baseurl}/customForm`
                    }
                }
            };
        } else {
            // Handle other cases
            return {
                task: {
                    type: 'message',
                    value: 'The conditions for displaying the task module are not met.'
                }
            };
        }
    }
    
}

module.exports.TeamsMessagingExtensionsActionBot = TeamsMessagingExtensionsActionBot;
