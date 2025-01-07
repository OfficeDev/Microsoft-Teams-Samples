using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        // Constructor that initializes the bot framework authentication and logger.
        public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger)
            : base(auth, logger)
        {
            // Define the error handling behavior during the bot's turn.
            OnTurnError = async (turnContext, exception) =>
            {
                // Log the exception details for debugging and tracking errors.
                logger.LogError(exception, $"[OnTurnError] unhandled error: {exception.Message}");

                // For development purposes, uncomment to provide a custom error message to users locally.
                // await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. Exception Caught: {exception.Message}");

                // Send a trace activity to the Bot Framework Emulator for deeper debugging.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
