// <copyright file="TeamTagController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Controllers
{
    using GraphTeamsTag.Helper;
    using GraphTeamsTag.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Graph;

    [Route("api/teamtag")]
    [ApiController]
    public class TeamTagController : Controller
    {
        /// <summary>
        /// Gets app details.
        /// </summary>
        private readonly ILogger<TeamTagController> _logger;

        /// <summary>
        /// Graph helper class using graph api's.
        /// </summary>
        private readonly GraphHelper graphHelper;

        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// HttpClientFactory dependency for the app.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Used to access the http context from the request.
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// client Id for the application.
        /// </summary>
        private static readonly string ClientIdConfigurationSettingsKey = "AzureAd:ClientId";

        public TeamTagController(ILogger<TeamTagController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, GraphHelper graphHelper)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            this.graphHelper = graphHelper;
        }

        /// <summary>
        /// Gets app details.
        /// </summary>
        /// <returns>If success return 200 status code, otherwise 500 status code</returns>
        [HttpGet("getAppData")]
        public string GetAppData()
        {
            try
            {
                var clientId = _configuration[ClientIdConfigurationSettingsKey];
                return clientId;
            }
            catch (Exception ex)
            {
                return "Error while fetching app id";
            }
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
        /// <param name="ssoToken">Token to be exchanged.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="teamTagId">Id of tag.</param>
        /// <returns>If success return 200 status code, otherwise 500 status code</returns>
        [HttpGet("tag")]
        public async Task<IActionResult> GetTeamTagAsync([FromQuery] string ssoToken, string teamId, string teamTagId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);
                var teamworkTag = await graphClient.Teams[teamId].Tags[teamTagId]
                .Request()
                .GetAsync();

                var teamwTagDto = new TeamTag
                {
                    Id = teamworkTag.Id,
                    DisplayName = teamworkTag.DisplayName,
                    Description = teamworkTag.Description
                };

                return this.Ok(teamwTagDto);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// List all the tags for the specified team.
        /// </summary>
        /// <param name="ssoToken">Token to be exchanged.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <returns>If success return list of tags, otherwise 500 status code</returns>
        [HttpGet("list")]
        public async Task<IActionResult> ListTeamTagAsync([FromQuery] string ssoToken, string teamId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);

                var tags = await graphClient.Teams[teamId].Tags.Request().GetAsync();
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

                return this.Ok(teamworkTagList);
            }
            catch (Exception e)
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
        /// <param name="ssoToken">Token to be exchanged.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <param name="tagId">Id of the tag.</param>
        /// <returns>If success return 200 status code, otherwise 500 status code</returns>
        [HttpGet("tag/members")]
        public async Task<IActionResult> GetTeamworkTagMembersAsync([FromQuery] string ssoToken, string teamId, string tagId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);
                var members = await graphClient.Teams[teamId].Tags[tagId].Members
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

                return this.Ok(tagMemberList);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// Deletes existing tag.
        /// </summary>
        /// <param name="ssoToken">Token to be exchanged.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="tagId">Id of tag to be deleted.</param>
        /// <returns></returns>
        [HttpDelete("tag")]
        public async Task<IActionResult> DeleteTeamTagAsync([FromQuery] string ssoToken, string teamId, string tagId)
        {
            try
            {
                var token = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, ssoToken);
                var graphClient = SimpleGraphClient.GetGraphClient(token);
                await graphClient.Teams[teamId].Tags[tagId]
                .Request()
                .DeleteAsync();
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(500);
            }
        }
    }
}