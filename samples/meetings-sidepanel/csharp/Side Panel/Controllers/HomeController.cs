using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using SidePanel.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using AdaptiveCards;

namespace SidePanel.Controllers
{

    public class HomeController : Controller
    {
        public static string conversationId;
        public static string meetingOrganizerId;
        public static string userName;
        public static List<TaskInfo> taskInfoData = new List<TaskInfo>();
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Configure call from Manifest
        [Route("/Home/configure")]
        public ActionResult configure()
        {
            return View("configure");
        }

        //SidePanel Call from Configure
        [Route("/Home/SidePanel")]
        public ActionResult SidePanel()
        {
            List<TaskInfo> model = this.SidePanelDefaultAgendaList();
            return PartialView("SidePanel", model);
        }

        //Add Default Agenda to the List
        private List<TaskInfo> SidePanelDefaultAgendaList()
        {
            if (taskInfoData.Count == 0)
            {
                var tData1 = new TaskInfo
                {
                    title = "Approve 5% dividend payment to shareholders."
                };
                taskInfoData.Add(tData1);
                var tData2 = new TaskInfo
                {
                    title = "Increase research budget by 10%."
                };
                taskInfoData.Add(tData2);
                var tData3 = new TaskInfo
                {
                    title = "Continue with WFH for next 3 months."
                };
                taskInfoData.Add(tData3);
            }
            return taskInfoData;
        }

        //Add New Agenda Point to the Agenda List
        [Route("AddNewAgendaPoint")]
        public List<TaskInfo> AddNewAgendaPoint(TaskInfo? taskInfo)
        {
            var tData = new TaskInfo
            {
                title = taskInfo.title
            };
            taskInfoData.Add(tData);
            return taskInfoData;
        }

        //Senda Agenda List to the Meeting Chat
        [Route("SendAgenda")]
        public void SendAgenda()
        {
            string appId = _configuration["MicrosoftAppId"];
            string appSecret = _configuration["MicrosoftAppPassword"];
            var serviceUrl = _configuration["ServiceUrl"];
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
                new AdaptiveTextBlock(){Text="From _" + userName + "_"},
                new AdaptiveTextBlock(){Text="Here is the Agenda for Today", Weight=AdaptiveTextWeight.Bolder}
            };

            foreach (var agendaPoint in taskInfoData)
            {
                var textBlock = new AdaptiveTextBlock() { Text = "- " + agendaPoint.title + " \r" };
                adaptiveCard.Body.Add(textBlock);
            }

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
        }

        //Check if The Role is Organizer
        [Route("GetRole")]
        public bool GetRole(string userId)
        {
            if(userId == meetingOrganizerId)
            {
                return true;
            }
            else
            return false;
        }
    }
}