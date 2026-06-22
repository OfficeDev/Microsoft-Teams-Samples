// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Catering
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ConversationState? conversationState = null)
            : base(configuration, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log the exception for debugging
                logger.LogError(exception, "[OnTurnError] unhandled error: {Message}", exception.Message);

                // Send a user-friendly error message
                await turnContext.SendActivityAsync("Sorry, an error occurred. Please try again later.");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in an error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception caught while attempting to delete ConversationState: {Message}", e.Message);
                    }
                }

                // Send a trace activity for debugging (visible in Bot Framework Emulator)
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
