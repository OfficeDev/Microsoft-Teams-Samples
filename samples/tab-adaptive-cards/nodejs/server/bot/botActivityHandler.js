// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
    }

    async onInvokeActivity(context) {
        console.log('Activity: ', context.activity.name);
        const user = context.activity.from;
        if (context.activity.name === 'tab/fetch') {
            console.log('Trying to fetch tab content: ', user);
            return adaptiveCards.createFetchResponse();
        } else if (context.activity.name === 'tab/submit'){
            console.log('Trying to submit tab content: ', user);
            return adaptiveCards.createSubmitResponse();
        }
    }
}

module.exports.BotActivityHandler = BotActivityHandler;