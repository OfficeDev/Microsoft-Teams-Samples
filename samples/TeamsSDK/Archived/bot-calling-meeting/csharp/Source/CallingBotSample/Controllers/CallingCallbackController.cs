// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CallingBotSample.Bots;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace CallingMeetingBot.Controllers
{
    [Route("callback")]
    public class CallingCallbackController : Controller
    {
        private readonly CallingBot bot;

        public CallingCallbackController(CallingBot bot)
        {
            this.bot = bot;
        }

        [HttpPost, HttpGet]
        public async Task HandleCallbackRequestAsync()
        {
            await bot.ProcessNotificationAsync(Request, Response).ConfigureAwait(false);
        }
    }
}
