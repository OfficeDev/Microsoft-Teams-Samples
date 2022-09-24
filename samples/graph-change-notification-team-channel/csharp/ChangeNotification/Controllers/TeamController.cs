
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Controllers
{
    using ChangeNotification.Helper;
    using ChangeNotification.Helpers;
    using ChangeNotification.Model;
    using ChangeNotification.Model.Configuration;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : Controller
    {
        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<ApplicationConfiguration> botSettings;
        private readonly ILogger _logger;
        private readonly SubscriptionManager _subscriptionManager;

        public TeamController(IOptions<ApplicationConfiguration> botSettings,ILogger<NotificationsController> logger,SubscriptionManager subscriptionManager)
        {
            this.botSettings = botSettings;
            _logger = logger;
            _subscriptionManager = subscriptionManager;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Json(SubscriptionManager.Subscriptions.Select(s => new
            {
                Resource = s.Value.Resource,
                ExpirationDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(s.Value.ExpirationDateTime.Value.DateTime, "UTC", "India Standard Time").ToString("MM/dd/yyyy h:mm tt"),
                Expired = s.Value.ExpirationDateTime < DateTime.UtcNow
            }));
        }

        // Callback
        [Route("/api/webhookLifecyle")]
        [HttpPost]
        public ActionResult WebhookLifecyle([FromQuery] string validationToken = null)
        {
            if (validationToken != null)
            {
                // Ack the webhook subscription
                return Ok(validationToken);
            }
            else
            {
                _logger.LogCritical("To do -- handle authorization challenge");
                return null;
            }
        }

        /// <summary>
        /// Initialize subscription for team
        /// </summary>
        /// <param name="teamId" name="pageId></param>
        /// <returns>ResourceList</returns>
        [Route("{teamId}/{pageId}")]
        [HttpPost]
        public async Task<IActionResult> TeamAsync([FromRoute] string teamId, string pageId)
        {
            try
            {
                await this._subscriptionManager.InitializeAllSubscription(teamId, pageId);
                return this.Ok(GlobalVariable.teamResourceList);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        public async Task<ActionResult<string>> Post([FromQuery] string validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation($"Received Token: '{validationToken}'");

                return Ok(validationToken);
            }
            // handle notifications
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();
                var notifications = JsonConvert.DeserializeObject<Notifications>(content);
                var fetchTeamId = notifications.Items[0].ResourceData.ODataId;
                var TeamId = "";
                Match match = Regex.Match(fetchTeamId, @"'([^']*)");
                if (match.Success)
                {
                    string yourValue = match.Groups[1].Value;
                    TeamId = yourValue.Trim('\'');
                }
                foreach (var notification in notifications.Items)
                {
                    // Initialize with the private key that matches the encryptionCertificateId.
                    var data = DecryptionHelper.GetDecryptedContent(notification.EncryptedContent,this.botSettings.Value.CertificateThumbprint);
                    var TeamResource = JsonConvert.DeserializeObject<TeamResource>(data);
                    var changeType = notifications.Items[0].ChangeType;
                    var teamName = TeamResource.DisplayName;
                  
                    GlobalVariable.teamResourceList.Add(new TeamResource()
                    {
                        ChangeType = changeType,
                        DisplayName = teamName,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            return Ok(GlobalVariable.teamResourceList);
        }
    }
}

