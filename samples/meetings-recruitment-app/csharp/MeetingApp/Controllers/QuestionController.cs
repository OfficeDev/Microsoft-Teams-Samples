using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using MeetingApp.Data.Repositories.Questions;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Controllers
{
    /// <summary>
    /// Class for Questions
    /// </summary>
    [Route("api/Question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private IQuestionsRepository _questionsRepository;

        public QuestionController(IQuestionsRepository questionsRepository)
        {
            _questionsRepository = questionsRepository;
        }

        /// <summary>
        /// Method to add a new question.
        /// </summary>
        /// <param name="questions"></param>
        /// <returns></returns>
        [Route("insertQuest")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestion([FromBody] List<QuestionSetEntity> questions)
        {
            try
            {
                var result = await this._questionsRepository.StoreQuestionEntityAsync(questions);
                if (result != true) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to edit an existing question.
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> EditQuestion([FromBody] QuestionSetEntity question)
        {
            try
            {
                var result = await this._questionsRepository.UpdateQuestionEntityAsync(question);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to delete an existing question.
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion([FromBody] QuestionSetEntity question)
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

        /// <summary>
        /// Method to get all the questions set for a meeting.
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetQuestionsSet(string meetingId)
        {
            try
            {
                var questions = await this._questionsRepository.GetQuestions(meetingId);
                if (questions == null) return NotFound();
                return Ok(questions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
