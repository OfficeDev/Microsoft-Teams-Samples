using AppInMeeting.Models;
using AppInMeeting.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInMeeting.Controllers
{
    public class HomeController : Controller
    {
        private readonly TasksService _taskService;
        public HomeController(TasksService taskService)
        {
            _taskService = taskService;
        }

        [Route("appInMeeting")]
        public IActionResult AppInMeeting()
        {
            return View();
        }

        [Route("taskInfo")]
        public IActionResult TaskInfo()
        {
            return View();
        }

        [Route("todo")]
        public IActionResult ToDo()
        {
            return PartialView();
        }

        [Route("doing")]
        public IActionResult Doing()
        {
            return PartialView();
        }

        [Route("done")]
        public IActionResult Done()
        {
            return PartialView();
        }

        [Route("getMeetingData")]
        public List<TaskInfoModel> GetMeetingData([FromQuery] string meetingId, [FromQuery] string status)
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
            return currentMeetingList;
        }
    }
}
