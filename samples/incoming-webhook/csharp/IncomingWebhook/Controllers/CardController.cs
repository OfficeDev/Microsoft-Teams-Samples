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
    [Route("api")]
    [ApiController]
    public class CardController : ControllerBase
    {
        public static string url = "";

        /// <summary>
        /// Method to send card to team using incoming webhook.
        /// </summary>
        /// <returns></returns>
        [HttpPost("Send")]
        public async Task SendCardAsync([FromBody] CardEntity cardEntity)
        {
            try
            {
                url = cardEntity.WebhookUrl;
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

        /// <summary>
        /// Method to save and send user response.
        /// </summary>
        /// <returns></returns>
        [HttpPost("Save")]
        public async Task SendResponse([FromBody] ResponseEntity entity)
        {
            try
            {
                string cardJson = @"{
                ""@type"": ""MessageCard"",
                ""summary"": ""Response Message"",
                ""sections"": [{ 
                ""activityTitle"": ""Welcome Message"",
                ""text"": ""Submitted response: "+entity.Comment+ @"""}]}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(cardJson, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
