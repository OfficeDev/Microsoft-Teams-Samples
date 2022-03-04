// <copyright file="Profiles.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.Mappings
{
    using AutoMapper;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// AutoMapper Profile to map Entitites to Domain Objects and vice-versa.
    /// </summary>
    public class Profiles : AutoMapper.Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Profiles"/> class.
        /// </summary>
        public Profiles()
        {
            // Meeting <-> Graph Online Meeting
            this.CreateMap<Meeting, OnlineMeeting>(MemberList.Source)
                .ForSourceMember(s => s.Participants, e => e.DoNotValidate())
                .ForSourceMember(s => s.ChatId, e => e.DoNotValidate())
                .ForMember(d => d.Participants, e => e.Ignore());

            this.CreateMap<OnlineMeeting, Meeting>(MemberList.Destination)
                .ForMember(d => d.Participants, e => e.Ignore())
                .ForMember(d => d.StartDateTime, e => e.MapFrom(s => s.StartDateTime.Value.UtcDateTime))
                .ForMember(d => d.EndDateTime, e => e.MapFrom(s => s.EndDateTime.Value.UtcDateTime))
                .ForMember(d => d.ChatId, e => e.MapFrom(s => s.ChatInfo.ThreadId));

            // ParticipantInfo <-> Graph Meeting Participant Info.
            this.CreateMap<LinkUnfurling.Domain.Entities.ParticipantInfo, MeetingParticipantInfo>(MemberList.Source)
                .ForMember(d => d.Identity, e => e.MapFrom(s =>
                new IdentitySet()
                {
                    User = new Identity
                    {
                        Id = s.AadId,
                    },
                }))
                .ForSourceMember(s => s.Role, e => e.DoNotValidate())
                .ForSourceMember(s => s.AadId, e => e.DoNotValidate()); // TODO try removing this.

            this.CreateMap<MeetingParticipantInfo, LinkUnfurling.Domain.Entities.ParticipantInfo>(MemberList.Destination)
                .ForMember(d => d.AadId, e => e.MapFrom(s => s.Identity.User.Id))
                .ForMember(d => d.Role, e => e.Ignore());

            // ParticipantRole <-> OnlineMeetingRole.
            this.CreateMap<ParticipantRole, OnlineMeetingRole>().ReverseMap();
        }
    }
}
