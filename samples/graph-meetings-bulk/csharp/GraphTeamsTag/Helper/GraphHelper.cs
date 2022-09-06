// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Helper
{
    using GraphTeamsTag.Models;
    using GraphTeamsTag.Provider;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;

    public class GraphHelper
    {
        /// <summary>
        /// Creates graph client to call Graph Beta API.
        /// </summary>
        public readonly GraphServiceClient graphBetaClient;

        public GraphHelper(SimpleBetaGraphClient simpleBetaGraphClient)
        {
            this.graphBetaClient = simpleBetaGraphClient.GetGraphClientforApp();
        }


        public async Task<Event> CreateOnlineMeetingAsync(string userid, List<MeetingCreation> meetingCreations)
        {
            var @event = new Event
            {

            };
            foreach (MeetingCreation meetingCreationlist in meetingCreations)
            {
                try
                {
                    @event = new Event()
                    {
                        Subject = meetingCreationlist.topicName,
                        //Organizer=obj.trainerName,

                        Attendees = new List<Attendee>()
                        {
                            new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = meetingCreationlist.participants                                   
                                },
                                Type = AttendeeType.Required
                            }
                        },
                        Start = new DateTimeTimeZone
                        {
                            DateTime = meetingCreationlist.startdate,
                            TimeZone = "Asia/Kolkata"
                        },
                        End = new DateTimeTimeZone
                        {
                            DateTime = meetingCreationlist.enddate,
                            TimeZone = "Asia/Kolkata"
                        },
                        AllowNewTimeProposals = true,
                        IsOnlineMeeting = true,
                        OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness

                    };
                    await graphBetaClient.Users[userid].Events
                   .Request()
                   .Header("Prefer", "outlook.timezone=\"Asia/Kolkata\"")
                   .AddAsync(@event);
                }
                catch (Exception ex)
                {
                }

            }
            return null;

        }

        public async Task<IEnumerable<MeetingCreationList>> listofevents(string userid)
        {
            var tags = await graphBetaClient.Users[userid].Events
              .Request()
               .Header("Prefer", "outlook.timezone=\"Pacific Standard Time\"")
               .Select("subject,body,bodyPreview,organizer,attendees,start,end,location")
               .GetAsync();
            var teamworkTagList = new List<MeetingCreationList>();
            do
            {
                IEnumerable<Event> teamTagCurrentPage = tags.CurrentPage;

                foreach (var tag in teamTagCurrentPage)
                {
                    var teamworkTagMembersList = new List<TeamworkTagMember>();

                    teamworkTagList.Add(new MeetingCreationList
                    {
                        // Id = tag.Id,
                        id = tag.Id,
                        topicName = tag.Subject,
                        Start = tag.Start,
                        End = tag.End,
                        Attendees = tag.Attendees.ToList(),

                    });
                }

                // If there are more result.
                if (tags.NextPageRequest != null)
                {
                    tags = await tags.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }
            while (tags.CurrentPage != null);

            return teamworkTagList;
        }
    }
}
