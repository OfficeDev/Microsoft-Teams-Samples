// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ActionTypes,
    CardFactory,
    MessageFactory,
    TeamsActivityHandler,
    TeamsInfo,
    TurnContext
} = require('botbuilder');

class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);

            const text = context.activity?.text?.trim().toLocaleLowerCase();

            if (!text) {
                await this.sendDataToBatchOperations(context);
            } else {
                if (text.includes('listusers')) {
                    await this.messageListOfUsersInput(context);
                } else if (text.includes('tenant')) {
                    await this.messageAllUsersInTenant(context);
                } else if (text.includes('team')) {
                    await this.messageAllUsersInTeamInput(context);
                } else if (text.includes('listchannels')) {
                    await this.messageListOfChannelsInput(context);
                } else if (text.includes('state')) {
                    await this.getOperationStateInput(context);
                } else if (text.includes('failed')) {
                    await this.getFailedEntriesInput(context);
                } else if (text.includes('cancel')) {
                    await this.cancelOperationInput(context);
                } else {
                    await this.cardActivityAsync(context, false);
                }
            }

            await next();
        });

        this.onMembersAddedActivity(async (context, next) => {
            await Promise.all((context.activity.membersAdded || []).map(async (member) => {
                if (member.id !== context.activity.recipient.id && context.activity.conversation.conversationType !== 'personal') {
                    await context.sendActivity(
                        `Welcome to the team ${member.givenName} ${member.surname}`
                    );
                }
            }));

            await next();
        });

        // This method registers the lambda function, which will be invoked when message sent by user is updated in chat.
        this.onTeamsMessageEditEvent(async (context, next) => {
            let editedMessage = context.activity.text;
            await context.sendActivity(`The edited message is ${editedMessage}"`);
            next();
        });
    }

    async onInstallationUpdateActivity(context) {
        if (context.activity.conversation.conversationType === 'channel') {
            context.sendActivity(MessageFactory.text(`Welcome to Microsoft Teams conversationUpdate events demo bot. This bot is configured in ${context.activity.conversation.name}`));
        } else {
            context.sendActivity(MessageFactory.text('Welcome to Microsoft Teams conversationUpdate events demo bot.'));
        }
    }

    async cardActivityAsync(context, isUpdate) {
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'Message list of users',
                value: null,
                text: 'listUsers'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Message all users in tenant',
                value: null,
                text: 'tenant'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Message all users in team',
                value: null,
                text: 'team'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Message list of channels',
                value: null,
                text: 'listChannels'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Get Operation state',
                value: null,
                text: 'state'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Get failed entries',
                value: null,
                text: 'failed'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Cancel operation',
                value: null,
                text: 'cancel'
            }
        ];

        await this.sendWelcomeCard(context, cardActions);
    }

    async sendWelcomeCard(context, cardActions) {
        const card = CardFactory.heroCard(
            'Welcome card',
            '',
            null,
            cardActions
        );
        await context.sendActivity(MessageFactory.attachment(card));
    }

    async messageListOfUsersInput(context) {
        const inputCard = CardFactory.adaptiveCard(this.getMultipleInputCard("user id", "user-id"));
        await context.sendActivity({ attachments: [inputCard] });
    }

    async messageListOfUsers(context) {
        const membersList = [];
        const tenantId = context.activity.conversation.tenantId;

        for (let i = 1; i <= 5; i++) {
            const userId = context.activity.value[`user-id${i}`];
            membersList.push({ "id": userId });
        }

        const message = MessageFactory.text("Hello user! You are part of the batch.");
        var response = await TeamsInfo.sendMessageToListOfUsers(context, message, tenantId, membersList);

        await context.sendActivity(MessageFactory.text(`All messages have been sent. OperationId: ${response.operationId}`));
    }

    async messageAllUsersInTenant(context) {
        const tenantId = context.activity.conversation.tenantId;
        const message = MessageFactory.text("Hello user! You received this tenant message from batch.");
        const response = await TeamsInfo.sendMessageToAllUsersInTenant(context, message, tenantId);

        await context.sendActivity(MessageFactory.text(`All messages have been sent. OperationId: ${response.operationId}`));
    }

    async messageAllUsersInTeamInput(context) {
        const inputCard = CardFactory.adaptiveCard(this.getInputCard("team", "team-id"));
        await context.sendActivity({ attachments: [inputCard] });
    }

    async messageAllUsersInTeam(context) {
        const teamId = context.activity.value["team-id"];
        const tenantId = context.activity.conversation.tenantId;
        const message = MessageFactory.text("Hello user! You received this team message from batch.");
        const response = await TeamsInfo.sendMessageToAllUsersInTeam(context, message, tenantId, teamId);

        await context.sendActivity(MessageFactory.text(`All messages have been sent. OperationId: ${response.operationId}`));
    }

    async messageListOfChannelsInput(context) {
        const inputCard = CardFactory.adaptiveCard(this.getMultipleInputCard("channel id", "channel-id"));
        await context.sendActivity({ attachments: [inputCard] });
    }

    async messageListOfChannels(context) {
        const membersList = [];
        const tenantId = context.activity.conversation.tenantId;

        for (let i = 1; i <= 5; i++) {
            const userId = context.activity.value[`channel-id${i}`];
            membersList.push({ "id": userId });
        }

        const message = MessageFactory.text("Hello user! You are part of the batch.");
        var response = await TeamsInfo.sendMessageToListOfChannels(context, message, tenantId, membersList);

        await context.sendActivity(MessageFactory.text(`All messages have been sent. OperationId: ${response.operationId}`));
    }

    async getOperationStateInput(context) {
        const inputCard = CardFactory.adaptiveCard(this.getInputCard("operation", "state-operationId"));
        await context.sendActivity({ attachments: [inputCard] });
    }

    async getOperationState(context) {
        const operationId = context.activity.value["state-operationId"];
        const operationState = await TeamsInfo.getOperationState(context, operationId);
        let statusResponses = "";
        for (const [key, value] of Object.entries(operationState.statusMap)) {
            statusResponses += key + ": " + value + ", ";
        };

        var response = `The operation was ${operationState.state} with the status responses: ${statusResponses} and total entries count: ${operationState.totalEntriesCount}`;

        await context.sendActivity(MessageFactory.text(response));
    }

    async getFailedEntriesInput(context) {
        const inputCard = CardFactory.adaptiveCard(this.getInputCard("operation", "entries-operationId"));
        await context.sendActivity({ attachments: [inputCard] });
    }

    async getFailedEntries(context) {
        const operationId = context.activity.value["entries-operationId"];
        let failedEntries = [];
        let continuationToken = null;
        let message = `This is the list of failed entries for the operation ${operationId}`;

        do {
            const currentPage = await TeamsInfo.getFailedEntries(context, operationId);
            continuationToken = currentPage.continuationToken;
            failedEntries = [...failedEntries, ...currentPage.failedEntryResponses];
        }
        while (continuationToken != null);

        failedEntries.forEach(entry => {
            message += `\n\n id: ${entry.entryId}, error: ${entry.error} \n`;
        });

        await context.sendActivity(MessageFactory.text(message));
    }

    async cancelOperationInput(context) {
        const inputCard = CardFactory.adaptiveCard(this.getInputCard("operation", "cancel-operationId"));
        await context.sendActivity({ attachments: [inputCard] });
    }

    async cancelOperation(context) {
        const operationId = context.activity.value["cancel-operationId"];
        var message = `The operation with Id: ${operationId}`;

        try {
            await TeamsInfo.cancelOperation(context, operationId);
            message += " has been canceled";
        }
        catch (error) {
            message += ` couldn't be canceled. ex: ${error}`;
        }

        await context.sendActivity(MessageFactory.text(message));
    }

    async sendDataToBatchOperations(context) {
        const operation = JSON.stringify(context.activity.value);

        if (operation.includes("user"))
            await this.messageListOfUsers(context);
        else if (operation.includes("channel"))
            await this.messageListOfChannels(context);
        else if (operation.includes("team"))
            await this.messageAllUsersInTeam(context);
        else if (operation.includes("state"))
            await this.getOperationState(context);
        else if (operation.includes("entries"))
            await this.getFailedEntries(context);
        else if (operation.includes("cancel"))
            await this.cancelOperation(context);
    }

    //Adaptive cards
    getInputCard = (idName, inputId) => ({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.5",
        "body": [
            {
                "type": "Input.Text",
                "id": `${inputId}`,
                "label": `Please enter the ${idName} id:`,
                "isRequired": true,
                "errorMessage": "Operation id is required"
            }
        ],
        "actions": [
            {
                "type": "Action.Submit",
                "title": "Submit"
            }
        ]
    });

    getMultipleInputCard = (idName, inputId) => ({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.5",
        "body": [
            {
                "type": "Input.Text",
                "id": `${inputId}1`,
                "label": `Please enter the ${idName} #1:`,
                "isRequired": true,
                "errorMessage": "User id is required"
            },
            {
                "type": "Input.Text",
                "id": `${inputId}2`,
                "label": `Please enter the ${idName} #2:`,
                "isRequired": true,
                "errorMessage": "User id is required"
            },
            {
                "type": "Input.Text",
                "id": `${inputId}3`,
                "label": `Please enter the ${idName} #3:`,
                "isRequired": true,
                "errorMessage": "User id is required"
            },
            {
                "type": "Input.Text",
                "id": `${inputId}4`,
                "label": `Please enter the ${idName} #4:`,
                "isRequired": true,
                "errorMessage": "User id is required"
            },
            {
                "type": "Input.Text",
                "id": `${inputId}5`,
                "label": `Please enter the ${idName} #5:`,
                "isRequired": true,
                "errorMessage": "User id is required"
            }
        ],
        "actions": [
            {
                "type": "Action.Submit",
                "title": "Submit"
            }
        ]
    })
}

module.exports.TeamsConversationBot = TeamsConversationBot;
