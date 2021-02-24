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
        [Route("configure")]
        public ActionResult Index()
        {
            //get the Json filepath  
            string file = Path.GetFullPath("ConfigData/Regions.json");
            //deserialize JSON from file  
            string Json = System.IO.File.ReadAllText(file);
            var domainlist = JsonSerializer.Deserialize<Rootobject>(Json);

            return View(domainlist);
        }

        [Route("welcome")]
        public ActionResult Welcome(string selectedDomain)
        {
            if (string.IsNullOrEmpty(selectedDomain))
                return RedirectToAction("Index");

            ViewBag.selectedDomain = selectedDomain;
            return View();
        }

    }
}
