// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using JoinTeamByQR.Helpers;
using JoinTeamByQR.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace JoinTeamByQR.Controllers
{
    /// <summary>
    /// Controller for team operations
    /// </summary>
    [Route("api/teams")]
    [ApiController]
    public class TeamDetailController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, string> _tokenCache;

        public TeamDetailController(ConcurrentDictionary<string, string> tokenCache)
        {
            _tokenCache = tokenCache;
        }

        /// <summary>
        /// This endpoint is called to get list of all teams of the organization.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TeamData>>> GetAllTeamsAsync()
        {
            var token = string.Empty;

            // Try to get token from cache
            // First try with a general token key, or implement user-specific token retrieval
            if (_tokenCache.TryGetValue("Token", out token) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    var teamData = await JoinTeamHelper.GetAllTeams(token);
                    return Ok(teamData);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error getting teams: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Authentication token not found. Please sign in first by sending '/signin' to the bot." });
        }
    }
}
