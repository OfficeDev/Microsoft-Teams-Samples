using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;

namespace AppCompleteSample.Controllers
{
    /// <summary>
    /// This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    /// implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    /// achieved by specifying a more specific type for the bot constructor argument.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">The bot framework HTTP adapter.</param>
        /// <param name="bot">The bot instance.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// Handles HTTP POST requests.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}