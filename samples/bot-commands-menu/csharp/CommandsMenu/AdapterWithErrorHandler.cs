// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    // CloudAdapter with error handling for SingleTenant authentication
    // Provides enhanced authentication and error handling capabilities
    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(BotFrameworkAuthentication botFrameworkAuthentication, ILogger<CloudAdapter> logger)
            : base(botFrameworkAuthentication, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log the unhandled exception.
                // Be mindful not to expose sensitive information in production environments.
                logger.LogError(exception, $"[OnTurnError] unhandled error: {exception.Message}");

                // Uncomment below for local debugging purposes.
                // It’s recommended to only send error messages to the user in development environments.
                // if (environment.IsDevelopment())
                // {
                //     await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");
                // }

                // Send a trace activity to the Bot Framework Emulator, useful for debugging.
                // You can view this activity in the Emulator's Debug console.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
