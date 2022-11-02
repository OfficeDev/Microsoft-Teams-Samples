namespace Microsoft.Teams.Selfhelp.Bot
{
    using System;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class that implements error handler.
    /// </summary>
    public class SelfHelpAdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfHelpAdapterWithErrorHandler"/> class.
        /// </summary>
        /// <param name="credentialProvider">Credential provider for bot.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="channelProvider">Framework channel service.</param>
        /// <param name="dCFSActivityMiddleware">Represents middleware that can operate on incoming activities.</param>
        /// <param name="conversationState">A state management object for conversation state.</param>
        public SelfHelpAdapterWithErrorHandler(
            ICredentialProvider credentialProvider,
            ILogger<BotFrameworkHttpAdapter> logger,
            IChannelProvider channelProvider,
            SelfHelpActivityMiddleware dCFSActivityMiddleware,
            ConversationState conversationState = null)
            : base(credentialProvider, channelProvider: channelProvider, logger: logger)
        {
            dCFSActivityMiddleware = dCFSActivityMiddleware ?? throw new ArgumentNullException(nameof(dCFSActivityMiddleware));

            // Add activity middleware to the adapter's middleware pipeline
            this.Use(dCFSActivityMiddleware);

            this.OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Error Message");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Exception caught on attempting to delete conversation state : {ex.Message}");
                        throw;
                    }
                }
            };
        }
    }
}