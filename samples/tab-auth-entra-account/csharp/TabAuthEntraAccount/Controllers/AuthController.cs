using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TabAuthEntraAccount.Controllers
{
    public class AuthController : Controller
    {

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public AuthController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Authentication Start page for Microsoft Entra ID
        /// </summary>
        /// <returns>current View</returns>
        public IActionResult AuthStart()
        {
            ViewBag.AzureClientId = _configuration["AzureAd:ClientId"];
            return View();
        }

        /// <summary>
        /// Authentication End page for Microsoft Entra ID
        /// </summary>
        /// <returns>current View</returns>
        public IActionResult AuthEnd()
        {
            return View();
        }
    }
}