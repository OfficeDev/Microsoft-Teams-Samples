// <copyright file="TeamTagController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Controllers
{
    using EventMeeting.Helper;
    using EventMeeting.Models;
    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;

    [Route("api/teamtag")]
    [ApiController]
    public class TeamTagController : ControllerBase
    {
        private readonly ILogger<TeamTagController> _logger;

        private readonly GraphHelper graphHelper;

        public TeamTagController(ILogger<TeamTagController> logger, GraphHelper graphHelper)
        {
            _logger = logger;
            this.graphHelper = graphHelper;
        }

        /// <summary>
        /// Create team tag.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="teamTag">Details of the tag to be created.</param>
        /// <returns>If success return 201 status code, otherwise 500 status code</returns>
        [HttpPost("{teamId}")]
        public async Task<IActionResult> CreateTeamTagAsync([FromRoute] string teamId, [FromBody] TeamTagUpdateDto teamTag)
        {
            try
            {
                await this.graphHelper.CreateTeamworkTagAsync(teamTag, teamId);
                return this.StatusCode(201);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// Gets the tag details.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="teamTagId">Id of tag.</param>
        /// <returns>If success return 200 status code, otherwise 500 status code</returns>
        [HttpGet("{teamId}/tag/{teamTagId}")]
        public async Task<IActionResult> GetTeamTagAsync([FromRoute] string teamId, [FromRoute] string teamTagId)
        {
            try
            {
                var teamworkTag = await this.graphHelper.GetTeamworkTagsAsync(teamTagId, teamId);
                return this.Ok(teamworkTag);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// List all the tags for the specified team.
        /// </summary>
        /// <param name="teamId">Id of the team.</param>
        /// <returns>If success return list of tags, otherwise 500 status code</returns>
        [HttpGet("{teamId}/list")]
        public async Task<IActionResult> ListTeamTagAsync([FromRoute] string teamId)
        {
            try
            {
                var teamworkTagList = (await this.graphHelper.ListTeamworkTagsAsync(teamId)).ToList();
                return this.Ok(teamworkTagList);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// Updates the tag.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="teamTag">Updated details of the tag.</param>
        /// <returns>If success return 204 status code, otherwise 500 status code</returns>
        [HttpPatch("{teamId}/update")]
        public async Task<IActionResult> UpdateTeamTagAsync([FromRoute] string teamId, [FromBody] TeamTagUpdateDto teamTag)
        {
            try
            {
                await this.graphHelper.UpdateTeamworkTagAsync(teamTag, teamId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// Get list of tag's member of the specified tag.
        /// </summary>
        /// <param name="teamId">Id of the team.</param>
        /// <param name="tagId">Id of the tag.</param>
        /// <returns>If success return 200 status code, otherwise 500 status code</returns>
        [HttpGet("{teamId}/tag/{tagId}/members")]
        public async Task<IActionResult> GetTeamworkTagMembersAsync([FromRoute] string teamId, [FromRoute] string tagId)
        {
            try
            {
                var members = await this.graphHelper.GetTeamworkTagMembersAsync(teamId, tagId);
                return this.Ok(members);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// Deletes existing tag.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="tagId">Id of tag to be deleted.</param>
        /// <returns></returns>
        [HttpDelete("{teamId}/tag/{tagId}")]
        public async Task<IActionResult> DeleteTeamTagAsync([FromRoute] string teamId, [FromRoute] string tagId)
        {
            try
            {
                await this.graphHelper.DeleteTeamworkTagAsync(teamId, tagId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }
    }
}