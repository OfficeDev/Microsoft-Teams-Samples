using MeetingApp.Data;
using MeetingApp.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Controllers
{
    [Route("api/Candidate")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private ICandidateRepository _candidateRepository;

        public CandidateController(ICandidateRepository candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCandidateDetailsById(string email)
        {
            var candidate = await this._candidateRepository.GetCandidateDetailsByEmail(email);
            if (candidate == null) return NotFound();
            return Ok(candidate);
        }
    }
}
