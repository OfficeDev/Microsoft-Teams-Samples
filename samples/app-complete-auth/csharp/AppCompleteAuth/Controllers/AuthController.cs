using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppCompleteAuth.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public AuthController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Authentication start
        /// </summary>
        /// <returns>current View</returns>
        public IActionResult Start()
        {
            ViewBag.AzureClientId = _configuration["MicrosoftAppId"];
            return View();
        }

        /// <summary>
        /// Authentication End
        /// </summary>
        /// <returns>current View</returns
        public IActionResult End()
        {
            return View();
        }
    }
}
