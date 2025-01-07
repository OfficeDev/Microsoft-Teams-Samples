// <copyright file="BotController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.BotBuilderSamples
{
    // This ASP Controller is created to handle requests to the bot.
    // Dependency Injection provides the Adapter and IBot at runtime. 
    // Multiple IBot implementations can be run at different endpoints.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _botInstance;

        // Constructor to inject the adapter and bot instance
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _botInstance = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            try
            {
                // Delegate HTTP request processing to the bot adapter.
                await _adapter.ProcessAsync(Request, Response, _botInstance);
            }
            catch (Exception ex)
            {
                // Log exception and return 500 Internal Server Error
                // You can replace this with proper logging mechanism.
                Response.StatusCode = 500;
                await Response.WriteAsync($"Error processing request: {ex.Message}");
            }
        }
    }
}
