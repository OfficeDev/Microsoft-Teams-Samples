// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.BotBuilderSamples.Controllers
{
    // This ASP.NET Controller handles incoming HTTP requests.
    // Dependency Injection will provide the `IBot` and `IBotFrameworkHttpAdapter` implementations at runtime.
    // Multiple different `IBot` implementations can be used for different endpoints by specifying a more specific type
    // in the constructor.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        // Constructor that initializes the bot and adapter via dependency injection.
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        // HTTP POST method to receive requests from users.
        // This method delegates the processing of the HTTP POST to the adapter.
        // The adapter invokes the bot with the request and response.
        [HttpPost]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
