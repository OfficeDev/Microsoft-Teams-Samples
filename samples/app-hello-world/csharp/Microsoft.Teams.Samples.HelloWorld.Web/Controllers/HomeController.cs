// <copyright file="HomeController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// HomeController class to handle web requests for the home page and other routes.
    /// </summary>
    [Route("")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Handles the default route and returns the Index view.
        /// </summary>
        /// <returns>The Index view.</returns>
        [Route("")]
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Handles the /hello route and returns the Index view.
        /// </summary>
        /// <returns>The Index view.</returns>
        [Route("hello")]
        public ActionResult Hello()
        {
            return this.View("Index");
        }

        /// <summary>
        /// Handles the /first route and returns the First view.
        /// </summary>
        /// <returns>The First view.</returns>
        [Route("first")]
        public ActionResult First()
        {
            return this.View();
        }

        /// <summary>
        /// Handles the /second route and returns the Second view.
        /// </summary>
        /// <returns>The Second view.</returns>
        [Route("second")]
        public ActionResult Second()
        {
            return this.View();
        }

        /// <summary>
        /// Handles the /configure route and returns the Configure view.
        /// </summary>
        /// <returns>The Configure view.</returns>
        [Route("configure")]
        public ActionResult Configure()
        {
            return this.View();
        }
    }
}