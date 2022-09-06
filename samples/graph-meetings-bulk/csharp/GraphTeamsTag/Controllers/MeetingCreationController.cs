

namespace GraphTeamsTag.Controllers
{
    using GraphTeamsTag.Helper;
    using GraphTeamsTag.Models;
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
        public async Task<IActionResult> eventAsync([FromRoute] string userid, [FromBody]  List<MeetingCreation> meetingCreations)
        {
            if (meetingCreations != null)
            {
                await this.graphHelper.CreateOnlineMeetingAsync(userid, meetingCreations);
                return this.StatusCode(201);
            }
            else {
                return this.StatusCode(500);
             }
        }
        [HttpGet("api/eventlist/{teamId}")]
        public async Task<IActionResult> ListTeamTagAsync([FromRoute] string teamId)
        {
            try
            {
                var teamworkTagList = (await this.graphHelper.listofevents(teamId)).ToList();
                return this.Ok(teamworkTagList);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }
       

    }
}
