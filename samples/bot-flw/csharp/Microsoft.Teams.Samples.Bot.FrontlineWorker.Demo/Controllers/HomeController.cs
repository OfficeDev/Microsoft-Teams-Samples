using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Teams.Samples.Bot.FrontlineWorker.Demo.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        //this route serves the loginstart.html view
        [Route("loginstart")]
        public ActionResult LoginStart()
        {
            return View();
        }

        //this route serves the loginend.html view
        [Route("loginend")]
        public ActionResult LoginEnd()
        {
            return View();
        }
    }
}