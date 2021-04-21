using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.BotBuilderSamples.Bots;

namespace Microsoft.BotBuilderSamples.Controllers
{
    public class HomeController : Controller
    {
        public static List<DeepLinksModel> deeplinks = new List<DeepLinksModel>();
        public DeepLinksModel task1Link;
        public DeepLinksModel task2Link;
        public DeepLinksModel task3Link;

        public string context1 = DeeplinkHelper.task1Context;
        public string context2 = DeeplinkHelper.task2Context;
        public string context3 = DeeplinkHelper.task3Context;


        public static List<DeepLinkChannelModel> channelDeeplinks = new List<DeepLinkChannelModel>();
        public DeepLinkChannelModel task1ChannelLink;
        public DeepLinkChannelModel task2ChannelLink;
        public DeepLinkChannelModel task3ChannelLink;

        public string channelContext1 = DeepLinkHelperChannel.task1Context;
        public string channelContext2 = DeepLinkHelperChannel.task2Context;
        public string channelContext3 = DeepLinkHelperChannel.task3Context;

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
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + context1,
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + context2,
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"

                };
                task3Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + context3,
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
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkapp?context=" + channelContext1,
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkapp?context=" + channelContext2,
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"

                };
                task3ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkapp?context=" + channelContext3,
                    ID = 3,
                    linkTitle = "Teams Apps"

                };


                channelDeeplinks.Add(task1ChannelLink);
                channelDeeplinks.Add(task2ChannelLink);
                channelDeeplinks.Add(task3ChannelLink);
            }
            return View(channelDeeplinks);
        }


        [Route("ChannelView")]
        public ActionResult ChannelView()
        {
            if (channelDeeplinks.Count == 0)
            {
                task1ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkapp?context=" + channelContext1,
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkapp?context=" + channelContext2,
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"

                };
                task3ChannelLink = new DeepLinkChannelModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkapp?context=" + channelContext3,
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
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + context1,
                    ID = 1,
                    linkTitle = "Bots in Teams"
                };

                task2Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + context2,
                    ID = 2,
                    linkTitle = "Bot Frawework SDK"

                };
                task3Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + context3,
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
    }
}
