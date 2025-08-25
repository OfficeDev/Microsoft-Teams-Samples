using BotWithSharePointFileViewer.helper;
using BotWithSharePointFileViewer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System;

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
        public async Task<IActionResult> UploadFileToSharepoint()
        {
            try
            {
                // Validate that files were uploaded
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest("No file was uploaded");
                }

                IFormFile file = Request.Form.Files[0];
                
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file");
                }

                // Validate token exists
                if (!_token.TryGetValue("token", out TokenState token) || string.IsNullOrEmpty(token?.AccessToken))
                {
                    return Unauthorized("No valid access token found. Please authenticate first.");
                }

                using (var fileStream = file.OpenReadStream()) 
                {
                    var result = ConvertStreamToByteArray(fileStream);
                    Stream stream = new MemoryStream(result);
                    
                    // Fix the SharePoint tenant name format - use correct Graph API format
                    var tenantName = _configuration["SharepointTenantName"];
                    var siteName = _configuration["SharepointSiteName"];
                    
                    Console.WriteLine($"Uploading file: {file.FileName} to site: {siteName} on tenant: {tenantName}");
                    
                    await SharePointFileHelper.UploadFileInSharePointSite(
                        token.AccessToken, 
                        siteName, 
                        tenantName + ".sharepoint.com", 
                        file.FileName, 
                        stream);
                    
                    Console.WriteLine("File uploaded successfully");
                    return Ok("File uploaded successfully");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Authorization error: {ex.Message}");
                return Unauthorized($"Authorization failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.Message}");
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
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
