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

        public IActionResult Index()
        {
            return View();
        }

        [Route("DeepLink")]
        public ActionResult DeepLink()

        {
            return View();
        }       

        [Route("DeepLinkTask")]
        public ActionResult DeepLinkTask()

        {
            return View();
        }

        [Route("TaskList")]
        public ActionResult TaskList()
        {
            if (deeplinks.Count==0)
            {
                task1Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/YOUR-MICROSOFT-APPID/ENTITY-ID?context=" + context1,
                    ID = 1
                };

                task2Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/YOUR-MICROSOFT-APPID/ENTITY-ID?context=" + context2,
                    ID = 2

                };
                task3Link = new DeepLinksModel()
                {
                    linkUrl = "https://teams.microsoft.com/l/entity/YOUR-MICROSOFT-APPID/ENTITY-ID?context=" + context3,
                    ID = 3

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
            return View(deeplinks.FirstOrDefault(i=>i.ID==id));

        }

    }
}
