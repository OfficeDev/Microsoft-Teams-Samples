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
                        Subject = meeting.TopicName,
                        Attendees = new List<Attendee>()
                        {
                            new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = meeting.Participants
                                },
                                Type = AttendeeType.Required
                            }
                        },
                        Start = new DateTimeTimeZone
                        {
                            DateTime = meeting.StartDate,
                            TimeZone = "Asia/Kolkata"
                        },
                        End = new DateTimeTimeZone
                        {
                            DateTime = meeting.EndDate,
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
               .Select("subject,webLink,createdDateTime,body,bodyPreview,organizer,attendees,start,end,location,")
               .GetAsync();
        var meetingeventList = new List<MeetingCreationList>();
            do
            {
                IEnumerable<Event> teamCurrentPage = tags.CurrentPage;

                foreach (var tag in teamCurrentPage)
                {
                    meetingeventList.Add(new MeetingCreationList
                    {
                        Id = tag.Id,
                        CreatedDateTime=tag.CreatedDateTime,
                        TopicName = tag.Subject,
                        Organizer=tag.Organizer,
                        MeetingLink = tag.WebLink,
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
                while (tags.CurrentPage != null) ;
                return meetingeventList;
        }       
    
    }
}