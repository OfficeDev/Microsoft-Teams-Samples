// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingEvents.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using MeetingEvents.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    public class ActivityBot : TeamsActivityHandler
    {
        private BotState _conversationState;

        public ActivityBot(ConversationState conversationState)
        {
            _conversationState = conversationState;
        }

        /// <summary>
        /// Activity Handler for Meeting Participant join event
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnTeamsMeetingParticipantsJoinAsync(MeetingParticipantsEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(createAdaptiveCardInvokeResponseAsync(meeting.Members[0].User.Name, " has joined the meeting.")));
            return;
        }

        /// <summary>
        /// Activity Handler for Meeting Participant leave event
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnTeamsMeetingParticipantsLeaveAsync(MeetingParticipantsEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(createAdaptiveCardInvokeResponseAsync(meeting.Members[0].User.Name, " left the meeting.")));
            return;
        }

        /// <summary>
        /// Sample Adaptive card for Meeting participant events.
        /// </summary>
        private Attachment createAdaptiveCardInvokeResponseAsync(string userName, string action)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveRichTextBlock
                    {
                        Inlines = new List<AdaptiveInline>
                        {
                            new AdaptiveTextRun
                            {
                                Text = userName,
                                Weight = AdaptiveTextWeight.Bolder,
                                Size = AdaptiveTextSize.Default,
                            },
                            new AdaptiveTextRun
                            {
                                Text = action,
                                Weight = AdaptiveTextWeight.Default,
                                Size = AdaptiveTextSize.Default,
                            }
                        },
                    Spacing = AdaptiveSpacing.Medium,
                    }
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Activity Handler for Meeting start event
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnTeamsMeetingStartAsync(MeetingStartEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            // Save any state changes that might have occurred during the turn.
            var conversationStateAccessors = _conversationState.CreateProperty<MeetingData>(nameof(MeetingData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingData());
            conversationData.StartTime = meeting.StartTime;
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForMeetingStart(meeting)));
        }

        /// <summary>
        /// Activity Handler for Meeting end event.
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnTeamsMeetingEndAsync(MeetingEndEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<MeetingData>(nameof(MeetingData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingData());
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForMeetingEnd(meeting, conversationData)));
        }

        /// <summary>
        /// Sample Adaptive card for Meeting Start event.
        /// </summary>
        private Attachment GetAdaptiveCardForMeetingStart(MeetingStartEventDetails meeting)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = meeting.Title  + "- started",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "Start Time : ",
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Convert.ToString(meeting.StartTime.ToLocalTime()),
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = "Join meeting",
                        Url = meeting.JoinUrl,
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for Meeting End event.
        /// </summary>
        private Attachment GetAdaptiveCardForMeetingEnd(MeetingEndEventDetails meeting, MeetingData conversationData)
        {

            TimeSpan meetingDuration = meeting.EndTime - conversationData.StartTime;
            var meetingDurationText = meetingDuration.Minutes < 1 ?
                  Convert.ToInt32(meetingDuration.Seconds) + "s"
                : Convert.ToInt32(meetingDuration.Minutes) + "min " + Convert.ToInt32(meetingDuration.Seconds) + "s";

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = meeting.Title  + "- ended",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                     new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "End Time : ",
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = "Total duration : ",
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Convert.ToString(meeting.EndTime.ToLocalTime()),
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = meetingDurationText,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    },
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}