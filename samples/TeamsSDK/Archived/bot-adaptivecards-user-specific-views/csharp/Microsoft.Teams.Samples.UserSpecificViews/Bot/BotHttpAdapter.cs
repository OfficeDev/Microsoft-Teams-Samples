// <copyright file="BotHttpAdapter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.UserSpecificViews.Bot
{
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Bot Http Adapter.
    /// </summary>
    public class BotHttpAdapter : CloudAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotHttpAdapter"/> class.
        /// </summary>
        /// <param name="auth">Cloud environment used to authenticate.</param>
        /// <param name="logger">Logger.</param>
        public BotHttpAdapter(
            BotFrameworkAuthentication auth,
            ILogger<BotHttpAdapter> logger)
            : base(auth, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                var errorMessage = exception.Message;
                logger.LogError(exception, $"[OnTurnError] unhandled error : {errorMessage}", errorMessage);

                // Uncomment below commented line for local debugging.
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
