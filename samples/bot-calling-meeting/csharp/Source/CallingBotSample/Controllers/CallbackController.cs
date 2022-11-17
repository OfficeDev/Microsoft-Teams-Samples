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
    public class CallbackController: Controller
    {
        private readonly CallingBot bot;

        public CallbackController(CallingBot bot)
        {
            this.bot = bot;
        }

        [HttpPost, HttpGet]
        public async Task HandleCallbackRequestAsync()
        {
            await this.bot.ProcessNotificationAsync(this.Request, this.Response).ConfigureAwait(false);
        }
    }
}
