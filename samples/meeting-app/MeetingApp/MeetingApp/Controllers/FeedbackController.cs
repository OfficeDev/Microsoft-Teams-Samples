using MeetingApp.Data.Models;
using MeetingApp.Data.Repositories.Feedback;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Controllers
{
    [Route("api/Feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private IFeedbackRepository _feedbackRepository;

        public FeedbackController(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        [HttpPost]
        public async Task<IActionResult> SaveFeedback([FromBody]FeedbackEntity feedback)
        {
            var result = await this._feedbackRepository.StoreFeedbackAsync(feedback);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
