// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;

namespace PeoplePicker.Controllers
{
    /// <summary>
    /// This controller handles requests from users. It delegates processing to the Bot framework adapter.
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
        /// <param name="adapter">The adapter for handling the bot's HTTP request/response.</param>
        /// <param name="bot">The bot that processes the message.</param>
        public BotController(CloudAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// Processes incoming HTTP requests and delegates them to the bot adapter for handling.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP request to the adapter.
            // The adapter will invoke the bot for further message handling.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
