// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Load environment variables from the .env file
require('dotenv').config();
const { TeamsActivityHandler, CardFactory, TeamsInfo, MessageFactory } = require('botbuilder');

// Base URL for retrieving file icons, set in the environment variables
const baseurl = process.env.BaseUrl;

/**
 * TeamsMessagingExtensionsActionBot
 * A bot implementation that handles messaging extension actions and fetch tasks in Microsoft Teams.
 * This bot is designed to dynamically generate file cards with appropriate icons
 * and handle task modules based on specific conditions.
 */
class TeamsMessagingExtensionsActionBot extends TeamsActivityHandler {
    constructor() {
        super();
    }

    /**
     * Get the icon URL for a file based on its extension.
     * Supports common file types like PDF, Word, Excel, PNG, JPG, and JPEG.
     * @param {string} fileName - The name of the file to get the icon for.
     * @returns {string} - URL of the corresponding file icon.
     */
    getFileIcon(fileName) {
        if (fileName.endsWith(".pdf")) {
            return `${baseurl}/icons/PDFIcons.png`; // URL for PDF icon
        } else if (fileName.endsWith(".doc") || fileName.endsWith(".docx")) {
            return `${baseurl}/icons/WordIcons.png`; // URL for Word icon
        } else if (fileName.endsWith(".xls") || fileName.endsWith(".xlsx")) {
            return `${baseurl}/icons/ExcelIcon.png`; // URL for Excel icon
        } else if (fileName.endsWith(".png")) {
            return `${baseurl}/icons/ImageIcon.png`; // URL for PNG icon
        } else if (fileName.endsWith(".jpg") || fileName.endsWith(".jpeg")) {
            return `${baseurl}/icons/ImageIcon.png`; // URL for JPG icon
        } else {
            return `${baseurl}/icons/ImageIcon.png`; // Default icon URL for other file types
        }
    }

    /**
     * Handle the 'submitAction' event from a Teams messaging extension.
     * Processes user-provided file data and creates an Adaptive Card with file details and icons.
     * @param {TurnContext} context - The bot's context object.
     * @param {object} action - The action details, including the user input.
     * @returns {object} - A response containing the card or an error message.
     */
    async handleTeamsMessagingExtensionSubmitAction(context, action) {
        try {
            const userInput = action.data;

            // Ensure that the input is an array of files
            if (!Array.isArray(userInput)) {
                throw new Error("Invalid input: Expected an array of files.");
            }

            // Generate an Adaptive Card displaying the files with corresponding icons
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
                                    url: this.getFileIcon(file.name), // Icon URL based on file type
                                    size: "Small" // Small size for the icon
                                }
                            ]
                        },
                        {
                            type: "Column",
                            width: "stretch",
                            items: [
                                {
                                    type: "TextBlock",
                                    text: file.name, // File name
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

            // Return a meaningful error response
            return {
                composeExtension: {
                    type: "message",
                    text: `An error occurred: ${error.message}`
                }
            };
        }
    }

    /**
     * Handle the 'fetchTask' event for Teams messaging extensions.
     * Displays a task module based on specific conditions.
     * @param {TurnContext} context - The bot's context object.
     * @param {object} action - The action details, including activity value and command context.
     * @returns {object} - A response with a task module or an error message.
     */
    async handleTeamsMessagingExtensionFetchTask(context, action) {
        const value = context.activity.value;

        // Check for conditions to display a task module
        if ((value.messagePayload?.replyToId === '' || value.messagePayload?.replyToId == null) && value.commandContext === 'thirdParty') {
            return {
                task: {
                    type: 'continue',
                    value: {
                        width: 700,
                        height: 450,
                        title: 'Task module WebView', // Title of the task module
                        url: `${baseurl}/customForm` // URL to load the task module
                    }
                }
            };
        } else {
            // If conditions are not met, return a message
            return {
                task: {
                    type: 'message',
                    value: 'The conditions for displaying the task module are not met.'
                }
            };
        }
    }
}

// Export the bot class for use in the application
module.exports.TeamsMessagingExtensionsActionBot = TeamsMessagingExtensionsActionBot;
