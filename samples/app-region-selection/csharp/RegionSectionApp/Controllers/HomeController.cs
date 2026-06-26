using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.BotBuilderSamples.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.BotBuilderSamples.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Displays the configuration page with the list of regions.
        /// </summary>
        [Route("configure")]
        public ActionResult Index()
        {
            // Get the JSON file path
            string file = Path.GetFullPath("ConfigData/Regions.json");

            // Deserialize JSON from file
            string json = System.IO.File.ReadAllText(file);
            var domainList = JsonSerializer.Deserialize<RootObject>(json);

            return View(domainList);
        }

        /// <summary>
        /// Displays the welcome page with the selected domain.
        /// </summary>
        /// <param name="selectedDomain">The selected domain.</param>
        [Route("welcome")]
        public ActionResult Welcome(string selectedDomain)
        {
            if (string.IsNullOrEmpty(selectedDomain))
                return RedirectToAction("Index");

            ViewBag.SelectedDomain = selectedDomain;
            return View();
        }
    }
}
