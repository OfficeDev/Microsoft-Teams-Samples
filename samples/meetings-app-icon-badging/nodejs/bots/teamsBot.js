// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, TeamsInfo, TurnContext, CardFactory } = require("botbuilder");
var ACData = require("adaptivecards-templating");
const notificationCardJson = require('../resources/SendTargetNotificationCard.json');

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        this.AppId = process.env.MicrosoftAppId;

        // This method is invoked whenever there is any message activity in bot's chat.
        this.onMessage(async (context, next) => {
            var members = new Array();
            let meetingId = context.activity.channelData.meeting.id;

            if (context.activity.value == null) {
                TurnContext.removeRecipientMention(context.activity);

                if (context.activity.text.trim() == "SendNotification") {
                    var meetingMembers = await TeamsInfo.getPagedMembers(context);
                    let tenantId = context.activity.channelData.tenant.id;

                    for (var member in meetingMembers) {
                        let participantDetail = await TeamsInfo.getMeetingParticipant(context, meetingId, member.aadObjectId, tenantId);

                        // Select only those members that are present when the meeting starts.
                        if (participantDetail.meeting.inMeeting) {
                            members.push({ id: participantDetail.user.id, name: participantDetail.user.name })
                        }
                    }

                    // Send an adaptive card to the user to select members for sending targeted notifications.
                    await context.sendActivity({ attachments: [this.createMembersAdaptiveCard(members)] });
                }
                else {
                    await context.sendActivity("Please type `SendNotification` to send In-meeting notifications.");
                }
            }
            else if (context.activity.value.Type == "StageViewNotification") {
                var adaptiveCardChoiceSet = context.activity.value.Choice;
                var selectedMembers = adaptiveCardChoiceSet.split(",");
                this.stageView(context, meetingId, selectedMembers);
            }
            else if (context.activity.value.Type == "AppIconBadging") {
                var adaptiveCardChoiceSet = context.activity.value.Choice;
                var selectedMembers = adaptiveCardChoiceSet.split(",");
                this.visualIndicator(context, meetingId, selectedMembers);
            }

            await next();
        });
    };

    // Custom method for sending targeted meeting notifications in stage.
    async stageView(context, meetingId, selectedMembers) {

        // Notification payload for meeting target notification API.
        let notificationInformation = {
            type: "targetedMeetingNotification",
            value: {
                recipients: selectedMembers,
                surfaces: [
                    {
                        surface: "meetingStage",
                        contentType: "task",
                        content: {
                            value: {
                                height: "300",
                                width: "400",
                                title: "Targeted meeting Notification",
                                url: `${this.baseUrl + "/hello.html"}`
                            }
                        }
                    }
                ]
            }
        }

        try {
            await TeamsInfo.sendMeetingNotification(context, notificationInformation, meetingId);
        } catch (exception) {
            console.log(exception);
        }
    }

    // Custom method for sending App Icon Badging on tab.
    async visualIndicator(context, meetingId, selectedMembers) {

        // Notification payload for meeting target notification API.
        let notificationInformation = {
            type: "targetedMeetingNotification",
            value: {
                recipients: selectedMembers,
                surfaces: [
                    {
                        surface: "meetingTabIcon",
                    }
                ]
            }
        }

        try {
            await TeamsInfo.sendMeetingNotification(context, notificationInformation, meetingId);
        } catch (exception) {
            console.log(exception);
        }
    }

    // Create an adaptive card to send a list of in-meeting participants
    createMembersAdaptiveCard(members) {
        var templatePayload = notificationCardJson;
        var template = new ACData.Template(templatePayload);

        var cardPayload = template.expand({
            $root: {
                members: members
            }
        });

        return CardFactory.adaptiveCard(cardPayload);
    }
}

module.exports.TeamsBot = TeamsBot;