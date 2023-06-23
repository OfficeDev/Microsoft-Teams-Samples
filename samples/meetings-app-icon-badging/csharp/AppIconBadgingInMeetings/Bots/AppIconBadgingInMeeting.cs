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
    public class AppIconBadgingInMeeting : TeamsActivityHandler
    {
        private readonly IConfiguration _config;

        public AppIconBadgingInMeeting(IConfiguration configuration)
        {
            _config = configuration;
        }

        /// <summary>
        /// Invoked when a message activity is recieved in chat.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value == null)
            {
                turnContext.Activity.RemoveRecipientMention();

                if (turnContext.Activity.Text.Trim() == "SendNotification")
                {
                    var meetingPartipantsList = new List<ParticipantDetail>();

                    // Get the list of members that are part of meeting group.
                    var participants = await TeamsInfo.GetPagedMembersAsync(turnContext);

                    var meetingId = turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");

                    foreach (var member in participants.Members)
                    {
                        TeamsMeetingParticipant participantDetails = await TeamsInfo.GetMeetingParticipantAsync(turnContext, meetingId, member.AadObjectId, _config["TenandId"]).ConfigureAwait(false);

                        // Select only those members that present when meeting is started.
                        if (participantDetails.Meeting.InMeeting == true)
                        {
                            var meetingParticipant = new ParticipantDetail() { Id = member.Id, Name = member.GivenName };
                            meetingPartipantsList.Add(meetingParticipant);
                        }
                    }

                    var meetingNotificationDetails = new MeetingNotification
                    {
                        ParticipantDetails = meetingPartipantsList
                    };

                    // Send and adaptive card to user to select members for sending targeted notifications.
                    Attachment adaptiveCardAttachment = GetAdaptiveCardAttachment("SendTargetNotificationCard.json", meetingNotificationDetails);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Please type `SendNotification` to send In-meeting notifications."));
                }
            }
            else
            {
                await HandleActions(turnContext, cancellationToken);
            }
        }

        /// <summary>
        /// Method to handle different notification actions in meeting.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
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
                        TargetedMeetingNotification notification = GetAppIconBadging(selectedMembers.Split(',').ToList());

                        await TeamsInfo.SendMeetingNotificationAsync(turnContext, notification, meetingId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    break;

                case "SatgeViewNotification":
                    try
                    {
                        var actionSet = JsonConvert.DeserializeObject<ActionBase>(turnContext.Activity.Value.ToString());
                        var selectedMembers = actionSet.Choice;
                        var pageUrl = _config["BaseUrl"] + "/hello";
                        var meetingId = turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");
                        TargetedMeetingNotification notification = GetStageViewNotification(selectedMembers.Split(',').ToList(), pageUrl);

                        await TeamsInfo.SendMeetingNotificationAsync(turnContext, notification, meetingId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Invoked when new member is added to conversation.
        /// </summary>
        /// <param name="membersAdded">List of members added to the conversation.</param>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to App Icon Badging and Stage View notification! type 'SendNotification' to send in meeting notification";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Invoked when submit activity is recieved from task module.
        /// </summary>
        /// <param name="fileName">Name of the file containing adaptive card.</param>
        /// <param name="cardData">The data that needs to be binded with the card.</param>
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A an adaptive card attachment.</returns>
        private Attachment GetAdaptiveCardAttachment(string fileName, object cardData)
        {
            var templateJson = File.ReadAllText("./Cards/" + fileName);
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJson);

            string cardJson = template.Expand(cardData);
            AdaptiveCardParseResult result = AdaptiveCard.FromJson(cardJson);

            // Get card from result
            AdaptiveCard card = result.Card;

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Send App Icon Badging Notification to recipients
        /// </summary>
        /// <param name="recipients">List of members added to the conversation.</param>
        /// <returns>Target meeting notification object.</returns>
        private TargetedMeetingNotification GetAppIconBadging(List<string> recipients)
        {
            TargetedMeetingNotification notification = new TargetedMeetingNotification()
            {
                Type = "TargetedMeetingNotification",
                Value = new TargetedMeetingNotificationValue()
                {
                    Recipients = recipients,
                    Surfaces = new List<Surface>()
                    {
                        new MeetingTabIconSurface()
                    },
                }
            };

            return notification;
        }

        /// <summary>
        /// Send Stage View Notification to recipients
        /// </summary>
        /// <param name="recipients">List of members added to the conversation.</param>
        /// <param name="pageUrl">page url that will be load in the notification.</param>
        /// <returns>Target meeting notification object.</returns>
        private TargetedMeetingNotification GetStageViewNotification(List<string> recipients, string pageUrl)
        {
            TargetedMeetingNotification notification = new TargetedMeetingNotification()
            {
                Type = "TargetedMeetingNotification",
                Value = new TargetedMeetingNotificationValue()
                {
                    Recipients = recipients,
                    Surfaces = new List<Surface>()
                    {
                        new MeetingStageSurface<TaskModuleContinueResponse>()
                        {
                            ContentType = ContentType.Task,
                            Content = new TaskModuleContinueResponse
                            {
                                Value = new TaskModuleTaskInfo()
                                {
                                    Title = "Stage View Notification",
                                    Height =300,
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