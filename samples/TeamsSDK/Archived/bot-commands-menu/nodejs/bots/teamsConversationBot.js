// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, TeamsActivityHandler, TurnContext } = require('botbuilder');
const flightsDetailsCard = require('../resources/flightsDetails.json');
const searchHotelsCard = require('../resources/searchHotels.json');

class TeamsCommandsMenuBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);

            if (context.activity.text) {
                const text = context.activity.text.trim().toLowerCase();

                if (text.includes('search flights')) {
                    await context.sendActivity({ attachments: [CardFactory.adaptiveCard(flightsDetailsCard)] });
                } else if (text.includes('search hotels')) {
                    await context.sendActivity({ attachments: [CardFactory.adaptiveCard(searchHotelsCard)] });
                } else if (text.includes('help')) {
                    await context.sendActivity('Displays this help message.');
                } else if (text.includes('best time to fly')) {
                    await context.sendActivity('Best time to fly to London for a 5-day trip is summer.');
                }
            } else if (context.activity.value) {
                const { checkinDate, checkoutDate, location, numberOfGuests } = context.activity.value;
                await context.sendActivity(
                    `Hotel search details: Check-in Date: ${checkinDate}, ` +
                    `Checkout Date: ${checkoutDate}, Location: ${location}, ` +
                    `Number of Guests: ${numberOfGuests}`
                );
            }

            await next();
        });
    }
}

module.exports.TeamsCommandsMenuBot = TeamsCommandsMenuBot;
