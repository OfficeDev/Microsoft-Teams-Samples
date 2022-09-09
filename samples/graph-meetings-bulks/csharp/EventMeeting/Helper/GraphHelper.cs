// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace EventMeeting.Helper
{
    using EventMeeting.Models;
    using EventMeeting.Provider;
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


        public async Task<Event> CreateOnlineMeetingAsync(string userid, List<Meeting> meetingCreations)
        {
            var @event = new Event
            {

            };
            foreach (Meeting meeting in meetingCreations)
            {
                try
                {
                    @event = new Event()
                    {
                        Subject = meeting.topicName,
                        Attendees = new List<Attendee>()
                        {
                            new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = meeting.participants,
                                    // Name = "Adele Vance"
                                },
                                Type = AttendeeType.Required
                            }
                        },
                        Start = new DateTimeTimeZone
                        {
                            DateTime = meeting.startdate,
                            TimeZone = "Asia/Kolkata"
                        },
                        End = new DateTimeTimeZone
                        {
                            DateTime = meeting.enddate,
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
                    Console.Write("Meeting event not created",ex);
                }

            }
            return null;
        }     
      
        public async Task<IEnumerable<MeetingCreationList>> MeetingEventList(string userid)
        {
            var tags = await graphBetaClient.Users[userid].Events
              .Request()
               .Header("Prefer", "outlook.timezone=\"Pacific Standard Time\"")
               .Select("subject,body,bodyPreview,organizer,attendees,start,end,location")
               .GetAsync();
            var meetingeventList = new List<MeetingCreationList>();
            do
            {
                IEnumerable<Event> teamCurrentPage = tags.CurrentPage;

                foreach (var tag in teamCurrentPage)
                {
                    meetingeventList.Add(new MeetingCreationList
                    {
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
            return meetingeventList;
        }       
    }
}