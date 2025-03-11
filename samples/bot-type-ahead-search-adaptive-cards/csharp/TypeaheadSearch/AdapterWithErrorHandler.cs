// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace TypeaheadSearch
{
    /// <summary>
    /// Adapter with error handling capabilities.
    /// </summary>
    public class AdapterWithErrorHandler : CloudAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="conversationState">The conversation state.</param>
        public AdapterWithErrorHandler(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<IBotFrameworkHttpAdapter> logger, ConversationState conversationState = default)
            : base(configuration, httpClientFactory, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Uncomment the below line for local debugging.
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator.
                await SendTraceActivityAsync(turnContext, exception);
            };
        }

        /// <summary>
        /// Sends a trace activity if the bot is running in the Bot Framework Emulator.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task SendTraceActivityAsync(ITurnContext turnContext, Exception exception)
        {
            // Only send a trace activity if we're talking to the Bot Framework Emulator.
            if (turnContext.Activity.ChannelId == Channels.Emulator)
            {
                var traceActivity = new Activity(ActivityTypes.Trace)
                {
                    Label = "TurnError",
                    Name = "OnTurnError Trace",
                    Value = exception.Message,
                    ValueType = "https://www.botframework.com/schemas/error",
                };

                // Send a trace activity.
                await turnContext.SendActivityAsync(traceActivity);
            }
        }
    }
}