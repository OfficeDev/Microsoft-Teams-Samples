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
        const userInput = action.data;
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
                    },
                    {
                        type: "Column",
                        width: "auto",
                        items: [
                            {
                                type: "ActionSet",
                                actions: [
                                    {
                                        type: "Action.Submit",
                                        title: "Edit",
                                        data: {
                                            action: "editFile",
                                            file: {
                                                name: file.name,
                                                type: file.type,
                                                size: file.size
                                            }
                                        }
                                    }
                                ]
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
    }

    async handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const { action, file } = taskModuleRequest.data;
    
        if (action === "editFile") {
            return {
                task: {
                    type: "continue",
                    value: {
                        title: "Edit File",
                        width: 550,
                        height: 400,
                        url: `${ baseurl }/customForm`
                    }
                }
            };
        }
    }

    async handleTeamsMessagingExtensionFetchTask(context, action) {
        console.log("Context", context);
        const value = context.activity.value;
        const channelData = context.activity.channelData;
        console.log('Source Info:', channelData.source);
        console.log("Command ID:", value.commandId);
        console.log("Command Context:", value.commandContext);
        console.log("Context Object:", value.context);
        console.log("Message Payload:", value.messagePayload);
        return {
            task: {
                type: 'continue',
                value: {
                    width: 700,
                    height: 450,
                    title: 'Task module WebView',
                    url: `${ baseurl }/customForm`
                }
            }
        };
    }
}

module.exports.TeamsMessagingExtensionsActionBot = TeamsMessagingExtensionsActionBot;
