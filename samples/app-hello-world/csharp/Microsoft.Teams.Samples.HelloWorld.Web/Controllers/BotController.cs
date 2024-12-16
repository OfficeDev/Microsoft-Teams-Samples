// <copyright file="BotController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2
namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;

    /// <summary>
    /// This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    /// implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    /// achieved by specifying a more specific type for the bot constructor argument.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBot bot;
        private readonly IBotFrameworkHttpAdapter adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">The bot framework HTTP adapter.</param>
        /// <param name="bot">The bot instance.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            this.adapter = adapter;
            this.bot = bot;
        }

        /// <summary>
        /// Handles the HTTP POST request.
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
