// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace BotRequestApproval;

public class AdapterWithErrorHandler : CloudAdapter
{
    public AdapterWithErrorHandler(IConfiguration configuration, IHttpClientFactory? httpClientFactory, ILogger<CloudAdapter> logger)
        : base(configuration, httpClientFactory, logger)
    {
        OnTurnError = async (turnContext, exception) =>
        {
            logger.LogError(exception, "[OnTurnError] unhandled error: {Message}", exception.Message);

            // Send a trace activity, which will be displayed in the Bot Framework Emulator.
            if (turnContext.Activity.ChannelId == Channels.Emulator)
            {
                var traceActivity = new Activity(ActivityTypes.Trace)
                {
                    Label = "TurnError",
                    Name = "OnTurnError Trace",
                    Value = exception.Message,
                    ValueType = "https://www.botframework.com/schemas/error",
                };

                await turnContext.SendActivityAsync(traceActivity);
            }
        };
    }
}