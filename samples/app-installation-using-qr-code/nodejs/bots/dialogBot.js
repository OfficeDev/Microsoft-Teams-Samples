// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');
const { TaskModuleResponseFactory } = require('../models/taskModuleResponseFactory');
const { SimpleGraphClient } = require('../simpleGraphClient');
const Token_State_Property = 'TokenData';
class DialogBot extends TeamsActivityHandler {
    /**
    *
    * @param {ConversationState} conversationState
    * @param {UserState} userState
    * @param {Dialog} dialog
    */
    constructor(conversationState, userState, dialog) {
        super();

        if (!conversationState) {
            throw new Error('[DialogBot]: Missing parameter. conversationState is required');
        }

        if (!userState) {
            throw new Error('[DialogBot]: Missing parameter. userState is required');
        }

        if (!dialog) {
            throw new Error('[DialogBot]: Missing parameter. dialog is required');
        }

        this.baseUrl = process.env.BaseUrl;
        this.conversationState = conversationState;
        this.userState = userState;
        this.dialog = dialog;
        this.dialogState = this.conversationState.createProperty('DialogState');
        this.conversationDataAccessor = this.conversationState.createProperty(Token_State_Property); 
        
        this.onMessage(async (context, next) => {
            console.log('Running dialog with Message Activity.');
            // Run the Dialog with the new message Activity.
            await this.dialog.run(context, this.dialogState);

            await next();
        });
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

    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;
        var taskInfo = {}; // TaskModuleTaskInfo

        if (cardTaskFetchId == "generate") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + '/generate';
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Generate QR code";
        }
        else if (cardTaskFetchId == "install") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + '/install';
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Install App";
        }

        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    async handleTeamsTaskModuleSubmit(context, taskModuleRequest){

        const appId = taskModuleRequest.data.appId;
        const teamId = taskModuleRequest.data.teamId;
        var TokenState = await this.conversationDataAccessor.get(context, {});

        if(TokenState.token == null){
            await this.dialog.run(context, this.dialogState);
        }
        else{
            const client = new SimpleGraphClient(TokenState.token);
            await client.installApp(appId,teamId);
            await context.sendActivity("App added successfully");
        }
        
        return null;
    }
}

module.exports.DialogBot = DialogBot;