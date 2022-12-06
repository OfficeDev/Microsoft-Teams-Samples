// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Helper
{
    using GraphTeamsTag.Models;
    using GraphTeamsTag.Provider;
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

        /// <summary>
        /// Create team tag.
        /// </summary>
        /// <param name="teamTag">Details of the tag to be created.</param>
        /// <param name="teamId">Id of team.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task CreateTeamworkTagAsync(TeamTagUpdateDto teamTag, string teamId)
        {
            var teamworkTag = new TeamworkTag()
            {
                DisplayName = teamTag.DisplayName,
                Description = teamTag.Description,
            };

            var teamworkTagMembersCollection = new TeamworkTagMembersCollectionPage();
            teamTag.MembersToBeAdded.ToList().ForEach(member => teamworkTagMembersCollection.Add(new TeamworkTagMember
            {
                UserId = member.UserId
            }));

            teamworkTag.Members = teamworkTagMembersCollection;
            await this.graphBetaClient.Teams[teamId].Tags.Request().AddAsync(teamworkTag);
        }

        /// <summary>
        /// List all the tags for the specified team.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <returns>List of tags in specified team.</returns>
        public async Task<IEnumerable<TeamTag>> ListTeamworkTagsAsync(string teamId)
        {
            try
            {
                var tags = await this.graphBetaClient.Teams[teamId].Tags.Request().GetAsync();
                var teamworkTagList = new List<TeamTag>();
                do
                {
                    IEnumerable<TeamworkTag> teamTagCurrentPage = tags.CurrentPage;

                    foreach (var tag in teamTagCurrentPage)
                    {
                        var teamworkTagMembersList = new List<TeamworkTagMember>();

                        teamworkTagList.Add(new TeamTag
                        {
                            Id = tag.Id,
                            DisplayName = tag.DisplayName,
                            Description = tag.Description,
                            MembersCount = tag.MemberCount == null ? 0 : (int)tag.MemberCount,
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
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the tag details.
        /// </summary>
        /// <param name="teamTagId">Id of tag.</param>
        /// <param name="teamId">Id of team.</param>
        /// <returns>Team tag details.</returns>
        public async Task<TeamTag> GetTeamworkTagsAsync(string teamTagId, string teamId)
        {
            try
            {
                var teamworkTag = await this.graphBetaClient.Teams[teamId].Tags[teamTagId]
               .Request()
               .GetAsync();

                var teamTag = new TeamTag
                {
                    Id = teamworkTag.Id,
                    DisplayName = teamworkTag.DisplayName,
                    Description = teamworkTag.Description
                };

                return teamTag;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Updates the tag details.
        /// </summary>
        /// <param name="teamTag">Updated details of the tag.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task UpdateTeamworkTagAsync(TeamTagUpdateDto teamTag, string teamId)
        {
            var teamworkTag = new TeamworkTag()
            {
                Id = teamTag.Id,
                DisplayName = teamTag.DisplayName,
                Description = teamTag.Description,
            };

            var teamworkTagUpdated = await this.graphBetaClient.Teams[teamId].Tags[teamTag.Id].Request().UpdateAsync(teamworkTag);
            if (teamworkTagUpdated != null)
            {
                await AddRemoveTagMembersAsync(teamTag, teamId);
            }
        }

        /// <summary>
        /// Add or removes the members of tag.
        /// </summary>
        /// <param name="teamTag">Updated detials of the tag.</param>
        /// <param name="teamId">Id of the team</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task AddRemoveTagMembersAsync (TeamTagUpdateDto teamTag, string teamId)
        {
            foreach (var member in teamTag.MembersToBeAdded)
            {
                try
                {
                    var teamworkTagMember = new TeamworkTagMember
                    {
                        UserId = member.UserId
                    };

                    await graphBetaClient.Teams[teamId].Tags[teamTag.Id].Members
                        .Request()
                        .AddAsync(teamworkTagMember);
                }
                    catch (Exception ex)
                {
                    Console.WriteLine("Member not added with user Id: " + member.UserId, ex);
                    continue;
                }
            }

            foreach (var member in teamTag.MembersToBeDeleted)
            {
                try
                {
                    await this.graphBetaClient.Teams[teamId].Tags[teamTag.Id].Members[member.Id]
                    .Request()
                    .DeleteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Member not deleted with user Id: " + member.UserId, ex);
                    continue;
                }
            }
        }

        /// <summary>
        /// Get list of tag's member of the specified tag.
        /// </summary>
        /// <param name="teamId">Id of the team.</param>
        /// <param name="tagId">Id of the tag.</param>
        /// <returns>List of member in the tags.</returns>
        public async Task<List<TeamworkTagMember>> GetTeamworkTagMembersAsync(string teamId, string tagId)
        {
            var members = await graphBetaClient.Teams[teamId].Tags[tagId].Members
                .Request()
                .GetAsync();

            var tagMemberList = new List<TeamworkTagMember>();

            do
            {
                tagMemberList.AddRange(members.CurrentPage);
                if (members.NextPageRequest != null)
                {
                    members = await members.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }
            while (members.CurrentPage != null);

            return tagMemberList;
        }

        /// <summary>
        /// Deletes existing tag.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="tagId">Id of tag to be deleted.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DeleteTeamworkTagAsync(string teamId, string tagId)
        {
            await graphBetaClient.Teams[teamId].Tags[tagId]
                .Request()
                .DeleteAsync();
        }
    }
}
