// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace BotConversationQuickStart
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        // Adapter class to handle incoming requests and integrate with the bot's logic pipeline.

        public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger)
            : base(auth, logger)
        {
            /// <summary>
            /// Handles unhandled exceptions that occur during a bot turn.
            /// Logs the error using the provided logger and sends a trace activity
            /// for debugging purposes in the Bot Framework Emulator.
            /// </summary>
            /// <param name="turnContext">The context for the current bot turn.</param>
            /// <param name="exception">The exception that was thrown during the turn.</param>
            /// <returns>A task that represents the asynchronous error handling operation.</returns>

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Uncomment below commented line for local debugging.
                //await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
