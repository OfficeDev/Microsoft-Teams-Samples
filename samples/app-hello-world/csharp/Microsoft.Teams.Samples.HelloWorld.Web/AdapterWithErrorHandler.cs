using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    /// <summary>
    /// Custom adapter that extends the BotFrameworkHttpAdapter with error handling capabilities.
    /// This class overrides the OnTurnError property to log errors, trace the error for debugging,
    /// and optionally notify the user.
    /// </summary>
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// Configures the error handling behavior for the adapter.
        /// </summary>
        /// <param name="configuration">The application's configuration.</param>
        /// <param name="logger">The logger instance for logging errors.</param>
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            // Configuring the OnTurnError handler to capture unhandled errors.
            OnTurnError = async (turnContext, exception) =>
            {
                // Log the error using the logger with a detailed message.
                logger.LogError(exception, $"[OnTurnError] Unhandled error: {exception.Message}");

                // Trace the error in the Bot Framework Emulator to aid debugging.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");

                // Optionally, you can uncomment the following line to send a message to the user when an error occurs.
                // await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            };
        }
    }
}
