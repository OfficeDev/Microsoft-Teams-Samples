
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AppCompleteAuth.Controllers
{
    /// <summary>
    /// Class for sharepoint file upload
    /// </summary>
    [Route("upload")]
    public class TabController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public TabController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
