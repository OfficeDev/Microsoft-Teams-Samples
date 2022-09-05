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
       
      
        public async Task<Event> CreateOnlineMeetingAsync(string userid,List<MeetingCreation> meetingCreations)
        {
            var @event=new Event{

            };
            foreach (MeetingCreation obj in meetingCreations)
            {
                try
                {
                    @event = new Event()
                    {
                        Subject = obj.topicName,
                        //Organizer=obj.trainerName,

                        Attendees = new List<Attendee>()
                        {
                            new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = obj.participants,
                                    // Name = "Adele Vance"
                                },
                                Type = AttendeeType.Required
                            }
                        },
                        Start = new DateTimeTimeZone
                        {
                            DateTime = obj.startdate,
                            TimeZone = "Asia/Kolkata"
                        },
                        End = new DateTimeTimeZone
                        {
                            DateTime = obj.enddate,
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

        public async Task<Event> CreateOnlineMeetingUsingForms(MeetingCreation obj)
        {                      
                try
                {
                    var @event = new Event()
                    {
                        Subject = obj.topicName,
                        //Organizer=obj.trainerName,

                        Attendees = new List<Attendee>()
                        {
                            new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = obj.participants,
                                   // Name = "Adele Vance"
                                },
                                Type = AttendeeType.Required
                            }
                        },
                        Start = new DateTimeTimeZone
                        {
                            DateTime = obj.startdate,
                            TimeZone = "Asia/Kolkata"
                        },
                        End = new DateTimeTimeZone
                        {
                            DateTime = obj.enddate,
                            TimeZone = "Asia/Kolkata"
                        }

                    };
                    await graphBetaClient.Users["6702afb6-109b-4c32-a141-6e65469502b9"].Events
                   .Request()
                   .Header("Prefer", "outlook.timezone=\"Asia/Kolkata\"")
                   .AddAsync(@event);
                }
                catch (Exception ex)
                {
                }

            return null;
           // return Status.Active;

        }
        //public async Task<Event> ListOfEvets([FromRoute] string userid)
        //{
        //    try
        //    {
        //        var events = await graphBetaClient.Users["6702afb6-109b-4c32-a141-6e65469502b9"].Events
        //       .Request()
        //        .Header("Prefer", "outlook.timezone=\"Pacific Standard Time\"")
        //        .Select("subject,body,bodyPreview,organizer,attendees,start,end,location")
        //        .GetAsync();                
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    return null;
        //    // return Status.Active;

        //}

        

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
                        id=tag.Id,
                        topicName = tag.Subject,
                        Start = tag.Start,
                        End = tag.End,
                        Attendees=tag.Attendees.ToList(),

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
        /// <summary>
        /// Updates the tag details.
        /// </summary>
        /// <param name="teamTag">Updated details of the tag.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task UpdateEventAsync(MeetingCreationUpdate meetingCreationUpdate, string teamId)
        {
            try
            {
            
            var @event = new Event()
            {
                Id = meetingCreationUpdate.id,
                Subject = meetingCreationUpdate.topicName,
                //Start = meetingCreationUpdate.start,
                //End= meetingCreationUpdate.end,
                // Attendees= meetingCreationUpdate.Attendees.ToList(),
            };
            //await graphBetaClient.Users[meetingCreationUpdate.id].Events
            //      .Request()
            //      .UpdateAsync(@event);
            }
            catch(Exception Ex)
            {

            }

            //var teamworkTagUpdated = await this.graphBetaClient.Teams[teamId].Tags[teamTag.Id].Request().UpdateAsync(teamworkTag);

        }
    }
}
