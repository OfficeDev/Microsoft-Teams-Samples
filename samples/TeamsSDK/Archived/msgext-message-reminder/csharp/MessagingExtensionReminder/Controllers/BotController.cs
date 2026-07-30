// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace MessagingExtensionReminder.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController(CloudAdapter adapter, IBot bot) : ControllerBase
    {
        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            await adapter.ProcessAsync(Request, Response, bot);
        }
    }
}
