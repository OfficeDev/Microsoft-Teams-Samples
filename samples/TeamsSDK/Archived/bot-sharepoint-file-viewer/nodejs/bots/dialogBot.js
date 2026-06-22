// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
class DialogBot extends TeamsActivityHandler {
    /**
    *
    * @param {ConversationState} conversationState
    * @param {UserState} userState
    * @param {Dialog} dialog
    */
    constructor(conversationState, userState, dialog) {
        super();
        this.baseUrl = process.env.ApplicationBaseUrl;

        if (!conversationState) {
            throw new Error('[DialogBot]: Missing parameter. conversationState is required');
        }

        if (!userState) {
            throw new Error('[DialogBot]: Missing parameter. userState is required');
        }

        if (!dialog) {
            throw new Error('[DialogBot]: Missing parameter. dialog is required');
        }

        this.conversationState = conversationState;
        this.userState = userState;
        this.dialog = dialog;
        this.dialogState = this.conversationState.createProperty('DialogState');

        this.onMessage(async (context, next) => {
            console.log('Running dialog with Message Activity.');
            var userInput = context.activity.text;

            if (userInput.trim() == "viewfile" || userInput.trim() == "uploadfile" || userInput.trim() == "login" || userInput.trim() == "logout") {
                // Run the Dialog with the new message Activity.
                await this.dialog.run(context, this.dialogState);
            }
            else {
                await context.sendActivity("Type 'uploadfile' to upload file to sharepoint site or 'viewfile' to get card for file viewer");
            }

            await next();
        });
    }

    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;
        var taskInfo = {}; // TaskModuleTaskInfo

        if (cardTaskFetchId == "upload") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + "/Upload";
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Upload file";
        }

        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        await context.sendActivity("File uploaded successfully");

        return null;
    }

    /**
    * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
    */
    async run(context) {
        await super.run(context);

        // Save any state changes. The load happened during the execution of the Dialog.
        await this.conversationState.saveChanges(context, false);
        await this.userState.saveChanges(context, false);
    }
}

module.exports.DialogBot = DialogBot;