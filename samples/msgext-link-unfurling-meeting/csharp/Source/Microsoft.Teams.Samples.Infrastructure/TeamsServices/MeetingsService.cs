// <copyright file="MeetingsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Services;

    /// <summary>
    /// Meetings service implementation.
    /// </summary>
    public class MeetingsService : IMeetingsService
    {
        private readonly GraphServiceClient graphServiceClient;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingsService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <param name="mapper">Mapper.</param>
        public MeetingsService(
            GraphServiceClient graphServiceClient,
            IMapper mapper)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<Meeting> CreateOnlineMeetingAsync(Meeting meeting)
        {
            // Convert to online meeting.
            var meetingRequestObject = this.mapper.Map<OnlineMeeting>(meeting);
            meetingRequestObject.Participants = this.GetMeetingParticipants(meeting.Participants);

            // Create meeting resource.
            var onlineMeeting = await this.graphServiceClient.Me.OnlineMeetings
                .CreateOrGet(
                    meetingRequestObject.ExternalId,
                    null/*chatInfo*/,
                    meetingRequestObject.EndDateTime,
                    meetingRequestObject.Participants,
                    meetingRequestObject.StartDateTime,
                    meetingRequestObject.Subject)
                .Request()
                .WithMaxRetry(3)
                .PostAsync();

            // Map to meeting entity.
            var meetingEntity = this.mapper.Map<Meeting>(onlineMeeting);
            return meetingEntity;
        }

        /// <inheritdoc/>
        public async Task<Meeting> GetOnlineMeetingAsync(string externalId)
        {
            var onlineMeeting = await this.graphServiceClient.Me.OnlineMeetings
                .CreateOrGet(externalId)
                .Request()
                .WithMaxRetry(3)
                .PostAsync();

            var meeting = this.mapper.Map<Meeting>(onlineMeeting);
            return meeting;
        }

        private MeetingParticipants GetMeetingParticipants(IEnumerable<LinkUnfurling.Domain.Entities.ParticipantInfo> participants)
        {
            var meetingParticipants = new MeetingParticipants()
            {
                Attendees = this.mapper.Map<IEnumerable<MeetingParticipantInfo>>(participants.Where(p => p.Role == ParticipantRole.Attendee)),
                Organizer = this.mapper.Map<IEnumerable<MeetingParticipantInfo>>(participants.Where(p => p.Role == ParticipantRole.Presenter)).First(),
                Producers = this.mapper.Map<IEnumerable<MeetingParticipantInfo>>(participants.Where(p => p.Role == ParticipantRole.Producer)),
            };

            return meetingParticipants;
        }
    }
}
