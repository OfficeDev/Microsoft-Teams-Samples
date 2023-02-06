using GraphPinnedMessage.Helper;
using GraphPinnedMessage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace GraphPinnedMessage.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class PinnedMessageController : Controller
    {
        private readonly ILogger<PinnedMessageController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PinnedMessageController(ILogger<PinnedMessageController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Exchange the id token with server token and fetch the pinned message.
        /// </summary>
        /// <returns>Returns Pinned message details</returns>

        [HttpGet("getGraphAccessToken")]
        public async Task<ActionResult> GetUserAccessToken([FromQuery]string ssoToken, string chatId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);

                var messages = await graphClient.Chats[chatId].PinnedMessages.Request().Expand("message").GetAsync();

                System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex("<[^>]*>");
                var teamworkTagList = messages.CurrentPage.Select(tag => new PinnedMessage
                {
                    Id = tag.Id,
                    Message = rx.Replace(tag.Message.Body.Content.ToString(), "")
                }).ToList();

                var messages2 = await graphClient.Chats[chatId].Messages.Request().Top(5).GetAsync();

                var chatMessages = messages2.CurrentPage.Where(tag => tag.MessageType.ToString() == "Message").ToList();
                var topMessages = chatMessages.Select(message => new PinnedMessage
                {
                    Id = message.Id,
                    Message = rx.Replace(message.Body.Content.ToString(), "")
                }).ToList();

                var MessageData = new MessageData
                {
                    Id = teamworkTagList[0].Id,
                    Message = teamworkTagList[0].Message,
                    Messages = topMessages
                };

                var responseMessageData = JsonConvert.SerializeObject(MessageData);

                return this.Ok(responseMessageData);
            }
            catch (Exception e)
            {
                return this.NotFound();
            }
        }

        [HttpGet("unpinMessage")]
        public async Task<string> UnpinMessage([FromQuery] string ssoToken, string chatId, string pinnedMessageId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);

                await graphClient.Chats[chatId].PinnedMessages[pinnedMessageId].Request().DeleteAsync();

                return "Unpinned Successfully";
            }
            catch (Exception e)
            {
                return "Error occured";
            }
        }

        /// <summary>
        /// Method to pin new message in chat.
        /// </summary>
        /// <returns>Returns Pinned message details</returns>

        [HttpGet("pinMessage")]
        public async Task<string> PinMessage([FromQuery] string ssoToken, string chatId, string messageId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);

                var pinnedChatMessageInfo = new PinnedChatMessageInfo
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"message@odata.bind", $"https://graph.microsoft.com/beta/chats/{chatId}/messages/{messageId}"}
                    }
                };

                await graphClient.Chats[chatId].PinnedMessages.Request().AddAsync(pinnedChatMessageInfo);

                return "Pinned new message Successfully";
            }
            catch (Exception e)
            {
                return "Error occured";
            }
        }
    }
}