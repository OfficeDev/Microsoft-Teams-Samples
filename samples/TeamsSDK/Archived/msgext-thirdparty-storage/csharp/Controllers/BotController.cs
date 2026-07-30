// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.BotBuilderSamples.Controllers
{
    /// <summary>
    /// ASP.NET Controller for processing bot messages.
    /// Handles HTTP POST requests to the bot and delegates processing to the Bot Framework adapter.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly CloudAdapter Adapter;
        private readonly IBot Bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// Dependency injection provides the adapter and bot implementation at runtime.
        /// </summary>
        /// <param name="adapter">The Bot Framework adapter.</param>
        /// <param name="bot">The bot implementation.</param>
        public BotController(CloudAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        /// <summary>
        /// Handles HTTP POST requests and delegates them to the Bot Framework adapter.
        /// The adapter processes the request and invokes the bot's logic.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpPost]
        public async Task PostAsync()
        {
            try
            {
                await Adapter.ProcessAsync(Request, Response, Bot);
            }
            catch (Exception ex)
            {
                // Log or handle errors here
                Console.Error.WriteLine($"Error processing request: {ex.Message}");
                throw;
            }
        }
    }
}
