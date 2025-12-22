using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Cards;
using SidePanel.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Teams.Apps;

namespace SidePanel.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        public static string? conversationId;
        public static string? serviceUrl;
        public static List<TaskInfo> taskInfoData = new List<TaskInfo>()
        {
            new TaskInfo(){Title = "Approve 5% dividend payment to shareholders."},
            new TaskInfo(){Title = "Increase research budget by 10%."},
            new TaskInfo(){Title = "Continue with WFH for next 3 months."}
        };

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly App _app;

        public HomeController(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            App app)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _app = app;
        }

        // Add New Agenda Point to the Agenda List
        [Route("AddNewAgendaPoint")]
        [HttpPost]
        public ActionResult<List<TaskInfo>> AddNewAgendaPoint([FromBody] TaskInfo taskInfo)
        {
            var tData = new TaskInfo
            {
                Title = taskInfo.Title
            };
            taskInfoData.Add(tData);
            return Ok(taskInfoData);
        }

        // Send Agenda List to the Meeting Chat
        [Route("SendAgenda")]
        [HttpGet]
        public async Task<IActionResult> SendAgenda()
        {
            if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(serviceUrl))
            {
                return BadRequest(new { 
                    error = "Conversation not initialized", 
                    message = "Please send a message in the chat first to initialize the bot connection." 
                });
            }

            try
            {
                var adaptiveCard = CreateAgendaAdaptiveCard();
                
                // Use the new Teams SDK to send the message
                await _app.Send(conversationId, adaptiveCard);
                
                return Ok(new { message = "Agenda published successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error sending agenda", message = ex.Message });
            }
        }

        // Create Adaptive Card with the Agenda List
        private AdaptiveCard CreateAgendaAdaptiveCard()
        {
            var card = new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Body = new List<CardElement>
                {
                    new TextBlock("Here is the Agenda for Today")
                    {
                        Weight = TextWeight.Bolder
                    }
                }
            };

            foreach (var agendaPoint in taskInfoData)
            {
                var textBlock = new TextBlock($"- {agendaPoint.Title}")
                {
                    Wrap = true
                };
                card.Body.Add(textBlock);
            }

            return card;
        }

        // Check if the Participant Role is Organizer
        [Route("IsOrganizer")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsOrganizer(string userId, string meetingId, string tenantId)
        {
            try
            {
                var response = await GetMeetingRoleAsync(meetingId, userId, tenantId);
                return Ok(response?.meeting?.role == "Organizer");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error checking organizer role: {ex.Message}");
            }
        }

        // Initialize conversation context (optional endpoint for future use)
        [Route("InitializeConversation")]
        [HttpPost]
        public IActionResult InitializeConversation([FromBody] ConversationInitRequest request)
        {
            if (string.IsNullOrEmpty(request?.ConversationId) || string.IsNullOrEmpty(request?.ServiceUrl))
            {
                return BadRequest(new { error = "Invalid request", message = "ConversationId and ServiceUrl are required" });
            }

            conversationId = request.ConversationId;
            serviceUrl = request.ServiceUrl;

            return Ok(new { message = "Conversation initialized successfully" });
        }

        // Check if conversation is initialized
        [Route("CheckConversation")]
        [HttpGet]
        public IActionResult CheckConversation()
        {
            return Ok(new
            {
                isInitialized = !string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(serviceUrl),
                conversationId = conversationId,
                serviceUrl = serviceUrl
            });
        }

        // Get Meeting Role for a User
        public async Task<UserMeetingRoleServiceResponse?> GetMeetingRoleAsync(string meetingId, string userId, string tenantId)
        {
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException($"Service URL is not available for tenant ID {tenantId}");
            }

            var httpClient = _httpClientFactory.CreateClient();
            
            // Get the bot token from the Teams SDK
            var clientId = _configuration["Teams:ClientId"];
            var clientSecret = _configuration["Teams:ClientSecret"];
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("Bot credentials not configured");
            }

            // Get OAuth token for Bot Framework
            var token = await GetBotTokenAsync(clientId, clientSecret, tenantId);

            var requestUrl = $"{serviceUrl}v1/meetings/{meetingId}/participants/{userId}?tenantId={tenantId}";
            
            using var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            getRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var getRoleResponse = await httpClient.SendAsync(getRoleRequest);
            getRoleResponse.EnsureSuccessStatusCode();

            var responseContent = await getRoleResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<UserMeetingRoleServiceResponse>(responseContent);
            
            return response;
        }

        private async Task<string> GetBotTokenAsync(string clientId, string clientSecret, string tenantId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "scope", "https://api.botframework.com/.default" }
            };

            var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);
            
            return tokenResponse?["access_token"].GetString() ?? throw new InvalidOperationException("Failed to obtain access token");
        }
    }

    // Request model for conversation initialization
    public class ConversationInitRequest
    {
        public string? ConversationId { get; set; }
        public string? ServiceUrl { get; set; }
    }
}
