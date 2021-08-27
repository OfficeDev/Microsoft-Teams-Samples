using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Teams.TemplateBotCSharp.Properties;

namespace Microsoft.Teams.TemplateBotCSharp.Bot
{
    /// <summary>
    /// Log any leaked exception from the application.
    /// </summary>
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// </summary>
        /// <param name="configuration">Object that passes the application configuration key-values.</param>
        /// <param name="conversationState">State management object for maintaining conversation state.</param>
        public AdapterWithErrorHandler(IConfiguration configuration, ConversationState conversationState)
            : base((Microsoft.Bot.Connector.Authentication.ICredentialProvider)configuration)
        {
            this.OnTurnError = async (turnContext, exception) =>
            {

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(Strings.ErrorMessage).ConfigureAwait(false);

                if (conversationState != null)
                {
                    // Delete the conversationState for the current conversation to prevent the
                    // bot from getting stuck in a error-loop caused by being in a bad state.
                    // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                    await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
                }
            };
        }
    }
}