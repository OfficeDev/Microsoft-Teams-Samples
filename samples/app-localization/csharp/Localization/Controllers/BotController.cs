// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;

namespace Localization.Controllers
{
    // This controller handles incoming HTTP requests, delegating message processing to the Bot Adapter.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        // Read-only fields for the adapter and bot instance injected through the constructor
    private readonly CloudAdapter _adapter;
        private readonly IBot _bot;

        // Constructor receives instances of adapter and bot, injected via Dependency Injection (DI).
    public BotController(CloudAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        // POST method to process incoming requests and delegate message processing to the bot via the adapter.
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate HTTP POST processing to the adapter, which will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
