// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.BotBuilderSamples
{
    [Route("api/messages")]
    [ApiController]
    public class BotController(IBotFrameworkHttpAdapter adapter, IBot bot) : ControllerBase
    {
        [HttpPost]
        public async Task PostAsync()
        {
            await adapter.ProcessAsync(Request, Response, bot);
        }
    }
}
