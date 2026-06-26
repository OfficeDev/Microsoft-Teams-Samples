using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace TeamsTalentMgmtApp
{
    public class BotLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<BotLoggingMiddleware> _logger;
        private readonly JsonSerializerOptions _serializerOptions;

        public BotLoggingMiddleware(ILogger<BotLoggingMiddleware> logger)
        {
            _logger = logger;
            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                // run full pipeline
                var responses = await nextSend().ConfigureAwait(false);

                foreach (var activity in activities)
                {
                    _logger.LogTrace(JsonSerializer.Serialize(activity, _serializerOptions));
                }

                return responses;
            });

            await next(cancellationToken);
        }
    }
}
