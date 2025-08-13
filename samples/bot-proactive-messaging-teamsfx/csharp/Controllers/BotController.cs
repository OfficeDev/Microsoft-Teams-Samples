// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Controllers // Replace with your actual namespace
{
    // This ASP Controller handles both incoming bot messages and proactive notification triggers.
    [ApiController]
    [Route("api")]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly string _appId;

        public BotController(
            IBotFrameworkHttpAdapter adapter,
            IBot bot,
            IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _bot = bot;
            _appId = configuration["MicrosoftAppId"];
            _conversationReferences = conversationReferences;
        }

        // Endpoint for handling incoming messages from Teams
        [HttpPost("messages")]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }

        // Endpoint to trigger proactive messages to stored conversation references
        [HttpGet("notify")]
        public async Task<IActionResult> NotifyAsync()
        {
            // Cast adapter to use ContinueConversationAsync method
            var adapter = (CloudAdapter)_adapter;

            foreach (var reference in _conversationReferences.Values)
            {
                await adapter.ContinueConversationAsync(
                    _appId,
                    reference,
                    async (turnContext, cancellationToken) =>
                    {
                        await turnContext.SendActivityAsync("proactive hello");
                    },
                    default);
            }

            return Content("<html><body><h1>Proactive messages have been sent.</h1></body></html>", "text/html");
        }
    }
}
