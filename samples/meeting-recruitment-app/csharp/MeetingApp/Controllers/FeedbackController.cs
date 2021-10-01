using System;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using MeetingApp.Data.Repositories.Feedback;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Controllers
{
    /// <summary>
    /// Class for Feedback 
    /// </summary>
    [Route("api/Feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private IFeedbackRepository _feedbackRepository;

        public FeedbackController(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        /// <summary>
        /// Method to save the feedback details provided.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveFeedback([FromBody] FeedbackEntity feedback)
        {
            try
            {
                var result = await this._feedbackRepository.StoreFeedbackAsync(feedback);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
