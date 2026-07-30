using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using System.Diagnostics;
using Activity = Microsoft.Bot.Schema.Activity;
using System.Net.Http.Headers;
using System.Text.Json;
using WebhookSampleBot.Models;

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
        public async Task<Activity> GetData(int delay = 0)
        {
            try
            {
                Trace.TraceInformation("Received data. Delay:'{0}'", delay);
                string content;
                using (var reader = new StreamReader(Request.Body))
                {
                    content = await reader.ReadToEndAsync();
                }

                Activity sampleCustomBotResponseActivity = await GetSampleResponseActivity(messageContent: content, delay: delay);
                Trace.TraceInformation("Sending payload: " + JsonSerializer.Serialize(sampleCustomBotResponseActivity));

                return sampleCustomBotResponseActivity;
            }
            catch (Exception ex)
            {
                return new Activity { Text = ex.ToString() };
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
        public async Task<Activity> GetDataAfterAuth(string? id = null, int delay = 0)
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

                Activity sampleCustomBotResponseActivity;
                if (!authResponse.AuthSuccessful)
                {
                    Trace.TraceWarning(authResponse.ErrorMessage);
                    sampleCustomBotResponseActivity = new Activity { Text = "You are not authorized to call into this end point." };
                }
                else
                {
                    sampleCustomBotResponseActivity = await GetSampleResponseActivity(messageContent: content, delay: delay);
                }

                Trace.TraceInformation("Sending payload: " + JsonSerializer.Serialize(sampleCustomBotResponseActivity));

                return sampleCustomBotResponseActivity;
            }
            catch (Exception ex)
            {
                return new Activity { Text = ex.ToString() };
            }
        }

        /// <summary>
        /// Gets the sample response activity.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>The sample response activity.</returns>
        /// <exception cref="NotSupportedException">Empty message bodies are not supported.</exception>
        private static async Task<Activity> GetSampleResponseActivity(string messageContent, int delay)
        {
            try
            {
                if (string.IsNullOrEmpty(messageContent))
                {
                    throw new NotSupportedException("Empty message bodies are not supported.");
                }

                await Task.Delay(delay * 1000);
                Activity? incomingActivity = JsonSerializer.Deserialize<Activity>(messageContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Activity sampleResponseActivity = SampleResponseProvider.GetResponseActivity(incomingActivity!);

                return sampleResponseActivity;
            }
            catch (Exception ex)
            {
                return new Activity { Text = ex.ToString() };
            }
        }
    }
}