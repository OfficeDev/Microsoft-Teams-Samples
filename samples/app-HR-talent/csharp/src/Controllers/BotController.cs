using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly INotificationService _notificationService;
        private readonly IGraphApiService _graphApiService;

        public BotController(IBot bot, IBotFrameworkHttpAdapter adapter, INotificationService notificationService, IGraphApiService graphApiService)
        {
            _bot = bot;
            _adapter = adapter;
            _notificationService = notificationService;
            _graphApiService = graphApiService;
        }

        [HttpPost]
        [Route("api/messages")]
        public Task PostMessageAsync(CancellationToken cancellationToken)
            => _adapter.ProcessAsync(Request, Response, _bot, cancellationToken);


        [HttpPost]
        [Route("api/installbot")]
        public async Task<IActionResult> InstallAsync(
            [FromBody] UserTenantMessageRequest request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _graphApiService.InstallBotForUser(request.Id, request.TenantId, cancellationToken);

                switch (result)
                {
                    case InstallResult.InstallFailed:
                        return BadRequest("Installation failed");

                    case InstallResult.AliasNotFound:
                        return NotFound($"Alias '{request.Id}' was not found in the tenant '{request.TenantId}'");
                }

            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                return StatusCode((int)ex.StatusCode);
            }

            return Ok();
        }

        [HttpPost]
        [Route("api/notify")]
        public async Task<IActionResult> NotifyAsync(
            [FromBody] NotifyRequest request, 
            CancellationToken cancellationToken)
        {
            try
            {
                IActivity activity;

                if (request.Text != null)
                {
                    activity = MessageFactory.Text(request.Text);
                }
                else
                {
                    var card = AdaptiveCard.FromJson(request.Card.ToString()).Card;

                    activity = MessageFactory.Attachment(new Attachment
                    {
                        Content = card,
                        ContentType = AdaptiveCard.ContentType
                    });
                }

                var result = await _notificationService.SendProactiveNotification(
                    request.Id, 
                    request.TenantId, 
                    activity, 
                    cancellationToken);

                if (result == NotificationResult.AliasNotFound)
                {
                    // Alias not found
                    return NotFound($"Alias '{request.Id}' was not found in the tenant '{request.TenantId}'");
                }

                if (result == NotificationResult.BotNotInstalled)
                {
                    // Precondition failed - app not installed!
                    return StatusCode(
                        412, 
                        $"The bot has not been installed for '{request.Id}' in the tenant '{request.TenantId}'");
                }
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                return StatusCode((int)ex.StatusCode);
            }

            return Accepted();
        }
    }

    public class UserTenantMessageRequest
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
    }

    public class NotifyRequest : UserTenantMessageRequest
    {
        public JsonElement Card { get; set; }
        public string Text { get; set; }
    }
}
