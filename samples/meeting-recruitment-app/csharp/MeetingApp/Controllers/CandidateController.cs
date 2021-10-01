using System;
using System.IO;
using System.Threading.Tasks;
using MeetingApp.Data.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace MeetingApp.Controllers
{
    /// <summary>
    /// Class with properties related to Candidate.
    /// </summary>
    [Route("api/Candidate")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private ICandidateRepository _candidateRepository;

        private IWebHostEnvironment _environment;

        public CandidateController(
            ICandidateRepository candidateRepository,
            IWebHostEnvironment environment)
        {
            _candidateRepository = candidateRepository;
            _environment = environment;
        }

        /// <summary>
        /// Method to get candidate details.
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCandidateDetails()
        {
            try
            {
                var candidates = await this._candidateRepository.GetCandidateDetails();
                if (candidates == null) return NotFound();
                return Ok(candidates);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to return file to be downloaded.
        /// </summary>
        /// <returns></returns>
        [HttpGet("file")]
        public async Task<ActionResult> DownloadFile()
        {
            try
            {
                var provider = new FileExtensionContentTypeProvider();
                var filePath = _environment.WebRootPath + "/test.pdf";
                if (!provider.TryGetContentType(filePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(bytes, contentType, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
