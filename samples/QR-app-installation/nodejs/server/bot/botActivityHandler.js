// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');
const { TaskModuleResponseFactory } = require('../models/taskModuleResponseFactory');

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        this.onMessage(async (context, next) => {
            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.getAdaptiveCardUserDetails())] });

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
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
};

module.exports.BotActivityHandler = BotActivityHandler;