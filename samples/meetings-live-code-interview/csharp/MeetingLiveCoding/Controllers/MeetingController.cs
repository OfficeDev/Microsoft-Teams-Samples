using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingLiveCoding.Models;
using System.Linq;

namespace MeetingLiveCoding.Controllers
{
    [Route("api/editorState")]
    [ApiController]
    public class MeetingController : Controller
    {
        /// <summary>
        /// Creating a static list for storing meeting details.
        /// </summary>
        public static List<MeetingDetails> _meetingDetails = new List<MeetingDetails>();

        /// <summary>
        /// Method to get all the questions set for a meeting.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMeetingDetails(string questionId, string meetingId)
        {
            try
            {
                var questions = meetingId;
                if (questions == null) return NotFound();

                var latestValue = new EditorValue();

                if (_meetingDetails == null)
                {
                    MeetingDetails meetingDetails = CreateNewMeeting(meetingId);

                    _meetingDetails.Add(meetingDetails);
                    latestValue.Value = null;
                }

                else
                {
                    var data = _meetingDetails.Find(e => e.MeetingId == meetingId);
                    if (data == null)
                    {
                        MeetingDetails meetingDetails = CreateNewMeeting(meetingId);

                        latestValue.Value = null;
                        _meetingDetails.Add(meetingDetails);
                    }

                    else
                    {
                        var questionData = data.Questions.FirstOrDefault(item => item.QuestionId == questionId);
                        latestValue.Value = questionData.Value;
                    }
                }

                    return Ok(latestValue);
                }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static MeetingDetails CreateNewMeeting(string meetingId)
        {
            return new MeetingDetails()
            {
                MeetingId = meetingId,
                Questions = new List<Question>
                        {
                           new Question()
                           {
                               QuestionId = "1",
                               Value = ""
                           },
                           new Question()
                           {
                               QuestionId = "2",
                               Value = ""
                           },
                           new Question()
                           {
                               QuestionId = "3",
                               Value = ""
                           }
                        }
            };
        }

        /// <summary>
        /// Method to update exting list of meeting data.
        /// </summary>
        /// <param name="meetingData"></param>
        /// <returns></returns>
        [Route("update")]
        [HttpPost]
        public void UpdateMeetingList([FromBody] PostMeetingData meetingData)
        {
            try
            {
                var data = _meetingDetails.FirstOrDefault(e => e.MeetingId == meetingData.MeetingId);
                if (data != null)
                {
                    var questionData = data.Questions.FirstOrDefault(item => item.QuestionId == meetingData.QuestionId);
                    questionData.Value = meetingData.EditorData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
