// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, TeamsActivityHandler, TurnContext } = require('botbuilder');
const FlightsDetailsCardTemplate = require('../resources/flightsDetails.json');
const SearchHotelsCardTemplate = require('../resources/searchHotels.json');

class TeamsCommandsMenuBot extends TeamsActivityHandler {
    constructor() {
        super();

        // Message handler to process user input
        this.onMessage(async (context, next) => {
            // Remove mention text and check if the message contains relevant commands
            const activity = TurnContext.removeMentionText(context.activity);

            if (activity && context.activity.text) {
                const text = context.activity.text.trim().toLowerCase();

                // Handle different commands with clear conditions
                if (/search flights/.test(text)) {
                    await this.sendFlightsDetailsCard(context);
                } else if (/search hotels/.test(text)) {
                    await this.sendHotelsDetailsCard(context);
                } else if (/help/.test(text)) {
                    await context.sendActivity('Displays this help message.');
                } else if (/best time to fly/.test(text)) {
                    await context.sendActivity('Best time to fly to London for a 5-day trip is summer.');
                }
            } 
            // Handle hotel search details from activity value
            else if (context.activity.value) {
                const { checkinDate, checkoutDate, location, numberOfGuests } = context.activity.value;
                await context.sendActivity(`Hotel search details: 
                    Check-in Date: ${checkinDate}, 
                    Checkout Date: ${checkoutDate},
                    Location: ${location}, 
                    Number of Guests: ${numberOfGuests}`);
            }

            // Proceed to the next handler
            await next();
        });
    }

    // Send the Flights Details Card as a response
    async sendFlightsDetailsCard(context) {
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(FlightsDetailsCardTemplate)] });
    }

    // Send the Hotels Details Card as a response
    async sendHotelsDetailsCard(context) {
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(SearchHotelsCardTemplate)] });
    }
}

module.exports.TeamsCommandsMenuBot = TeamsCommandsMenuBot;
