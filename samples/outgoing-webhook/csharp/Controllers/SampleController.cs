using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WebhookSampleBot.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebhookSampleBot.Controllers
{
    [ApiController]
    public class SampleController : ControllerBase
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns>The response activity.</returns>
        [HttpPost]
        [Route("api/Sample")]
        public async Task<Microsoft.Bot.Connector.Activity> GetData(int delay = 0)
        {
            try
            {
                Trace.TraceInformation("Received data. Delay:'{0}'", delay);
                string content;
                using (var reader = new StreamReader(Request.Body))
                {
                    content = await reader.ReadToEndAsync();
                }

                Microsoft.Bot.Connector.Activity sampleCustomBotResponseActivity = await this.GetSampleResponseActivity(messageContent: content, delay: delay);
                Trace.TraceInformation("Sending payload: " + JsonConvert.SerializeObject(sampleCustomBotResponseActivity));

                return sampleCustomBotResponseActivity;
            }
            catch (Exception ex)
            {
                return new Microsoft.Bot.Connector.Activity(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the data after authentication.
        /// </summary>
        /// <param name="id">The identifier used for authentication.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>The response activity.</returns>
        [HttpPost]
        [Route("api/authenticatedSample")]
        public async Task<Microsoft.Bot.Connector.Activity> GetDataAfterAuth(string id = null, int delay = 0)
        {
            try
            {
                Trace.TraceInformation("Received data. Id:'{0}', Delay:'{1}'", id, delay);
                string content;
                using (var reader = new StreamReader(Request.Body))
                {
                    content = await reader.ReadToEndAsync();
                }

                var authHeaderVal = AuthenticationHeaderValue.Parse(Request.Headers.Authorization.ToString());
                AuthResponse authResponse = AuthProvider.Validate(
                     authenticationHeaderValue: authHeaderVal,
                     messageContent: content,
                     claimedSenderId: id);

                Microsoft.Bot.Connector.Activity sampleCustomBotResponseActivity;
                if (authResponse.AuthSuccessful == false)
                {
                    Trace.TraceWarning(authResponse.ErrorMessage);
                    sampleCustomBotResponseActivity = new Microsoft.Bot.Connector.Activity();
                    sampleCustomBotResponseActivity.Text = "You are not authorized to call into this end point.";
                }
                else
                {
                    sampleCustomBotResponseActivity = await this.GetSampleResponseActivity(messageContent: content, delay: delay);
                }

                Trace.TraceInformation("Sending payload: " + JsonConvert.SerializeObject(sampleCustomBotResponseActivity));

                return sampleCustomBotResponseActivity;
            }
            catch (Exception ex)
            {
                return new Microsoft.Bot.Connector.Activity(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the sample response activity.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>The sample response activity.</returns>
        /// <exception cref="NotSupportedException">Empty message bodies are not supported.</exception>
        private async Task<Microsoft.Bot.Connector.Activity> GetSampleResponseActivity(string messageContent, int delay)
        {
            try
            {
                if (string.IsNullOrEmpty(messageContent))
                {
                    throw new NotSupportedException("Empty message bodies are not supported.");
                }

                await Task.Delay(delay * 1000);
                Microsoft.Bot.Connector.Activity incomingActivity = JsonConvert.DeserializeObject<Microsoft.Bot.Connector.Activity>(messageContent);
                Microsoft.Bot.Connector.Activity sampleResponseActivity = SampleResponseProvider.GetResponseActivity(incomingActivity);

                return sampleResponseActivity;
            }
            catch (Exception ex)
            {
                return new Microsoft.Bot.Connector.Activity(ex.ToString());
            }
        }

    }
}