using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Teams.AI;

namespace AgentKnowledgeHub
{
    public class AdapterWithErrorHandler : TeamsAdapter
    {
        public AdapterWithErrorHandler(IConfiguration auth, ILogger<TeamsAdapter> logger)
            : base(auth, null, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your agent.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Only send error message for user messages, not for other message types so the agent doesn't spam a channel or chat.
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    // Send a message to the user
                    await turnContext.SendActivityAsync($"The agent encountered an unhandled error: {exception.Message}");
                    await turnContext.SendActivityAsync("To continue to run this agent, please fix the agent source code.");

                    // Send a trace activity
                    await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
                }
            };
        }
    }
}