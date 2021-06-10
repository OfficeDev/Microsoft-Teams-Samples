using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using DetailsTab.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using AdaptiveCards.Templating;

namespace DetailsTab.Controllers
{

    public class HomeController : Controller
    {
        public static string conversationId;
        public static string userName;
        public static string serviceUrl;
     //   public static List<TaskInfo> taskInfoDataList = new List<TaskInfo>();
        public static TaskInfoList TaskList = new TaskInfoList();
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            TaskList.baseUrl = _configuration["BaseUrl"];
        }

        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View(TaskList);
        }
        [HttpGet]
        [Route("Detail")]
        public IActionResult Detail()
        {
            return View();
        }

        [Route("Result")]
        public IActionResult Result(string id)
        {
            TaskInfo info = TaskList.taskInfoList.Find(x => x.id == id);
            return View(info);
        }
        [HttpPost]
        [Route("Index")]
        public IActionResult Index(TaskInfo taskInfo)
        {
            taskInfo.id = Guid.NewGuid().ToString();
            TaskList.taskInfoList.Add(taskInfo);
            //taskInfoDataList.Add(taskInfo);
            return Json(TaskList);
        }
        [HttpPost]
        [Route("AddNewAgenda")]
        public void AddNewAgenda(TaskInfo taskInfo)
        {

            string appId = _configuration["MicrosoftAppId"];
            string appSecret = _configuration["MicrosoftAppPassword"];

            TaskList.taskInfoList.Find(x => x.id == taskInfo.id).IsSent = true;

            using var connector = new ConnectorClient(new Uri(serviceUrl), appId, appSecret);
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl, DateTime.MaxValue);

            var replyActivity = new Activity();
            replyActivity.Type = "message";
            replyActivity.Conversation = new ConversationAccount(id: conversationId);

            var adaptiveAttachment = HomeController.AgendaAdaptiveList(taskInfo, "Poll.json", null, null);
            replyActivity.Attachments = new List<Attachment> { adaptiveAttachment };

            var response = connector.Conversations.SendToConversationAsync(conversationId, replyActivity).Result;
        }

        public static Attachment AgendaAdaptiveList(TaskInfo taskInfo, string jsonFileName, int? percentOption1, int? percentOption2)
        {
            string[] type = { ".", "Resources", jsonFileName };
            var adaptiveCard = System.IO.File.ReadAllText(Path.Combine(type));

            // Create a Template instance from the template payload
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCard);


            // You can use any serializable object as your data
            var payloadData = new
            {
                Title = taskInfo.title,
                option1 = taskInfo.option1,
                option2 = taskInfo.option2,
                Id = taskInfo.id,
                percentoption1 = percentOption1,
                percentoption2 = percentOption2
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            string cardJson = template.Expand(payloadData);

            var adaptiveCardAttachmnt = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            return adaptiveCardAttachmnt;
        }
    }
}
