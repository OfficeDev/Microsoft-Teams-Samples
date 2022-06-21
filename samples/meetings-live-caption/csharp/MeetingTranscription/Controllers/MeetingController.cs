using MeetingLiveCaption.Models;
using MeetingLiveCaption.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MeetingLiveCaption.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        public static string MeetingCARTURL;

        private readonly IOptions<MeetingSettings> meetingSetting;

        public MeetingController(IOptions<MeetingSettings> meetingSetting)
        {
            this.meetingSetting = meetingSetting;
        }

        /// <summary>
        /// Method to send card to team using incoming webhook.
        /// </summary>
        /// <returns></returns>
        [HttpPost("LiveCaption")]
        public async Task<IActionResult> LiveCaptionAsync([FromBody] LiveCaption liveCaption)
        {
            try
            {
                var data = new StringContent(liveCaption.CaptionText, Encoding.UTF8, "text/plain");
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(MeetingCARTURL, data);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return this.Ok();
                }
                else
                {
                    return this.BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return this.BadRequest();
            }
        }

        /// <summary>
        /// Method to send card to team using incoming webhook.
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCARTUrl")]
        public IActionResult SaveCartURL([FromBody] LiveCaption liveCaption)
        {
            try
            {
                if (!string.IsNullOrEmpty(liveCaption.CARTUrl.Trim()) && liveCaption.CARTUrl.Contains("meetingid") && liveCaption.CARTUrl.Contains("token"))
                {
                    MeetingCARTURL = liveCaption.CARTUrl;
                    return this.Ok();
                }
                else
                {
                    return this.BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return this.BadRequest();
            }
        }
    }
}
