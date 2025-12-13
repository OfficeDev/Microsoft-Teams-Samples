using System.Threading.Tasks;
using MeetingEventsCallingBot.Bots;
using Microsoft.AspNetCore.Mvc;

namespace MeetingEventsCallingBot.Controllers
{
    public class CallbackController : Controller
    {
        private readonly CallingBotService bot;

        public CallbackController(CallingBotService bot)
        {
            this.bot = bot;
        }

        [HttpPost, HttpGet]
        [Route("api/calls")]
        public async Task HandleCallbackRequestAsync()
        {
            await this.bot.ProcessNotificationAsync(this.Request, this.Response).ConfigureAwait(false);
        }
    }
}
