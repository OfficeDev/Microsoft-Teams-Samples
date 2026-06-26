// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;

/// <summary>
/// AdapterWithErrorHandler handles errors during bot execution.
/// </summary>
public class AdapterWithErrorHandler : CloudAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
    /// </summary>
    /// <param name="botFrameworkAuthentication">The bot framework authentication.</param>
    /// <param name="logger">The logger.</param>
    public AdapterWithErrorHandler(BotFrameworkAuthentication botFrameworkAuthentication, ILogger<AdapterWithErrorHandler> logger)
        : base(botFrameworkAuthentication, logger)
    {
        OnTurnError = async (turnContext, exception) =>
        {
            // Log any leaked exception from the application.
            logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

            // Send a trace activity, which will be displayed in the Bot Framework Emulator.
            await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
        };
    }
}
