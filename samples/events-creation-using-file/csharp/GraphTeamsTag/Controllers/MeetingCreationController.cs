

namespace GraphTeamsTag.Controllers
{
    using GraphTeamsTag.Helper;
    using GraphTeamsTag.Models;
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


        [HttpPost("api/metting")]
        public async Task<IActionResult> eventAsync([FromBody]  List<MeetingCreation> obj)
        {
            if (obj != null)
            {
                await this.graphHelper.CreateOnlineMeetingAsync(obj);
                return this.StatusCode(201);
            }
            else {
                return this.StatusCode(500);
             }

            
        }        

    }
}
