// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
namespace WebhookSampleBot.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Sample controller for custom bot.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class SampleController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleController"/> class.
        /// </summary>
        public SampleController()
        {
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns>The response activity.</returns>
        [HttpPost]
        [Route("api/Sample")]
        public async Task<Activity> GetData(int delay = 0)
        {
            Trace.TraceInformation("Received data. Delay:'{0}'", delay);

            string content = await this.Request.Content.ReadAsStringAsync();
            Activity sampleCustomBotResponseActivity = await this.GetSampleResponseActivity(messageContent: content, delay: delay);

            Trace.TraceInformation("Sending payload: " + JsonConvert.SerializeObject(sampleCustomBotResponseActivity));
            return sampleCustomBotResponseActivity;
        }

        /// <summary>
        /// Gets the data after authentication.
        /// </summary>
        /// <param name="id">The identifier used for authentication.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>The response activity.</returns>
        [HttpPost]
        [Route("api/authenticatedSample")]
        public async Task<Activity> GetDataAfterAuth(string id = null, int delay = 0)
        {
            Trace.TraceInformation("Received data. Id:'{0}', Delay:'{1}'", id, delay);

            string content = await this.Request.Content.ReadAsStringAsync();

            AuthResponse authResponse = AuthProvider.Validate(
                authenticationHeaderValue: this.Request.Headers.Authorization,
                messageContent: content,
                claimedSenderId: id);

            Activity sampleCustomBotResponseActivity;
            if (authResponse.AuthSuccessful == false)
            {
                Trace.TraceWarning(authResponse.ErrorMessage);
                sampleCustomBotResponseActivity = new Activity();
                sampleCustomBotResponseActivity.Text = "You are not authorized to call into this end point.";
            }
            else
            {
                sampleCustomBotResponseActivity = await this.GetSampleResponseActivity(messageContent: content, delay: delay);
            }

            Trace.TraceInformation("Sending payload: " + JsonConvert.SerializeObject(sampleCustomBotResponseActivity));
            return sampleCustomBotResponseActivity;
        }

        /// <summary>
        /// Gets the sample response activity.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>The sample response activity.</returns>
        /// <exception cref="NotSupportedException">Empty message bodies are not supported.</exception>
        private async Task<Activity> GetSampleResponseActivity(string messageContent, int delay)
        {
            if (string.IsNullOrEmpty(messageContent))
            {
                throw new NotSupportedException("Empty message bodies are not supported.");
            }

            await Task.Delay(delay * 1000);
            Activity incomingActivity = JsonConvert.DeserializeObject<Activity>(messageContent);
            Activity sampleResponseActivity = SampleResponseProvider.GetResponseActivity(incomingActivity);

            return sampleResponseActivity;
        }
    }
}