using CallingMediaBot.Web.Bots;
using Microsoft.AspNetCore.Mvc;

namespace CallingMeetingBot.Controllers;

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
