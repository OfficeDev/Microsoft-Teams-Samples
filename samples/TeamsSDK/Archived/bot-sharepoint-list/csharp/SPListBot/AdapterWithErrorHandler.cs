// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.SPListBot
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        private readonly ILogger<IBotFrameworkHttpAdapter> _logger;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<IBotFrameworkHttpAdapter> logger)
            : base(configuration, null, logger)
        {
            _logger = logger;

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any unhandled exception from the application.
                _logger.LogError(exception, $"[OnTurnError] Unhandled error: {exception.Message}");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync(
                    "OnTurnError Trace",
                    exception.Message,
                    "https://www.botframework.com/schemas/error",
                    "TurnError");

                // Uncomment below for local debugging to send error message to user
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");
            };
        }
    }
}
