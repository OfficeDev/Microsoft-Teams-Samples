using AppInMeeting.Models;
using AppInMeeting.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInMeeting.Controllers
{
    [Route("api/{controller}")]
    public class MeetingController : Controller
    {
        private readonly TasksService _taskService;
        public MeetingController(TasksService taskService)
        {
            _taskService = taskService;
        }

        [Route("getMeetingData")]
        public IActionResult GetMeetingData([FromQuery] string meetingId, [FromQuery] string status)
        {
            var currentMeetingList = new List<TaskInfoModel>();

            if (status == "todo")
            {
                _taskService.ToDoDictionary.TryGetValue(meetingId, out currentMeetingList);
            }

            if (status == "doing")
            {
                _taskService.DoingDictionary.TryGetValue(meetingId, out currentMeetingList);
            }

            if (status == "done")
            {
                _taskService.DoneDictionary.TryGetValue(meetingId, out currentMeetingList);
            }

            if (currentMeetingList == null)
            {
                return this.Ok(new List<TaskInfoModel>());
            }
            else
            {
                return this.Ok(currentMeetingList);
            }
        }

        [Route("saveMeetingData")]
        [HttpPost]
        public IActionResult SaveMeetingData([FromQuery] string meetingId, [FromQuery] string status, [FromBody] TaskInfoModel taskInfo)
        {
            var currentMeetingList = new List<TaskInfoModel>();

            if (status == "todo")
            {
                var isPresent = _taskService.ToDoDictionary.TryGetValue(meetingId, out currentMeetingList);
                if (isPresent)
                {
                    currentMeetingList.Add(taskInfo);
                }
                else
                {
                    var newMeetingList = new List<TaskInfoModel> { taskInfo };
                    _taskService.ToDoDictionary.AddOrUpdate(meetingId, newMeetingList, (key, newValue) => newMeetingList);
                }
            }

            if (status == "doing")
            {
                var isPresent = _taskService.DoingDictionary.TryGetValue(meetingId, out currentMeetingList);
                if (isPresent)
                {
                    currentMeetingList.Add(taskInfo);
                    _taskService.DoingDictionary.AddOrUpdate(meetingId, currentMeetingList, (key, newValue) => currentMeetingList);
                }
                else
                {
                    var newMeetingList = new List<TaskInfoModel> { taskInfo };
                    _taskService.DoingDictionary.AddOrUpdate(meetingId, newMeetingList, (key, newValue) => newMeetingList);
                }
            }

            if (status == "done")
            {
                var isPresent = _taskService.DoneDictionary.TryGetValue(meetingId, out currentMeetingList);
                if (isPresent)
                {
                    currentMeetingList.Add(taskInfo);
                    _taskService.DoneDictionary.AddOrUpdate(meetingId, currentMeetingList, (key, newValue) => currentMeetingList);
                }
                else
                {
                    var newMeetingList = new List<TaskInfoModel> { taskInfo };
                    _taskService.DoingDictionary.AddOrUpdate(meetingId, newMeetingList, (key, newValue) => newMeetingList);
                }
            }

            return this.Ok(currentMeetingList);
        }
    }
}
