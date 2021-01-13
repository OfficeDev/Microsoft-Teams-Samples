// <copyright file="BotController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;

    /// <summary>
    /// Message controller for the bot.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly IBot bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">Token Bot Adapter instance.</param>
        /// <param name="bot">Token Bot instance.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            this.adapter = adapter;
            this.bot = bot;
        }

        /// <summary>
        /// POST: api/Messages
        /// Delegate the processing of the HTTP POST to the adapter.
        /// The adapter will invoke the bot.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await this.adapter.ProcessAsync(this.Request, this.Response, this.bot);
        }
    }
}
