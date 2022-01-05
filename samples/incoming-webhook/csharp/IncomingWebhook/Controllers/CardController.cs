using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IncomingWebhook.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace IncomingWebhook.Controllers
{
    /// <summary>
    /// Controller class for sending card using incoming webhook.
    /// </summary>
    [Route("api/Send")]
    [ApiController]
    public class CardController : ControllerBase
    {

        /// <summary>
        /// Method to send card to team using incoming webhook.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task SendCardAsync([FromBody] CardEntity cardEntity)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(cardEntity.CardBody, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(cardEntity.WebhookUrl, content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
