using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using AdaptiveCards;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SidePanel.Models;

namespace SidePanel.Controllers
{
    public class HomeController : Controller
    {
        public static string conversationId;
        public static string serviceUrl;
        public static List<TaskInfo> taskInfoData = new List<TaskInfo>()
        {
            new TaskInfo(){Title = "Approve 5% dividend payment to shareholders."},
            new TaskInfo(){Title = "Increase research budget by 10%."},
            new TaskInfo(){Title = "Continue with WFH for next 3 months."}
        };

        private readonly IConfiguration _configuration;
        private readonly AppCredentials botCredentials;
        private readonly HttpClient httpClient;

        public HomeController(IConfiguration configuration, IHttpClientFactory httpClientFactory, AppCredentials botCredentials)
        {
            _configuration = configuration;
            this.botCredentials = botCredentials;
            this.httpClient = httpClientFactory.CreateClient();
        }

        //Add New Agenda Point to the Agenda List
        [Route("/Home/AddNewAgendaPoint")]
        [HttpPost]
        public List<TaskInfo> AddNewAgendaPoint([FromBody] TaskInfo taskInfo)
        {
            var tData = new TaskInfo
            {
                Title = taskInfo.Title
            };
            taskInfoData.Add(tData);
            return taskInfoData;
        }

        //Senda Agenda List to the Meeting Chat
        [Route("/Home/SendAgenda")]
        public void SendAgenda()
        {
            string appId = _configuration["MicrosoftAppId"];
            string appSecret = _configuration["MicrosoftAppPassword"];
            using var connector = new ConnectorClient(new Uri(serviceUrl), appId, appSecret);
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl, DateTime.MaxValue);
            var replyActivity = new Activity();
            replyActivity.Type = "message";
            replyActivity.Conversation = new ConversationAccount(id: conversationId);
            var adaptiveAttachment = AgendaAdaptiveList();
            replyActivity.Attachments = new List<Attachment> { adaptiveAttachment };
            var response = connector.Conversations.SendToConversationAsync(conversationId, replyActivity).Result;
        }

        //Create Adaptive Card with the Agenda List
        private Attachment AgendaAdaptiveList()
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            adaptiveCard.Body = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock(){Text="Here is the Agenda for Today", Weight=AdaptiveTextWeight.Bolder}
            };

            foreach (var agendaPoint in taskInfoData)
            {
                var textBlock = new AdaptiveTextBlock() { Text = "- " + agendaPoint.Title + " \r" };
                adaptiveCard.Body.Add(textBlock);
            }

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
        }

        //Check if the Participant Role is Organizer
        [Route("/Home/IsOrganizer")]
        public async Task<ActionResult<bool>> IsOrganizer(string userId, string meetingId, string tenantId)
        {
            var response = await GetMeetingRoleAsync(meetingId, userId, tenantId);
            if (response.meeting.role == "Organizer")
                return true;
            else
                return false;
        }

        public async Task<UserMeetingRoleServiceResponse> GetMeetingRoleAsync(string meetingId, string userId, string tenantId)
        {
            if (serviceUrl == null)
            {
                throw new InvalidOperationException("Service URL is not avaiable for tenant ID " + tenantId);
            }

            using var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(serviceUrl), string.Format("v1/meetings/{0}/participants/{1}?tenantId={2}", meetingId, userId, tenantId)));
            getRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await this.botCredentials.GetTokenAsync());

            using var getRoleResponse = await this.httpClient.SendAsync(getRoleRequest);
            getRoleResponse.EnsureSuccessStatusCode();

            var response = JsonConvert.DeserializeObject<UserMeetingRoleServiceResponse>(await getRoleResponse.Content.ReadAsStringAsync());
            return response;
        }
    }
}