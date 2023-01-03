// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, TeamsInfo, TurnContext, MessageFactory } = require("botbuilder");

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();

        /**  Handler invoked on member added. */
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {                  
                    await context.sendActivity("Hello and welcome!");
                    await context.sendActivity("Please use one of these two commands : " + `<b>Participant Context</b>` + " and " + `<b>Meeting Context</b> <br>` + "Thank you");
                }
            }

            await next();
        });

        /** Handler invoked on message. */
        this.onMessage(async (context, next) => {
            let meetingId = context.activity.channelData.meeting.id;
            let tenantId = context.activity.channelData.tenant.id;
            let participantId = context.activity.from.aadObjectId;

            var text = TurnContext.removeRecipientMention(context.activity);
            if (text.includes("Participant Context")) {
                let participant = await TeamsInfo.getMeetingParticipant(context, meetingId, participantId, tenantId);
                var formattedString =await this.formatObject(participant); 
                await context.sendActivity(formattedString);
            }
            else if (text.includes("Meeting Context")) {
                let meetingsInfo = await TeamsInfo.getMeetingInfo(context);
                var formattedString =await this.formatObject(meetingsInfo);                
                await context.sendActivity(JSON.stringify(formattedString));
            }
            else {
                await context.sendActivity("Please use one of these two commands : " + `<b>Participant Context</b>` + " and " + `<b>Meeting Context</b> <br>` + "Thank you");
            }
            await next();
        });
    }

    /**
     * Gets the serialize formatted object string.
     * @param {object} obj Incoming object needs to be formatted.
     * @returns Formatted string.
     */
    async formatObject(obj) {
        let formattedString = "";
        Object.keys(obj).forEach((key) => {
            var block = `<b>${key}:</b> <br>`;
            var storeTemporaryFormattedString = "";           
            if (typeof (obj[key]) === 'object') {
                Object.keys(obj[key]).forEach((secondKey) => {
                    storeTemporaryFormattedString += ` <b> &nbsp;&nbsp;${secondKey}:</b> ${obj[key][secondKey]}<br/>`;
                });

                formattedString += block + storeTemporaryFormattedString;
                storeTemporaryFormattedString = "";
            }
        });

        return formattedString;
    }
}

module.exports.TeamsBot = TeamsBot;