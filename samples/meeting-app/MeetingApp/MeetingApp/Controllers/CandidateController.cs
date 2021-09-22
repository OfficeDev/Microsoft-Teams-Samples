using MeetingApp.Data;
using MeetingApp.Data.Repositories;
using MeetingApp.Graph;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MeetingApp.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> GetCandidateDetailsById()
        {
            var candidates = await this._candidateRepository.GetCandidateDetails();
            if (candidates == null) return NotFound();
            return Ok(candidates);
        }

        [HttpGet("file")]
        public async Task<ActionResult> DownloadFile()
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
    }
}
