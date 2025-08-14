// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CallingBotSample.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CallingBotSample.Services.MicrosoftGraph
{
    public class OnlineMeetingService : IOnlineMeetingService
    {
        private readonly GraphServiceClient graphServiceClient;
        private readonly UsersOptions usersOptions;

        public OnlineMeetingService(GraphServiceClient graphServiceClient, IOptions<UsersOptions> usersOptions)
        {
            this.graphServiceClient = graphServiceClient;
            this.usersOptions = usersOptions.Value;
        }

        /// <inheritdoc/>
        public Task<OnlineMeeting> Create(string subject, IEnumerable<string> participantsIds)
        {
            var onlineMeeting = new OnlineMeeting
            {
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddMinutes(30),
                Subject = subject,
                Participants = new MeetingParticipants
                {
                    Attendees = participantsIds.Select(p => new MeetingParticipantInfo
                    {
                        Identity = new IdentitySet
                        {
                            User = new Identity
                            {
                                Id = p,
                            }
                        },
                        Role = OnlineMeetingRole.Presenter,
                    })
                }
            };

            // To call this API the user (UserIdWithAssignedOnlineMeetingPolicy) must have been granted an application access policy
            // https://learn.microsoft.com/en-us/graph/cloud-communication-online-meeting-application-access-policy
            return graphServiceClient.Users[usersOptions.UserIdWithAssignedOnlineMeetingPolicy].OnlineMeetings
                .Request()
                .AddAsync(onlineMeeting);
        }

        /// <inheritdoc/>
        public Task<OnlineMeeting> Get(string meetingId)
        {
            return graphServiceClient.Users[usersOptions.UserIdWithAssignedOnlineMeetingPolicy].OnlineMeetings[meetingId]
                .Request()
                .GetAsync();
        }
    }
}
