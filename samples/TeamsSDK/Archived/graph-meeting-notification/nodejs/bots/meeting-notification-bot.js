// <copyright file="meeting-notification-bot.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

const { TeamsActivityHandler, CardFactory, TurnContext, TeamsInfo } = require('botbuilder');
const { GraphHelper } = require('../helper/graph-helper');

class MeetingNotficationBot extends TeamsActivityHandler {
    constructor(conversationReferences) {
        super();
        if (!conversationReferences) throw new Error('[conversationReferences]: Missing parameter. dialog is required');
        this.conversationReferences = conversationReferences;

        this.onMessage(async (context, next) => {
            var meetingInfo = await TeamsInfo.getMeetingInfo(context);
            var decodedMeetingJoinUrl = decodeURI(meetingInfo.details.joinUrl);

            this.addConversationReference(context.activity, decodedMeetingJoinUrl);
            console.log('Running dialog with Message Activity.');
            
            await GraphHelper.createSubscription(meetingInfo.details.joinUrl);
            await next();
        });
    }
    
    async onInstallationUpdateAdd(context) {
        var meetingInfo = await TeamsInfo.getMeetingInfo(context);
        var decodedMeetingJoinUrl = decodeURI(meetingInfo.details.joinUrl);

        this.addConversationReference(context.activity, decodedMeetingJoinUrl);
        console.log('Running dialog with Message Activity.');
        
        await GraphHelper.createSubscription(meetingInfo.details.joinUrl);
        this.SendWelcomeCard(context);
    };

    addConversationReference(activity, meetingUrl) {
        const conversationReference = TurnContext.getConversationReference(activity);
        this.conversationReferences[meetingUrl] = conversationReference;
    }

    async SendWelcomeCard(context) {
        try {
            const card = {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.3",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Hello and welcome to Meeting Notification Bot",
                        "size": "medium",
                        "weight": "bolder"
                      },
                      {
                        "type": "TextBlock",
                        "text": "You will start getting notification for this meeting in the chat.",
                        "wrap": true
                      },
                      {
                        "type": "TextBlock",
                        "text": "If the notifications are not coming. Please @mention bot and send message 'Renew' in this meeting chat.",
                        "wrap": true,
                        "color": "attention"
                      }
                ]
            }

            const welcomeCard = CardFactory.adaptiveCard(card);
            await context.sendActivity({ attachments: [welcomeCard] });
        }
        catch (e) {
            console.log("error", e);
        }
    }

    static async MeetingStartEndCard(context, memberIndicator) {
        try {
            const card = {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.3",
                "body": [
                    {
                        "type": "TextBlock",
                        "size": "large",
                        "weight": "bolder",
                        "text": "Meeting notification"
                      },
                      {
                        "type": "TextBlock",
                        "weight": "bolder",
                        "text": memberIndicator
                      }
                ]
            }

            const meetingStartEndCard = CardFactory.adaptiveCard(card);
            await context.sendActivity({ attachments: [meetingStartEndCard] });
        }
        catch (e) {
            console.log("error", e);
        }
    }

    static async DisplayMeetingUpdate(context, eventType, memberList) {
        try {
            const card = {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.3",
                "body": [
                    {
                        "type": "TextBlock",
                        "size": "large",
                        "weight": "bolder",
                        "text": "Meeting notification"
                      },
                      {
                        "type": "Container",
                        "items": [
                            {
                                "type": "TextBlock",
                                "size": "Medium",
                                "weight": "Bolder",
                                "text": `${eventType} : `
                            },
                            {
                                "type": "FactSet",
                                "facts": memberList
                            }
                            ]
                      }
                ]
            }

            const meetingUpdateCard = CardFactory.adaptiveCard(card);
            await context.sendActivity({ attachments: [meetingUpdateCard] });
        }
        catch (e) {
            console.log("error", e);
        }
    }
}
module.exports.MeetingNotficationBot = MeetingNotficationBot;