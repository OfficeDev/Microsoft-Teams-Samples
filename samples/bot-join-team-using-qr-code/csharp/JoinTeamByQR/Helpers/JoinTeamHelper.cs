// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using JoinTeamByQR.Models;
using Microsoft.Graph;
namespace JoinTeamByQR.Helpers
{
    /// <summary>
    /// Helper class for joining team operations.
    /// </summary>
    public class JoinTeamHelper
    {
        /// <summary>
        /// Helper method to get list of all teams of organization.
        /// </summary>
        public static async Task<List<TeamData>> GetAllTeams(string tokenResponse)
        {
            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            try
            {
                var client = new SimpleGraphClient(tokenResponse);
                var teamData = await client.GetAllTeams();

                return teamData;
            }
            catch (ServiceException)
            {
                throw;
            }
        }

        /// <summary>
        /// Helper method to add user to the team.
        /// </summary>
        public static async Task AddUserToTeam(string tokenResponse, string teamId, string userId)
        {
            if (string.IsNullOrEmpty(tokenResponse))
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            if (string.IsNullOrEmpty(teamId))
            {
                throw new ArgumentNullException(nameof(teamId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var client = new SimpleGraphClient(tokenResponse);
            await client.AddUserToTeam(teamId, userId);
        }
    }
}
