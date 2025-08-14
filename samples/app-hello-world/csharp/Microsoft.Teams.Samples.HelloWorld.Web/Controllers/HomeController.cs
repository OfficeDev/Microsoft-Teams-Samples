using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    /// <summary>
    /// HomeController is responsible for handling routes related to the main pages of the application.
    /// This includes rendering the main index page, hello page, and other specific views.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Displays the home page (Index view).
        /// </summary>
        /// <returns>Returns the Index view.</returns>
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Redirects to the Index view but uses a different URL (Hello page).
        /// </summary>
        /// <returns>Returns the Index view as Hello.</returns>
        [Route("hello")]
        public ActionResult Hello()
        {
            return View("Index");
        }

        /// <summary>
        /// Displays the 'First' page.
        /// </summary>
        /// <returns>Returns the First view.</returns>
        [Route("first")]
        public ActionResult First()
        {
            return View();
        }

        /// <summary>
        /// Displays the 'Second' page.
        /// </summary>
        /// <returns>Returns the Second view.</returns>
        [Route("second")]
        public ActionResult Second()
        {
            return View();
        }

        /// <summary>
        /// Displays the 'Configure' page.
        /// </summary>
        /// <returns>Returns the Configure view.</returns>
        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }
    }
}
