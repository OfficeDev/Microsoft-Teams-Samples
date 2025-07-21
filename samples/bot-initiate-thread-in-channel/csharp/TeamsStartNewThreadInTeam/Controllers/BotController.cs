// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.BotBuilderSamples.Controllers
{
    /// <summary>
    /// ASP Controller for handling incoming messages and delegating them to the Bot.
    /// Dependency Injection provides the Adapter and IBot implementations at runtime.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        /// <summary>
        /// Constructor to initialize BotController with dependency injection.
        /// </summary>
        /// <param name="adapter">The bot framework HTTP adapter used to process incoming messages.</param>
        /// <param name="bot">The bot implementation that processes user messages.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// POST method to process incoming messages.
        /// Delegates the HTTP POST request to the adapter for processing.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the HTTP POST to the adapter to invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
