using System;
using Microsoft.AspNetCore.Mvc;
using PersonalTabMVC.Models;

namespace PersonalTabMVC.Controllers
{
    [Route("personaltab")]
    public class PersonalTabController : Controller
    {
        [Route("")]
        public IActionResult PersonalTab()
        {
            PersonalTab color = new PersonalTab();
            ViewBag.Gray = ($"{color.GetColor()} Gray!'");
            ViewBag.Red = ($"{color.GetColor()} Red!'");
            return View();
        }
    }
}