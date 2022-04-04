using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingLiveCoding.Models;

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
                    var meetingDetails = new MeetingDetails()
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

                    _meetingDetails.Add(meetingDetails);
                    latestValue.Value = null;
                }

                else
                {
                    var data = _meetingDetails.Find(e => e.MeetingId == meetingId);
                    if (data == null)
                    {
                        var meetingDetails = new MeetingDetails()
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

                        latestValue.Value = null;
                        _meetingDetails.Add(meetingDetails);
                    }

                    else
                    {
                        var meetingIndex = _meetingDetails.FindIndex(e => e.MeetingId == meetingId);
                        var questionIndex = _meetingDetails[meetingIndex].Questions.FindIndex(e => e.QuestionId == questionId);
                        latestValue.Value = _meetingDetails[meetingIndex].Questions[questionIndex].Value;
                    }
                }

                    return Ok(latestValue);
                }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to update exting list of meeting data.
        /// </summary>
        /// <param name="meetingData"></param>
        /// <returns></returns>
        [Route("update")]
        [HttpPost]
        public void UpdateMeetingList(PostMeetingData meetingData)
        {
            try
            {
                var data = _meetingDetails.Find(e => e.MeetingId == meetingData.MeetingId);
                var meetingIndex = _meetingDetails.FindIndex(e => e.MeetingId == meetingData.MeetingId);
                var questionIndex = _meetingDetails[meetingIndex].Questions.FindIndex(e => e.QuestionId == meetingData.QuestionId);
                _meetingDetails[meetingIndex].Questions[questionIndex].Value = meetingData.Description;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
