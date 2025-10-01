// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(BotFrameworkAuthentication botFrameworkAuthentication, ILogger<CloudAdapter> logger)
            : base(botFrameworkAuthentication, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log the error with additional context for better traceability.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Optionally, send a user-friendly message to the user.
                // Uncomment the line below for local debugging.
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception: {exception.Message}");

                // Attempt to send a trace activity that will be displayed in the Bot Framework Emulator.
                try
                {
                    await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
                }
                catch (System.Exception traceEx)
                {
                    // Log any error that might occur during the trace activity (e.g., failed to send trace).
                    logger.LogError(traceEx, "Error occurred while sending trace activity.");
                }
            };
        }
    }
}
