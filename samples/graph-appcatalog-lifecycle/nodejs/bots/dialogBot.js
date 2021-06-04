// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, MessageFactory, CardFactory, ActionTypes } = require('botbuilder');
const { AppCatalogHelper } = require('../Helper/AppCatalog');
var _ = require('lodash');
class DialogBot extends TeamsActivityHandler {
    /**
     *
     * @param {ConversationState} conversationState
     * @param {UserState} userState
     * @param {Dialog} dialog
     */
    constructor(conversationState, userState, dialog) {
        super();
        if (!conversationState) throw new Error('[DialogBot]: Missing parameter. conversationState is required');
        if (!userState) throw new Error('[DialogBot]: Missing parameter. userState is required');
        if (!dialog) throw new Error('[DialogBot]: Missing parameter. dialog is required');

        this.conversationState = conversationState;
        this.userState = userState;
        this.dialog = dialog;
        this.dialogState = this.conversationState.createProperty('DialogState');

        this.onMessage(async (context, next) => {
            console.log('Running dialog with Message Activity.');
            console.log(context.activity.text);
            let arr = [];
            var data;
            switch (context.activity.text) {
                case "login":
                case "logout":
                    // Run the Dialog with the new message Activity.
                    await this.dialog.run(context, this.dialogState);
                    await this.cardActivityAsync(context);
                    break;
                case "listapp":
                    //Run the Dialog with the new message Activity.
                    data = await AppCatalogHelper.GetAllapp(context);
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.ListCardData(context);
                    }
                    else {
                        arr = await this.GetData(data);
                        await this.DislplayCardData(context, arr, "Listapp");
                        await this.ListCardData(context);
                    }
                    // this.FormatData(data);
                    break;
                case "app":
                    //Run the Dialog with the new message Activity.
                    data = await AppCatalogHelper.GetappById(context);
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.ListCardData(context);
                    }
                    else {
                        arr = await this.GetData(data);
                        await this.DislplayCardData(context, arr, "app");
                        await this.ListCardData(context);
                    }
                    break;
                case "findapp":
                    //Run the Dialog with the new message Activity.
                    data = await AppCatalogHelper.FindApplicationByTeamsId(context);
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.ListCardData(context);
                    }
                    else {
                        arr = await this.GetData(data);
                        await this.DislplayCardData(context, arr, "findapp");
                        await this.ListCardData(context);
                    }
                    break;
                case "status":
                    //Run the Dialog with the new message Activity.
                    data = await AppCatalogHelper.AppStatus(context);
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.ListCardData(context);
                    }
                    else {
                        arr = await this.GetData(data);
                        await this.DislplayCardData(context, arr, "status");
                        await this.ListCardData(context);
                    }
                    break;
                case "bot":
                    //Run the Dialog with the new message Activity.
                    data = await AppCatalogHelper.ListAppHavingBot(context);
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.ListCardData(context);
                    }
                    else {
                        arr = await this.GetData(data);
                        await this.DislplayCardData(context, arr, "bot");
                        await this.ListCardData(context);
                    }
                    break;
                case "home":
                    await this.cardActivityAsync(context);
                    break;
                case "list":
                    await this.ListCardData(context);
                    break;
                case "publish":
                    data = await AppCatalogHelper.PublishApp();
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.cardActivityAsync(context);
                    }
                    else {
                        await this.DislplayData(context, "publish", data);
                        await this.cardActivityAsync(context);
                    }
                    break;
                case "update":
                    data = await AppCatalogHelper.UpdateApp();
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.cardActivityAsync(context);
                    }
                    else {
                        await this.DislplayData(context, "update", data);
                        await this.cardActivityAsync(context);
                    }
                    break;
                case "delete":
                    data = await AppCatalogHelper.DeleteApp();
                    if (data === null) {
                        await this.dialog.run(context, this.dialogState);
                        await this.cardActivityAsync(context);
                    }
                    else {
                        await this.DislplayData(context, "Delete","App deleted successfully");
                        await this.cardActivityAsync(context);
                    }
                    break;
                default:
                    await context.sendActivity(MessageFactory.text(context.activity.text, context.activity.text));
                    break;
            }

            // Run the Dialog with the new message Activity.
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
    async GetData(data) {
        var arr = [];
        data.value.slice(0, 10).map(element => {
            arr.push({
                id: element.id,
                displayName: element.displayName

            });
        });
        console.log("!", arr);
        return arr;
    }
    async cardActivityAsync(context) {
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'Home',
                value: 1,
                text: 'home',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'List',
                value: 2,
                text: 'list',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Delete App',
                value: 3,
                text: 'delete',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Publish App',
                value: 4,
                text: 'publish',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Update app',
                value: 5,
                text: 'update',
            }
        ];
            await this.sendWelcomeCard(context, cardActions);
    }

    async ListCardData(context) {
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'Home',
                value: 1,
                text: 'home',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'List Apps',
                value: 3,
                text: 'listapp',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'ListApp by ID',
                value: 4,
                text: 'app',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'App based on manifest Id',
                value: 5,
                text: 'findapp',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'App Status',
                value: 6,
                text: 'status',
            },
            {
                type: ActionTypes.MessageBack,
                title: 'List of bot',
                value: 7,
                text: 'bot',
            }
        ];
        const card = CardFactory.heroCard(
            'AppCatalog Sample',
            '',
            null,
            cardActions
        );
        await context.sendActivity(MessageFactory.attachment(card));
    }

    async sendWelcomeCard(context, cardActions) {
        const card = CardFactory.heroCard(
            'AppCatalog Sample',
            '',
            null,
            cardActions
        );
        await context.sendActivity(MessageFactory.attachment(card));
    }

    async DislplayData(context, header, response) {
        let card = {
            "type": "AdaptiveCard",
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.3",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": header
                },
                {
                    "type": "TextBlock",
                    "wrap": true,
                    "text": response
                }]
        }
        var tdata = CardFactory.adaptiveCard(card);
        await context.sendActivity({ attachments: [tdata] });
    }

    async DislplayCardData(context, cardData, header) {

        console.log("cardData", cardData);
        let body = [{
            "type": "TextBlock",
            "size": "Medium",
            "weight": "Bolder",
            "text": header
        },]
        cardData.forEach(data => {
            body.push(
                {
                    "type": "TextBlock",
                    "wrap": true,
                    "text": data.displayName
                },
                {
                    "type": "TextBlock",
                    "wrap": true,
                    "text": data.id
                }
            )
        })

        let card = {
            "type": "AdaptiveCard",
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.3",
            "body": body
        }
        var tdata = CardFactory.adaptiveCard(card);
        await context.sendActivity({ attachments: [tdata] });
    }
}
module.exports.DialogBot = DialogBot;
