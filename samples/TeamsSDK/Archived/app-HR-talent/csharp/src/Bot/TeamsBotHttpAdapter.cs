using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TeamsTalentMgmtApp.Bot
{
    public class TeamsBotHttpAdapter : CloudAdapter
    {
        public TeamsBotHttpAdapter(
                    IWebHostEnvironment env,
                    IConfiguration configuration,
                    ILogger<TeamsBotHttpAdapter> logger,
                    IBotTelemetryClient botTelemetryClient,
                    ConversationState conversationState = null)
                    : base(configuration, logger: logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                logger.LogCritical(exception, exception.Message);
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");

                if (env.IsDevelopment())
                {
                    await turnContext.SendActivityAsync(exception.ToString());
                }

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };

            Use(new TelemetryLoggerMiddleware(botTelemetryClient));
            Use(new ShowTypingMiddleware());
        }
    }
}
