// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsActivityHandler, CardFactory, TurnContext, TeamsInfo } = require('botbuilder');
const { GraphHelper } = require('../helper/graph-helper');
class MeetingNotficationBot extends TeamsActivityHandler {

    /**
     * 
     * @param {object} conversationReferences 
     */
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

        this.onInstallationUpdateAdd(async (context, next) => {
            var meetingInfo = await TeamsInfo.getMeetingInfo(context);
            var decodedMeetingJoinUrl = decodeURI(meetingInfo.details.joinUrl);

            this.addConversationReference(context.activity, decodedMeetingJoinUrl);
            console.log('Running dialog with Message Activity.');
            
            await GraphHelper.createSubscription(meetingInfo.details.joinUrl);
            await next();
        });
    }

    addConversationReference(activity, meetingUrl) {
        const conversationReference = TurnContext.getConversationReference(activity);
        this.conversationReferences[meetingUrl] = conversationReference;
    }

    async DisplayData(context, header, availability, activity) {
        try {
            const card = {
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
                        "type": "FactSet",
                        "facts": [
                            {
                                "title": "Availabilty:",
                                "value": availability
                            },
                            {
                                "title": "Activity:",
                                "value": activity
                            }
                        ]
                    }
                ]
            }
            const tdata = CardFactory.adaptiveCard(card);
            await context.sendActivity({ attachments: [tdata] });
        }
        catch (e) {
            console.log("error", e);
        }
    }
}
module.exports.MeetingNotficationBot = MeetingNotficationBot;
