using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingLiveCoding.Models;
using System.Collections.Concurrent;

namespace MeetingLiveCoding.Controllers
{
    [Route("api/editorState")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly ConcurrentDictionary<string, List<MeetingDetails>> _meetingDetails;

        public HomeController(
            ConcurrentDictionary<string, List<MeetingDetails>> meetingDetails)
        {
            _meetingDetails = meetingDetails;
        }

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
                var currentMeetingList = new List<MeetingDetails>();
                var meetingList = new List<MeetingDetails>();
                _meetingDetails.TryGetValue("meetingDetails", out currentMeetingList);

                if (currentMeetingList == null)
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

                    meetingList.Add(meetingDetails);
                    _meetingDetails.AddOrUpdate("meetingDetails", meetingList, (key, newvalue) => currentMeetingList);
                    latestValue.Value = null;
                }

                else
                {
                    var data = currentMeetingList.Find(e => e.MeetingId == meetingId);
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
                        currentMeetingList.Add(meetingDetails);
                        _meetingDetails.AddOrUpdate("meetingDetails", currentMeetingList, (key, newvalue) => currentMeetingList);
                    }

                    else
                    {
                        var meetingIndex = currentMeetingList.FindIndex(e => e.MeetingId == meetingId);
                        var questionIndex = currentMeetingList[meetingIndex].Questions.FindIndex(e => e.QuestionId == questionId);
                        latestValue.Value = currentMeetingList[meetingIndex].Questions[questionIndex].Value;
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
                var meetingList = new List<MeetingDetails>();
                _meetingDetails.TryGetValue("meetingDetails", out meetingList);
                var data = meetingList.Find(e => e.MeetingId == meetingData.MeetingId);
                var meetingIndex = meetingList.FindIndex(e => e.MeetingId == meetingData.MeetingId);
                var questionIndex = meetingList[meetingIndex].Questions.FindIndex(e => e.QuestionId == meetingData.QuestionId);
                meetingList[meetingIndex].Questions[questionIndex].Value = meetingData.Description;

                _meetingDetails.AddOrUpdate("meetingDetails", meetingList, (key, newvalue) => meetingList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
