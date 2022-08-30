

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


        [HttpPost("api/meeting")]
        public async Task<IActionResult> eventAsync([FromBody]  List<MeetingCreation> obj)
        {
            if (obj != null)
            {
                //foreach(MeetingCreation meetingCreation in obj)
                //{
                //    string str =  meetingCreation.date +  + meetingCreation.timing + ;
                //    meetingCreation.date = str;
                //}
                await this.graphHelper.CreateOnlineMeetingAsync(obj);
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

    }
}
