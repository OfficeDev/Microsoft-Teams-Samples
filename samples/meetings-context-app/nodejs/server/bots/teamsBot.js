// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TeamsInfo, TurnContext, teamsGetTeamMeetingInfo } = require("botbuilder");
const adaptiveCards = require('../models/adaptiveCard');

const conversationDataReferences = {};

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        global.globalString = "";

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can send task request to your manager and your manager can approve/reject the request.");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {
            //await this.startIncManagement(context);
            let meetingId = context.activity.channelData.meeting.id;
            let tenantId = context.activity.channelData.tenant.id;
            let participantId = context.activity.from.aadObjectId;

            if (context.activity.text.includes("partitipant details")) {
                let participant = await TeamsInfo.getMeetingParticipant(context, meetingId, participantId, tenantId);
                this.formatObject(participant);
                await context.sendActivity(globalString);
            }
            else if (context.activity.text.includes("meeting details")) {
                let meetingsInfo = await TeamsInfo.getMeetingInfo(context);
                this.formatObject(meetingsInfo);
                await context.sendActivity(globalString);

            }
            else {
                await context.sendActivity("Please use one of these two commands : " + `<b>Partitipant Details</b>` + " and " + `<b>Meeting Details</b> <br>` + "Thank you");
            }
            await next();
        });
    }

    async onInvokeActivity(context) {
        console.log('Activity: ', context.activity.name);
        const user = context.activity.from;
        if (context.activity.name === 'adaptiveCard/action') {
            const action = context.activity.value.action;
            console.log('Verb: ', action.verb);
            const allMembers = await (await TeamsInfo.getMembers(context)).filter(tm => tm.aadObjectId);
            const card = await adaptiveCards.selectResponseCard(context, user, allMembers);
            return adaptiveCards.invokeResponse(card);
        }
    }

    async startIncManagement(context) {
        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCards.optionInc())]
        });
    }


    async formatObject(obj) {
        let formattedString = "";
        Object.keys(obj).forEach((key) => {
            // obj -> user, conversation
            var block = `<b>${key}:</b> <br>`;
            var temp = "";
            // obj[key] === obj.key
            if (typeof (obj[key]) === 'object') {
                var tempObj = obj[key]; // user
                Object.keys(obj[key]).forEach((secondKey) => {
                    // user -> id, display 
                    temp += ` <b> &nbsp;&nbsp;${secondKey}:</b> ${tempObj[secondKey]}<br/>`;
                });
                formattedString += block + temp;
                temp = "";
            }
        });
        return globalString = formattedString;
    }
}

module.exports.TeamsBot = TeamsBot;