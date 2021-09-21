// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const DEEPLINKTAB = 'DeepLinkTab';
const TabEntityID = "statictab";
const TabConfigEntityID = "configTab";
var isChannelUser;
var channelId;
var tabUrl;
var buttonCaption;
var deepLinkCardTitle;
var botId;

class DeepLinkStaticTabDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(DEEPLINKTAB, [
            this.beginDeepLinkStaticTabDialog.bind(this),
        ]));
    }

    async beginDeepLinkStaticTabDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "DeeplinkDialog";
        botId = process.env.MicrosoftAppId;
        this.getChannelID(stepContext);
        var message = this.createDeepLinkMessage(stepContext);
        await stepContext.context.sendActivity(message);
        return await stepContext.endDialog();
    }

    createDeepLinkMessage(stepContext) {
        var reply = stepContext.context._activity;

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        var card = this.createDeepLinkCard();
        reply.attachments = [card];
        return reply;
    }

    createDeepLinkCard() {

        if (isChannelUser) {
            tabUrl = this.getConfigTabDeepLinkURL(channelId);
            buttonCaption = "Config Tab Deep Link";
            deepLinkCardTitle = "Please click below to navigate config tab";
        }
        else {
            tabUrl = this.getStaticTabDeepLinkURL();
            buttonCaption = "Static Tab Deep Link";
            deepLinkCardTitle = "Please click below to navigate static tab";
        }

        const buttons = [
            { type: ActionTypes.OpenUrl, title: buttonCaption, value: tabUrl }
        ];

        const card = CardFactory.heroCard(deepLinkCardTitle, undefined,
            buttons);
        return card;
    }

    getStaticTabDeepLinkURL() {
        //Example -  BaseURL + 28:BotId + TabEntityId (set in the manifest) + ?conversationType=chat
        return "https://teams.microsoft.com/l/entity/28:" + botId + "/" + TabEntityID + "?conversationType=chat";
    }

    getConfigTabDeepLinkURL(channelId) {
        //Example -  BaseURL + BotId + TabConfigEntityId (e.g. entityId: "configTab" : it should be same which we have set at the time of Tab Creation like below) + ?context= + {"channelId":"19:47051e5643ed49b58665e1250b6db460@thread.skype"} (should be encoded)
        //microsoftTeams.settings.setSettings({ suggestedDisplayName: "Bot Info", contentUrl: createTabUrl(), entityId: "configTab" });

        channelId = channelId.replace("19:", "19%3a")
            .replace("@thread.skype", "%40thread.skype");

        return "https://teams.microsoft.com/l/entity/" + botId + "/" + TabConfigEntityID + "?context=%7B%22channelId%22%3A%22" + channelId + "%22%7D";
    }

    getChannelID(stepContext) {
        isChannelUser = false;

        if (stepContext.context._activity.channelData != null) {
            channelId = stepContext.context._activity.channelData.teamsChannelId;

            if (channelId != null) {
                isChannelUser = true;
            }
            else {
                isChannelUser = false;
            }
        }
    }
}

exports.DeepLinkStaticTabDialog = DeepLinkStaticTabDialog;