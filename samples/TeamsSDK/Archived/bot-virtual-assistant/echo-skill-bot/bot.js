// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const appInsights = require('applicationinsights');

const { ActivityHandler, ActivityTypes, EndOfConversationCodes } = require('botbuilder');

// Define the EchoBot class, which extends the ActivityHandler class from botbuilder.
class EchoBot extends ActivityHandler {
    constructor() {
        // Call the constructor of the superclass (ActivityHandler).
        super();

        // Configure Application Insights with your instrumentation key or connection string
        const instrumentationKey = process.env.APPINSIGHTS_INSTRUMENTATIONKEY;
        const connectionString = process.env.APPINSIGHTS_CONNECTIONSTRING;

        appInsights.setup(connectionString || instrumentationKey)
            .setAutoCollectRequests(true)
            .setAutoCollectPerformance(true, true)
            .setAutoCollectExceptions(true)
            .setAutoCollectDependencies(true)
            .setAutoCollectConsole(true, true)
            .setUseDiskRetryCaching(true)
            .start();

        const appInsightsClient = appInsights.defaultClient;

        // Set up an onMessage handler to handle incoming messages.
        this.onMessage(async (context, next) => {
            try {
                // Track the event before sending the message
                appInsightsClient.trackEvent({ name: 'onMessage before message send', properties: { message: context.activity.text } });

                // Echo back the received message text.
                await context.sendActivity(`Echo bot: ${context.activity.text}`);

                // Track the event after sending the message
                appInsightsClient.trackEvent({ name: 'onMessage after message send', properties: { message: context.activity.text } });

                // Send an EndOfConversation activity to indicate completion
                await context.sendActivity({
                    type: ActivityTypes.EndOfConversation,
                    code: EndOfConversationCodes.CompletedSuccessfully
                });

                // Call the next handler in the pipeline.
                await next();
            } catch (err) {
                // Log the error to the console.
                console.error(`\n [onMessage] Exception caught: ${err}`);

                // Track the exception with Application Insights
                appInsightsClient.trackException({ exception: err });
            }
        });
    }
}

// Export the EchoBot class so it can be used in other files.
module.exports.EchoBot = EchoBot;
