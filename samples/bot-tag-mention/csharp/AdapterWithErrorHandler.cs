// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;

namespace TagMentionBot
{
    /// <summary>
    /// Adapter with error handling capabilities.
    /// </summary>
    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<IBotFrameworkHttpAdapter> logger,
            IStorage storage,
            ConversationState conversationState)
            : base(configuration, httpClientFactory, logger)
        {
            Use(new TeamsSSOTokenExchangeMiddleware(storage, configuration["ConnectionName"]));

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                logger.LogError(exception, $"[OnTurnError] unhandled error: {exception.Message}");

                // Uncomment the below line for local debugging.
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in an error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a web page.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"Exception caught on attempting to delete ConversationState: {e.Message}");
                    }
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator.
                await turnContext.TraceActivityAsync(
                    "OnTurnError Trace",
                    exception.Message,
                    "https://www.botframework.com/schemas/error",
                    "TurnError");
            };
        }
    }
}
