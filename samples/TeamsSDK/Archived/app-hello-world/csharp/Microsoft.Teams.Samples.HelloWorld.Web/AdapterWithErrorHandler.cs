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
    /// <summary>
    /// Custom adapter that extends the CloudAdapter with error handling capabilities.
    /// This class overrides the OnTurnError property to log errors, trace the error for debugging,
    /// and optionally notify the user.
    /// </summary>
    public class AdapterWithErrorHandler : CloudAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// Configures the error handling behavior for the adapter.
        /// </summary>
        /// <param name="botFrameworkAuthentication">The Bot Framework Authentication.</param>
        /// <param name="logger">The logger instance for logging errors.</param>
        public AdapterWithErrorHandler(BotFrameworkAuthentication botFrameworkAuthentication, ILogger<CloudAdapter> logger)
            : base(botFrameworkAuthentication, logger)
        {
            // Configuring the OnTurnError handler to capture unhandled errors.
            OnTurnError = async (turnContext, exception) =>
            {
                // Log the error using the logger with a detailed message.
                logger.LogError(exception, $"[OnTurnError] Unhandled error: {exception.Message}");

                // Uncomment below commented line for local debugging.
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");

                // Trace the error in the Bot Framework Emulator to aid debugging.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
