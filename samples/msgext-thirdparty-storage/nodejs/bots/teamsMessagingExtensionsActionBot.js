// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

require('dotenv').config();
const { TeamsActivityHandler, CardFactory, TeamsInfo, MessageFactory } = require('botbuilder');
const baseurl = process.env.BaseUrl;

class TeamsMessagingExtensionsActionBot extends TeamsActivityHandler {
	constructor() {
        super();
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

    async onTeamsTaskModuleFetch(context, taskModuleRequest) {
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
                        //url: `https://your-edit-file-url?name=${encodeURIComponent(file.name)}&type=${encodeURIComponent(file.type)}&size=${file.size}`
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
