using MeetingLiveCaption.Models;
using Microsoft.AspNetCore.Mvc;
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
        public static string MeetingCartUrl;

        /// <summary>
        /// Method to send caption in the live meeting.
        /// </summary>
        /// <returns>Return status OK if success, else return bad request.</returns>
        [HttpPost("LiveCaption")]
        public async Task<IActionResult> LiveCaptionAsync([FromBody] LiveCaption liveCaption)
        {
            try
            {
                var data = new StringContent(liveCaption.CaptionText, Encoding.UTF8, "text/plain");
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(MeetingCartUrl, data);

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
        /// Method to save cart url.
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCARTUrl")]
        public IActionResult SaveCartURL([FromBody] LiveCaption liveCaption)
        {
            try
            {
                if (!string.IsNullOrEmpty(liveCaption.CartUrl.Trim()) && liveCaption.CartUrl.Contains("meetingid") && liveCaption.CartUrl.Contains("token"))
                {
                    MeetingCartUrl = liveCaption.CartUrl;
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
