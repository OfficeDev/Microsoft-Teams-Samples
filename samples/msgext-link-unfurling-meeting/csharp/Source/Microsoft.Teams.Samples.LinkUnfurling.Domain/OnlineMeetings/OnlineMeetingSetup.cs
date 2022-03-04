// <copyright file="OnlineMeetingSetup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.OnlineMeetings
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Services;

    /// <summary>
    /// Meeting setup implementation.
    /// </summary>
    internal class OnlineMeetingSetup : IMeetingSetup
    {
        private readonly IMeetingsService meetingService;
        private readonly IConversationService conversationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlineMeetingSetup"/> class.
        /// </summary>
        /// <param name="meetingService">Meeting service.</param>
        /// <param name="conversationService">Conversation service.</param>
        public OnlineMeetingSetup(
            IMeetingsService meetingService,
            IConversationService conversationService)
        {
            this.meetingService = meetingService ?? throw new ArgumentNullException(nameof(meetingService));
            this.conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        }

        /// <inheritdoc/>
        public async Task<(string, string)> SetupReviewMeetingAsync(Meeting meeting, ConversationContext conversationContext, Resource resource, string userId)
        {
            ConversationContext meetingConversationContext = default;
            var joinMeetingLink = string.Empty;

            // If the action is invoked in a meeting group chat, read online meeting linked to chat and fetch join meeting url.
            // Note: Teams doesn't support adding application tabs to channel meetings in public ring at the time of writing.
            if (conversationContext.ConversationType == ConversationType.GroupChat && !string.IsNullOrEmpty(conversationContext.MeetingId))
            {
                meetingConversationContext = conversationContext;
                joinMeetingLink = await this.conversationService.GetJoinMeetingUrlForChatAsync(conversationContext, userId);
            }
            else
            {
                // else Create an online meeting if the request is made from a non-meeting conversation.
                // Note: This may return a meeting instance that was created earlier (with the same external id).
                var newMeeting = await this.CreateOnlineMeetingAsync(meeting, conversationContext, resource);
                joinMeetingLink = newMeeting.JoinUrl;
                meetingConversationContext = new ConversationContext()
                {
                    ConversationId = newMeeting.ChatId,
                    ConversationType = ConversationType.GroupChat,
                    IsMeetingConversation = true,
                    MeetingId = newMeeting.Id,
                    TeamId = string.Empty,
                };
            }

            // Setup application and tab.
            var tabId = await this.SetupAppAndTabAsync(meetingConversationContext, resource);
            return (joinMeetingLink, tabId);
        }

        private async Task<Meeting> CreateOnlineMeetingAsync(Meeting meetingRequest, ConversationContext conversationContext, Resource resource)
        {
            // Creates an online meeting.
            return await this.meetingService.CreateOnlineMeetingAsync(meetingRequest);
        }

        private async Task<string> SetupAppAndTabAsync(ConversationContext conversationContext, Resource resource)
        {
            // Install app to the meeting chat.
            await this.conversationService.AddApplicationToConversationAsync(conversationContext);

            // Add tab to the meeting chat.
            var tabInfo = new TabInfo()
            {
                DisplayName = resource.Name,
                ContentUrl = resource.Url,
                EntityId = resource.Id,
                RemoveUrl = string.Empty,
                WebsiteUrl = resource.Url,
            };
            return await this.conversationService.AddTabToConversationAsync(conversationContext, tabInfo);
        }
    }
}
