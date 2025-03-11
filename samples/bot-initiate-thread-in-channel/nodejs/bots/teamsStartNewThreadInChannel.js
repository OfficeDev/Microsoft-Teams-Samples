// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    MessageFactory,
    TeamsActivityHandler,
    teamsGetChannelId,
    TeamsInfo
} = require('botbuilder');

/**
 * Bot class to handle message activities and start new threads in a Teams channel.
 */
class TeamsStartNewThreadInChannel extends TeamsActivityHandler {
    constructor() {
        super();

        // Handle incoming message activity and start a new thread
        this.onMessage(async (context, next) => {
            try {
                // Get the channel ID from the incoming activity
                const teamsChannelId = teamsGetChannelId(context.activity);

                // Create an activity message to start a new thread
                const activity = MessageFactory.text('This will be the first message in a new thread');

                // Send the activity message to the Teams channel and get the reference
                const [reference] = await TeamsInfo.sendMessageToTeamsChannel(
                    context, 
                    activity, 
                    teamsChannelId, 
                    process.env.MicrosoftAppId
                );

                // Continue the conversation and send the first response to the new thread
                await context.adapter.continueConversationAsync(
                    process.env.MicrosoftAppId, 
                    reference, 
                    async (turnContext) => {
                        await turnContext.sendActivity(
                            MessageFactory.text('This will be the first response to the new thread')
                        );
                    }
                );

                // Proceed to the next middleware
                await next();
            } catch (error) {
                console.error('Error starting new thread in Teams channel:', error);
                await context.sendActivity("Sorry, an error occurred while starting the thread.");
            }
        });
    }
}

// Export the bot class to be used in the main bot application.
module.exports.TeamsStartNewThreadInChannel = TeamsStartNewThreadInChannel;
