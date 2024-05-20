// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');

let conversationArray = [];

class PersonalBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            let inputMessage = context.activity.text;
            conversationArray = [];
            conversationArray.push(inputMessage);
            exports.PersonalBot.conversationArray = conversationArray;
            await context.sendActivity(`Selected Color  : ${inputMessage}`);
        });
    }
}

module.exports.PersonalBot = PersonalBot;

