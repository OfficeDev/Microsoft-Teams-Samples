using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Teams.AI;

namespace AgentKnowledgeHub.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly TeamsAdapter Adapter;
        private readonly IBot Bot;

        public BotController(TeamsAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync(CancellationToken cancellationToken = default)
        {
            await Adapter.ProcessAsync(Request, Response, Bot, cancellationToken);
        }
    }
}