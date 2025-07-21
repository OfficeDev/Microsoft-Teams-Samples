// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TeamsInfo } = require("botbuilder");
const adaptiveCards = require('../models/adaptiveCard');

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        this.onMembersAdded(this.handleMembersAdded.bind(this));
        this.onMessage(this.handleMessage.bind(this));
    }

    /**
     * Handles the event when members are added to the conversation.
     * @param {TurnContext} context - The context object for the turn.
     * @param {function} next - The next middleware function to call.
     */
    async handleMembersAdded(context, next) {
        const membersAdded = context.activity.membersAdded;
        for (const member of membersAdded) {
            if (member.id !== context.activity.recipient.id) {
                await context.sendActivity("Hello and welcome! With this sample you can send task requests to your manager, and your manager can approve/reject the request.");
            }
        }
        await next();
    }

    /**
     * Handles incoming messages.
     * @param {TurnContext} context - The context object for the turn.
     * @param {function} next - The next middleware function to call.
     */
    async handleMessage(context, next) {
        await this.startTaskManagement(context);
        await next();
    }

    /**
     * Handles invoke activities, such as adaptive card actions.
     * @param {TurnContext} context - The context object for the turn.
     * @returns {Promise<InvokeResponse>} - The response to the invoke activity.
     */
    async onInvokeActivity(context) {
        const user = context.activity.from;
        if (context.activity.name === 'adaptiveCard/action') {
            const action = context.activity.value.action;
            const allMembers = await TeamsInfo.getMembers(context);
            const card = await adaptiveCards.selectResponseCard(context, user, allMembers);
            return adaptiveCards.invokeResponse(card);
        }
    }

    /**
     * Starts the task management process by sending an initial adaptive card.
     * @param {TurnContext} context - The context object for the turn.
     */
    async startTaskManagement(context) {
        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCards.optionInc())]
        });
    }
}

module.exports.TeamsBot = TeamsBot;