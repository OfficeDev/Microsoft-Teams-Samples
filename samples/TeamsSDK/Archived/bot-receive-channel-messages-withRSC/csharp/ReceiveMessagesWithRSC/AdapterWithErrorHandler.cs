// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;

namespace ReceiveMessagesWithRSC
{
    /// <summary>
    /// A CloudAdapter with error handling capabilities.
    /// </summary>
    public class AdapterWithErrorHandler : CloudAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger.</param>
        public AdapterWithErrorHandler(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<CloudAdapter> logger)
            : base(configuration, httpClientFactory, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                logger.LogError(exception, "[OnTurnError] unhandled error: {Message}", exception.Message);

                // Send a trace activity, which will be displayed in the Bot Framework Emulator.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
