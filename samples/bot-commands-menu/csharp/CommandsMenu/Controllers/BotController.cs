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
    // BotController handles incoming HTTP POST requests and delegates them to the Bot Framework's adapter.
    // It acts as an interface between incoming requests and the bot's processing logic.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter adapter;  // Renamed Adapter to adapter (camelCase)
        private readonly IBot bot;  // Renamed Bot to bot (camelCase)

        // Initializes a new instance of the BotController.
        // adapter: The bot framework HTTP adapter used to process requests.
        // bot: The bot implementation that processes the incoming messages.
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            this.adapter = adapter;
            this.bot = bot;
        }

        // Handles HTTP POST requests to process incoming bot messages.
        // The method delegates the request processing to the bot framework's adapter.
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot and handle the response.
            await adapter.ProcessAsync(Request, Response, bot);
        }
    }
}
