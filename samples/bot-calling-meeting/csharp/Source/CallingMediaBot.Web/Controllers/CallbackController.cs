using System.Threading.Tasks;
using CallingMediaBot.Web.Bots;
using Microsoft.AspNetCore.Mvc;

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
