// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    CardFactory,
    TeamsActivityHandler,
    TurnContext
} = require('botbuilder');

const FlightsDetailsCardTemplate  = require('../resources/flightsDetails.json');

const SearchHotelsCardTemplate  = require('../resources/searchHotels.json');

class TeamsCommandsMenuBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            
        var activity = TurnContext.removeMentionText(context.activity);

        if (activity != null)
        {
            if (context.activity.text != null) 
            {
                const text = context.activity.text.trim().toLocaleLowerCase();
                if (text.includes('search flights')) 
                {
                    await this.SearchFlightsReaderCardAsync(context);
                } 
                else if (text.includes('search hotels')) 
                {
                    await this.SearchHotelsReaderCardAsync(context);
                } 
                else if (text.includes('help')) 
                {
                    await context.sendActivity('Displays this help message.');
                } 
                else if (text.includes('best time to fly')) 
                {
                    await context.sendActivity('Best time to fly to London for a 5 day trip this summer.');
                } 
            }
        }
        else if (context.activity.value != null) 
        {
            await context.sendActivity(`Hotel search details are: ${context.activity.value.checkinDate}, \n Task CheckoutDate : ${context.activity.value.checkoutDate},\n Location : ${context.activity.value.location},\n NumberOfGuests : ${context.activity.value.numberOfGuests}` );
        }
        // By calling next() you ensure that the next BotHandler is run.
        await next();
        });
    }

    async SearchFlightsReaderCardAsync(context) {
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(FlightsDetailsCardTemplate)]});
    }

    async SearchHotelsReaderCardAsync(context) {
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(SearchHotelsCardTemplate)]});
    }
}

module.exports.TeamsCommandsMenuBot = TeamsCommandsMenuBot;