namespace GraphTeamsTag.Controllers
{
    using EventMeeting.Helper;
    using EventMeeting.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Graph;
    using System.Diagnostics;
    using System.Reflection.Metadata;


    [ApiController]
    public class MeetingCreationController : ControllerBase
    {
        private readonly ILogger<MeetingCreationController> _logger;
        private readonly GraphHelper graphHelper;
        public MeetingCreationController(ILogger<MeetingCreationController> logger, GraphHelper graphHelper)
        {
            _logger = logger;
            this.graphHelper = graphHelper;
        }

        [HttpPost("api/meeting/{userid}")]
        public async Task<IActionResult> MeetingCreate([FromRoute] string userid, [FromBody] List<Meeting> meeting)
        {
            if (meeting != null)
            {
                await this.graphHelper.CreateOnlineMeetingAsync(userid, meeting);
                return this.StatusCode(201);
            }
            else
            {
                return this.StatusCode(500);
            }
        }
        [HttpGet("api/eventlist/{teamId}")]
        public async Task<IActionResult> ListEventMeetingAsync([FromRoute] string teamId)
        {
            try
            {
                var meetingeventList = (await this.graphHelper.MeetingEventList(teamId)).ToList();
                return this.Ok(meetingeventList);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }     
    }
}