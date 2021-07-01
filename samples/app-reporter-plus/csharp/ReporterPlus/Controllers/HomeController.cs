using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReporterPlus.Helpers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ReporterPlus.Controllers
{
    public class HomeController : Controller
    {
        public readonly string blobAccountName;
        public readonly string blobSasString;

        public HomeController(IConfiguration configuration) : base()
        {
            //Initializing Constants
            Constants.BlobContainerName = configuration["BlobContainerName"];
            Constants.BlobConnectionString = configuration["BlobConnectionString"];
            Constants.ServiceUrl = configuration["ServiceUrl"];
            Constants.MicrosoftAppId = configuration["MicrosoftAppId"];
            Constants.MicrosoftAppPassword = configuration["MicrosoftAppPassword"];
            this.blobAccountName = configuration["BlobAccountName"];
            this.blobSasString = configuration["BlobSAS_Token"];
        }

        [Route("Configure")]
        public IActionResult Configure()
        {
            return View();
        }

        [Route("BarCodePage")]
        public IActionResult BarCodePage()
        {
            return View();
        }

        [Route("Scanner")]
        public IActionResult Scanner()
        {
            return View();
        }

        [Route("ViewDetails")]
        public IActionResult ViewDetails()
        {
            return View();
        }

        [Route("LoadViewDetails")]
        public async Task<ActionResult<string>> LoadViewDetails(string reqID)
        {
            var blobData = BlobHelper.GetBlob(reqID, null).Result;
            var serializeResponse = JsonConvert.SerializeObject(blobData);
            var response = JsonConvert.SerializeObject(serializeResponse);
            return response;
        }

        [Route("BlobAccessDetails")]
        public async Task<List<string>> BlobAccessDetails()
        {
            List<string> blobAccessCred = new List<string>();
            blobAccessCred.Add(blobAccountName);
            blobAccessCred.Add(Constants.BlobContainerName);
            blobAccessCred.Add(blobSasString);
            return blobAccessCred;
        }

    }
}
