// <copyright file="AppIconBadgingInMeeting.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using AdaptiveCards;
using AdaptiveCards.Templating;
using AppIconBadgingInMeetings.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppIconBadgingInMeetings
{
    /// <summary>
    /// Handles Teams activities related to app icon badging in meetings.
    /// </summary>
    public class AppIconBadgingInMeeting : TeamsActivityHandler
    {
        private readonly IConfiguration _config;

        public AppIconBadgingInMeeting(IConfiguration configuration)
        {
            _config = configuration;
        }

        /// <summary>
        /// Invoked when a message activity is received in chat.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value == null)
            {
                turnContext.Activity.RemoveRecipientMention();

                if (turnContext.Activity.Text.Trim() == "SendNotification")
                {
                    var meetingParticipantsList = new List<ParticipantDetail>();

                    // Get the list of members that are part of the meeting group.
                    var participants = await TeamsInfo.GetPagedMembersAsync(turnContext).ConfigureAwait(false);

                    var meetingId = turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");

                    foreach (var member in participants.Members)
                    {
                        var participantDetails = await TeamsInfo.GetMeetingParticipantAsync(turnContext, meetingId, member.AadObjectId, _config["TenantId"]).ConfigureAwait(false);

                        // Select only those members that are present when the meeting is started.
                        if (participantDetails.Meeting.InMeeting.HasValue && participantDetails.Meeting.InMeeting.Value)
                        {
                            var meetingParticipant = new ParticipantDetail { Id = member.Id, Name = member.GivenName };
                            meetingParticipantsList.Add(meetingParticipant);
                        }
                    }

                    var meetingNotificationDetails = new MeetingNotification
                    {
                        ParticipantDetails = meetingParticipantsList
                    };

                    // Send an adaptive card to the user to select members for sending targeted notifications.
                    var adaptiveCardAttachment = GetAdaptiveCardAttachment("SendTargetNotificationCard.json", meetingNotificationDetails);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Please type `SendNotification` to send in-meeting notifications."), cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await HandleActions(turnContext, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handles different notification actions in the meeting.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task HandleActions(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var action = JsonConvert.DeserializeObject<ActionBase>(turnContext.Activity.Value.ToString());

            switch (action.Type)
            {
                case "AppIconBadging":
                    try
                    {
                        var actionSet = JsonConvert.DeserializeObject<ActionBase>(turnContext.Activity.Value.ToString());
                        var selectedMembers = actionSet.Choice;
                        var meetingId = turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");
                        var notification = GetAppIconBadging(selectedMembers.Split(',').ToList());

                        await TeamsInfo.SendMeetingNotificationAsync(turnContext, notification, meetingId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        Console.WriteLine(ex);
                    }
                    break;

                case "StageViewNotification":
                    try
                    {
                        var actionSet = JsonConvert.DeserializeObject<ActionBase>(turnContext.Activity.Value.ToString());
                        var selectedMembers = actionSet.Choice;
                        var pageUrl = _config["BaseUrl"] + "/hello";
                        var meetingId = turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");
                        var notification = GetStageViewNotification(selectedMembers.Split(',').ToList(), pageUrl);

                        await TeamsInfo.SendMeetingNotificationAsync(turnContext, notification, meetingId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        Console.WriteLine(ex);
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Invoked when a new member is added to the conversation.
        /// </summary>
        /// <param name="membersAdded">List of members added to the conversation.</param>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to App Icon Badging and Stage View notification! Type 'SendNotification' to send in-meeting notification.";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Creates an adaptive card attachment.
        /// </summary>
        /// <param name="fileName">Name of the file containing the adaptive card.</param>
        /// <param name="cardData">The data that needs to be bound with the card.</param>
        /// <returns>An adaptive card attachment.</returns>
        private Attachment GetAdaptiveCardAttachment(string fileName, object cardData)
        {
            var templateJson = File.ReadAllText(Path.Combine("./Cards", fileName));
            var template = new AdaptiveCardTemplate(templateJson);

            var cardJson = template.Expand(cardData);
            var result = AdaptiveCard.FromJson(cardJson);

            // Get card from result
            var card = result.Card;

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Creates a targeted meeting notification for app icon badging.
        /// </summary>
        /// <param name="recipients">List of recipients.</param>
        /// <returns>Targeted meeting notification object.</returns>
        private TargetedMeetingNotification GetAppIconBadging(List<string> recipients)
        {
            var notification = new TargetedMeetingNotification
            {
                Type = "TargetedMeetingNotification",
                Value = new TargetedMeetingNotificationValue
                {
                    Recipients = recipients,
                    Surfaces = new List<Surface>
                        {
                            new MeetingTabIconSurface()
                        },
                }
            };

            return notification;
        }

        /// <summary>
        /// Creates a targeted meeting notification for stage view.
        /// </summary>
        /// <param name="recipients">List of recipients.</param>
        /// <param name="pageUrl">Page URL that will be loaded in the notification.</param>
        /// <returns>Targeted meeting notification object.</returns>
        private TargetedMeetingNotification GetStageViewNotification(List<string> recipients, string pageUrl)
        {
            var notification = new TargetedMeetingNotification
            {
                Type = "TargetedMeetingNotification",
                Value = new TargetedMeetingNotificationValue
                {
                    Recipients = recipients,
                    Surfaces = new List<Surface>
                        {
                            new MeetingStageSurface<TaskModuleContinueResponse>
                            {
                                ContentType = ContentType.Task,
                                Content = new TaskModuleContinueResponse
                                {
                                    Value = new TaskModuleTaskInfo
                                    {
                                        Title = "Stage View Notification",
                                        Height = 300,
                                        Width = 400,
                                        Url = pageUrl
                                    }
                                }
                            }
                        },
                }
            };

            return notification;
        }
    }
}