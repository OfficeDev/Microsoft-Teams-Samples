// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Controllers
{
    public class HomeController : Controller
    {
        public readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string channelID = DeepLinkBot.channelID;
        public string teamsUrl = "https://teams.microsoft.com/l/entity/";

        public static List<DeepLinksModel> deeplinks = new List<DeepLinksModel>();
        public DeepLinksModel task1Link;
        public DeepLinksModel task2Link;
        public DeepLinksModel task3Link;

        public static List<DeepLinkChannelModel> channelDeeplinks = new List<DeepLinkChannelModel>();
        public DeepLinkChannelModel task1ChannelLink;
        public DeepLinkChannelModel task2ChannelLink;
        public DeepLinkChannelModel task3ChannelLink;

        DeeplinkHelper deeplinkHelper = new DeeplinkHelper();      
        public IActionResult Index()
        {
            return View();
        }

        [Route("DeepLink")]
        public ActionResult DeepLink()
        {
            if (deeplinks.Count == 0)
            {
                task1Link = new DeepLinksModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["TabEntityId"], "topic1"),
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2Link = new DeepLinksModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["TabEntityId"], "topic2"),
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"
                };

                task3Link = new DeepLinksModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["TabEntityId"], "topic3"),
                    ID = 3,
                    linkTitle = "Teams Apps"
                };

                deeplinks.Add(task1Link);
                deeplinks.Add(task2Link);
                deeplinks.Add(task3Link);
            }

            return View(deeplinks);
        }


        [Route("DeepLinkChannel")]
        public ActionResult DeepLinkChannel()
        {
            if (channelDeeplinks.Count == 0)
            {
                task1ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"],_configuration["BaseURL"],channelID,_configuration["ChannelEntityId"], "bot1"),
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot2"),
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"

                };

                task3ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot3"),
                    ID = 3,
                    linkTitle = "Teams Apps"
                };

                channelDeeplinks.Add(task1ChannelLink);
                channelDeeplinks.Add(task2ChannelLink);
                channelDeeplinks.Add(task3ChannelLink);
            }
            ViewBag.AppId = _configuration["MicrosoftAppId"];
            return View(channelDeeplinks);
        }


        [Route("ChannelView")]
        public ActionResult ChannelView()
        {
            if (channelDeeplinks.Count == 0)
            {
                task1ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot1"),
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot2"),
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"

                };

                task3ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot3"),
                    ID = 3,
                    linkTitle = "Teams Apps"
                };

                channelDeeplinks.Add(task1ChannelLink);
                channelDeeplinks.Add(task2ChannelLink);
                channelDeeplinks.Add(task3ChannelLink);
            }

            return View(channelDeeplinks);
        }

        [Route("Configure")]
        public ActionResult Configure()

        {
            return View();
        }

        [Route("TaskList")]
        public ActionResult TaskList()
        {
            if (deeplinks.Count == 0)
            {
                task1Link = new DeepLinksModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["TabEntityId"], "topic1"),
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2Link = new DeepLinksModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["TabEntityId"], "topic2"),
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"
                };

                task3Link = new DeepLinksModel()
                {
                    linkUrl = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["TabEntityId"], "topic3"),
                    ID = 3,
                    linkTitle = "Teams Apps"
                };

                deeplinks.Add(task1Link);
                deeplinks.Add(task2Link);
                deeplinks.Add(task3Link);
            }

            return View(deeplinks);
        }

        [Route("Detail/{id}")]
        public ActionResult Detail(int id)
        {
            return View(deeplinks.FirstOrDefault(i => i.ID == id));
        }

        [Route("DetailChannel/{id}")]
        public ActionResult DetailChannel(int id)
        {
            var model = new DeepLinkChannelModel
            {
                ID = id
            };

            return View(model);   
        }

        [Route("ExtendedDeepLinks")]
        public PartialViewResult ExtendedDeepLinks()
        {
            return PartialView();
        }
    }
}
