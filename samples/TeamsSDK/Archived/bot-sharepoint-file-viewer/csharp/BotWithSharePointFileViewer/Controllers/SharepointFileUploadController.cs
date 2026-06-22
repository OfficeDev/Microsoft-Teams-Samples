using BotWithSharePointFileViewer.helper;
using BotWithSharePointFileViewer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.IO;

namespace BotWithSharePointFileViewer.Controllers
{
    /// <summary>
    /// Class for sharepoint file upload
    /// </summary>
    [Route("upload")]
    public class SharePointFileUploadController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, TokenState> _token;
        public readonly IConfiguration _configuration;
        public SharePointFileUploadController(
            ConcurrentDictionary<string, TokenState> token,IConfiguration configuration)
        {
            _token = token;
            _configuration = configuration;
        }

        /// <summary>
        /// This endpoint is called to save the updated changes for particular product based on id.
        /// </summary>
        [HttpPost]
        public  void UploadFileToSharepoint()
        {
            IFormFile file = Request.Form.Files[0];

            using (var fileStream = file.OpenReadStream()) {
                var result = ConvertStreamToByteArray(fileStream);
                Stream stream = new MemoryStream(result);
                var token = new TokenState();
                _token.TryGetValue("token", out token);

                SharePointFileHelper.UploadFileInSharePointSite(token.AccessToken, _configuration["SharepointSiteName"], _configuration["SharepointTenantName"] + ":", file.FileName, stream);
            };
        }

        // Method to convert stream into byte array.
        private static byte[] ConvertStreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
