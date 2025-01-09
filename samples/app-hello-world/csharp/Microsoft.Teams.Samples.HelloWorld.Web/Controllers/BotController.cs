using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    /// <summary>
    /// This controller handles incoming requests and delegates them to the Bot Framework adapter.
    /// Dependency Injection is used to inject the Bot Framework HTTP adapter and the bot implementation.
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
        /// <param name="adapter">The Bot Framework HTTP adapter that processes incoming requests.</param>
        /// <param name="bot">The bot implementation that processes activities.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// Handles incoming POST requests and delegates processing to the adapter.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the incoming request to the adapter.
            // The adapter will invoke the appropriate bot implementation.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
