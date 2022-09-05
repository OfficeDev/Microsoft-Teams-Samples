

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
        public async Task<IActionResult> eventAsync([FromRoute] string userid, [FromBody]  List<MeetingCreation> obj)
        {
            if (obj != null)
            {
                await this.graphHelper.CreateOnlineMeetingAsync(userid, obj);
                return this.StatusCode(201);
            }
            else {
                return this.StatusCode(500);
             }

            
        }

        [HttpPost("api/meetings")]
        public async Task<IActionResult> meetingasync([FromBody] MeetingCreation obj)
        {
            if (obj != null)
            {
                await this.graphHelper.CreateOnlineMeetingUsingForms(obj);
                return this.StatusCode(201);
            }
            else
            {
                return this.StatusCode(500);
            }


        }
      
        //public async Task<IActionResult> eventList([FromRoute] string userid)
        //{
        //    try
        //    {
        //        var teamworkTagList = (await this.graphHelper.listofevents(userid)).ToList();
        //        return this.Ok(teamworkTagList);                
        //    }
        //    catch(Exception e)
        //    {
        //        return this.StatusCode(500);
        //    }
        //}
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
        /// <summary>
        /// Updates the event.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="teamTag">Updated details of the tag.</param>
        /// <returns>If success return 204 status code, otherwise 500 status code</returns>
        [HttpPatch("api/eventupdate/{id}")]
        public async Task<IActionResult> UpdateTeamTagAsync([FromRoute] string id, [FromBody] MeetingCreationUpdate meetingCreationUpdate)
        {
            try
            {
                await this.graphHelper.UpdateEventAsync(meetingCreationUpdate,id);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

    }
}
