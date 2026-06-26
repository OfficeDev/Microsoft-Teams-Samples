// <copyright file="AdapterWithErrorHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace LinkUnfurlingInShareToTeams
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<IBotFrameworkHttpAdapter> logger)
            : base(configuration, httpClientFactory, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await SendTraceActivityAsync(turnContext, exception);
            };
        }

        private static async Task SendTraceActivityAsync(ITurnContext turnContext, Exception exception)
        {
            // Only send a trace activity if we're talking to the Bot Framework Emulator
            if (turnContext.Activity.ChannelId == Channels.Emulator)
            {
                Activity traceActivity = new Activity(ActivityTypes.Trace)
                {
                    Label = "TurnError",
                    Name = "OnTurnError Trace",
                    Value = exception.Message,
                    ValueType = "https://www.botframework.com/schemas/error",
                };

                // Send a trace activity
                await turnContext.SendActivityAsync(traceActivity);
            }
        }
    }
}