// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace ReceiveMessagesWithRSC.Controllers
{
    /// <summary>
    /// ASP.NET Controller that handles incoming bot messages via the CloudAdapter.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly CloudAdapter _adapter;
        private readonly IBot _bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">The Cloud adapter.</param>
        /// <param name="bot">The bot instance.</param>
        public BotController(CloudAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// Handles HTTP POST and GET requests to process bot messages.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
