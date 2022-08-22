// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChannelNotification.Controllers
{
    using ChannelNotification.Helper;
    using ChannelNotification.Helpers;
    using ChannelNotification.Model;
    using ChannelNotification.Model.Configuration;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;


    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : Controller
    {
        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<BotConfiguration> botSettings;
        

        private readonly ILogger _logger;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
       

        public NotificationsController(IOptions<BotConfiguration> botSettings,
            ILogger<NotificationsController> logger,
            IBotFrameworkHttpAdapter adapter,
            ConcurrentDictionary<string, ConversationReference> conversationReferences,SubscriptionManager subscriptionManager)
        {
            this.botSettings = botSettings;
            _logger = logger;
            _adapter = adapter;
            _conversationReferences = conversationReferences;
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
        [Route("{teamId}")]
        [HttpPost]
        public async Task<IActionResult> GetTeamAsync([FromRoute] string teamId)
        {
            try
            {
               //var teamID = teamId;
                await this._subscriptionManager.InitializeAllSubscription(teamId);
                //return this.StatusCode(200);
               return this.Ok(GlobalVariable.listchannelResource);
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
                    var channelId = channelResource.Id;
                    var channelName = channelResource.DisplayName;
                    GlobalVariable.listchannelResource.Add(new ChannelResource() {
                        ChangeType = var_changeType, 
                        DisplayName = channelName });


                    #region
                    //if (_conversationReferences.TryGetValue(TeamId, out var channelConversationReference))
                    //{
                    //    //if (var_changeType != "")
                    //    //{
                    //    //    switch (var_changeType)
                    //    //    {
                    //    //        case "created":


                    //    //            //await ((BotAdapter)_adapter).ContinueConversationAsync(this.botSettings.Value.MicrosoftAppId, channelConversationReference,
                    //    //            // async (turnContext, cancellationToken) =>
                    //    //            // {
                    //    //            //     await turnContext.SendActivityAsync(MessageFactory.Attachment(NotificationCardHelper.GetChannelCreateCard(channelName)), cancellationToken);

                    //    //            // },
                    //    //            // default);

                    //    //            break;
                    //    //        case "updated":
                    //    //            GlobalVariable.listchannelResource.Add(new ChannelResource() { ChangeType = var_changeType, DisplayName = channelName });
                    //    //            //GlobalVariable.dictnotification.Add(channelResource.ChangeType, channelResource);
                    //    //            //GlobalVariable.dictnotification.Add(channelResource.DisplayName, channelResource);

                    //    //         //   await ((BotAdapter)_adapter).ContinueConversationAsync(this.botSettings.Value.MicrosoftAppId, channelConversationReference,
                    //    //         //   async (turnContext, cancellationToken) =>
                    //    //         //    {
                    //    //         //        await turnContext.SendActivityAsync(MessageFactory.Attachment(NotificationCardHelper.GetChannelUpdateCard(channelName)), cancellationToken);

                    //    //         //    },
                    //    //         //default);
                    //    //            break;
                    //    //        case "deleted":
                    //    //            GlobalVariable.listchannelResource.Add(new ChannelResource() { ChangeType = var_changeType, DisplayName = channelName });
                    //    //            //GlobalVariable.dictnotification.Add(channelResource.ChangeType, channelResource);
                    //    //            //GlobalVariable.dictnotification.Add(channelResource.DisplayName, channelResource);

                    //    //         //   await ((BotAdapter)_adapter).ContinueConversationAsync(this.botSettings.Value.MicrosoftAppId, channelConversationReference,
                    //    //         //   async (turnContext, cancellationToken) =>
                    //    //         //   {
                    //    //         //       await turnContext.SendActivityAsync(MessageFactory.Attachment(NotificationCardHelper.GetChannelDeleteCard(channelName)), cancellationToken);

                    //    //         //   },
                    //    //         //default);
                    //    //            break;
                    //    //        default:
                    //    //            break;
                    //    //    }
                    //    //}
                    //}
                    #endregion
                }
                return Ok(GlobalVariable.listchannelResource);
            }
        }
    }
}