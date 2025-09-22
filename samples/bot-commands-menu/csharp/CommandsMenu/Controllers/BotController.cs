// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;

namespace CommandsMenu.Controllers
{
    // BotController handles incoming HTTP POST requests and delegates them to the CloudAdapter.
    // It acts as an interface between incoming requests and the bot's processing logic for SingleTenant setup.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly CloudAdapter adapter;  // Updated to CloudAdapter for SingleTenant authentication
        private readonly IBot bot;  // Renamed Bot to bot (camelCase)

        // Initializes a new instance of the BotController.
        // adapter: The CloudAdapter used to process requests with SingleTenant authentication.
        // bot: The bot implementation that processes the incoming messages.
        public BotController(CloudAdapter adapter, IBot bot)
        {
            this.adapter = adapter;
            this.bot = bot;
        }

        // Handles HTTP POST requests to process incoming bot messages.
        // The method delegates the request processing to the CloudAdapter for SingleTenant authentication.
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the CloudAdapter.
            // The CloudAdapter will invoke the bot and handle the response with SingleTenant authentication.
            await adapter.ProcessAsync(Request, Response, bot);
        }
    }
}
