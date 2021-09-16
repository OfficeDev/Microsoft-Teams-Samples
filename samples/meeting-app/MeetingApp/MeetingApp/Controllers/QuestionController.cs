using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Controllers
{
    [Route("api/Question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        [Route("insertQuest")]
        [HttpPost]
        public async Task<IActionResult> GetCandidateDetailsById(QuestionSetEntity questions)
        {
            var result = "";
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
