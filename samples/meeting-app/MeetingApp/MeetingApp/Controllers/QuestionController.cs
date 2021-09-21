using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using MeetingApp.Data.Repositories.Questions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Controllers
{
    [Route("api/Question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private IQuestionsRepository _questionsRepository;

        public QuestionController(IQuestionsRepository questionsRepository)
        {
            _questionsRepository = questionsRepository;
        }

        [Route("insertQuest")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestion([FromBody]List<QuestionSetEntity> questions)
        {
            var result = await this._questionsRepository.StoreQuestionEntityAsync(questions);
            if (result != true) return NotFound();
            return Ok(result);
        }

        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> EditQuestion([FromBody]QuestionSetEntity question)
        {
            var result = await this._questionsRepository.UpdateQuestionEntityAsync(question);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion([FromBody]QuestionSetEntity question)
        {
            try
            {
                var result = await this._questionsRepository.DeleteQuestion(question);
                if (result != (int)HttpStatusCode.NoContent) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestionsSet(string meetingId)
        {
            var questions = await this._questionsRepository.GetQuestions(meetingId);
            if (questions == null) return NotFound();
            return Ok(questions);
        }
    }
}
