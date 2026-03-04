// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Controllers
{
    using ChangeNotification.Helper;
    using ChangeNotification.Helpers;
    using ChangeNotification.Model;
    using ChangeNotification.Model.Configuration;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;


    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : Controller
    {
        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<ApplicationConfiguration> botSettings;
        private readonly ILogger _logger;
        private readonly SubscriptionManager _subscriptionManager;

        public NotificationsController(IOptions<ApplicationConfiguration> botSettings, ILogger<NotificationsController> logger, SubscriptionManager subscriptionManager)
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

        /// <summary>
        /// Get channel members list
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="channelId">Channel ID</param>
        /// <returns>Channel members</returns>
        [HttpGet("members/{teamId}/{channelId}")]
        public async Task<ActionResult> GetChannelMembers(string teamId, string channelId)
        {
            try
            {
                var memberKey = $"{teamId}-{channelId}";
                
                // Get fresh member list from Graph API
                var members = await _subscriptionManager.GetChannelMembers(teamId, channelId);
                
                // Update local cache
                SubscriptionManager.ChannelMembersList[memberKey] = members;
                
                return Json(new
                {
                    teamId,
                    channelId,
                    members,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Error getting channel members");
                return StatusCode(500, new
                {
                    error = "Failed to get channel members",
                    message = error.Message
                });
            }
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
        /// Initialize subscription for channel
        /// </summary>
        /// <param name="teamId" name="pageId></param>
        /// <returns>ResourceList</returns>
        [Route("{teamId}/{pageId}/{channelId}")]
        [HttpPost]
        public async Task<IActionResult> channelAsync([FromRoute] string teamId, string pageId, string channelId)
        {
            try
            {
                await this._subscriptionManager.InitializeAllSubscription(teamId, pageId, channelId);
                return this.Ok(GlobalVariable.channelResourceList);

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
                    var data = DecryptionHelper.GetDecryptedContent(notification.EncryptedContent,
                                                    this.botSettings.Value.CertificateThumbprint);

                    var channelResource = JsonConvert.DeserializeObject<ChannelResource>(data);
                    var var_changeType = notifications.Items[0].ChangeType;
                    var channelName = channelResource.DisplayName;
                    
                    // Extract teamId and channelId from @odata.id
                    var odataId = notification.ResourceData.ODataId;
                    string teamId = null, channelId = null;
                    if (!string.IsNullOrEmpty(odataId))
                    {
                        var teamMatch = Regex.Match(odataId, @"teams\('([^']+)'\)");
                        var channelMatch = Regex.Match(odataId, @"channels\('([^']+)'\)");
                        teamId = teamMatch.Success ? teamMatch.Groups[1].Value : null;
                        channelId = channelMatch.Success ? channelMatch.Groups[1].Value : null;
                    }

                    // Get userId and tenantId from decrypted data
                    var userId = channelResource.UserId;
                    var tenantId = channelResource.TenantId;

                    bool shouldUpdateMemberList = true;
                    bool hasAccess = false;

                    if (!string.IsNullOrEmpty(teamId) && !string.IsNullOrEmpty(channelId))
                    {
                        var channelResourceItem = new ChannelResource()
                        {
                            ChangeType = var_changeType,
                            DisplayName = channelName,
                            CreatedDate = DateTime.Now,
                            TeamId = teamId,
                            ChannelId = channelId
                        };

                        // Handle different change types with conditional member list updates
                        if (var_changeType == "deleted" && !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(tenantId))
                        {
                            // Call the access check method
                            hasAccess = await _subscriptionManager.CheckUserChannelAccess(teamId, channelId, userId, tenantId);
                            channelResourceItem.UserHaveAccess = hasAccess;

                            // Skip member list update if user still has access
                            shouldUpdateMemberList = !hasAccess;

                            if (hasAccess)
                            {
                                _logger.LogInformation($"Skipping member list update for user {userId} - user still has access");
                            }
                            else
                            {
                                _logger.LogInformation($"User {userId} no longer has access - updating member list");
                            }
                        }

                        // Handle shared/unshared events
                        if (var_changeType == "created" && !string.IsNullOrEmpty(channelName))
                        {
                            // Shared event - update member list
                            shouldUpdateMemberList = true;
                            _logger.LogInformation($"Channel shared with team {channelName} - updating member list");
                        }

                        if (var_changeType == "deleted" && !string.IsNullOrEmpty(channelName) && !hasAccess)
                        {
                            // Unshared event - update member list
                            shouldUpdateMemberList = true;
                            _logger.LogInformation($"Channel unshared from team {channelName} - updating member list");
                        }

                        // Update member list conditionally
                        if (shouldUpdateMemberList)
                        {
                            try
                            {
                                var memberKey = $"{teamId}-{channelId}";
                                var updatedMembers = await _subscriptionManager.GetChannelMembers(teamId, channelId);
                                SubscriptionManager.ChannelMembersList[memberKey] = updatedMembers;

                                channelResourceItem.MemberListUpdated = true;
                                channelResourceItem.CurrentMemberCount = updatedMembers.Count;

                                _logger.LogInformation($"Member list updated for {memberKey}. Current count: {updatedMembers.Count}");
                            }
                            catch (Exception error)
                            {
                                _logger.LogError(error, "Error updating member list");
                                channelResourceItem.MemberListUpdateError = error.Message;
                            }
                        }
                        else
                        {
                            channelResourceItem.MemberListUpdated = false;
                            if (var_changeType == "deleted" && hasAccess)
                            {
                                channelResourceItem.MemberListSkipReason = "User still has access";
                            }
                        }

                        GlobalVariable.channelResourceList.Add(channelResourceItem);
                    }
                }
            }
            return Ok(GlobalVariable.channelResourceList);
        }
    }
}