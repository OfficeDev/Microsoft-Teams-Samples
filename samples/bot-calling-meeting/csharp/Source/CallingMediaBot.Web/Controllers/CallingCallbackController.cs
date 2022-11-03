// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMeetingBot.Controllers;
using CallingMediaBot.Web.Bots;
using Microsoft.AspNetCore.Mvc;

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
