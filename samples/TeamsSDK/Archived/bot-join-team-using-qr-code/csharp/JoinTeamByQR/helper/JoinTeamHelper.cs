// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using JoinTeamByQR.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoinTeamByQR.helper
{
    public class JoinTeamHelper
    {
        // Helper method to get list of all teams of organization.
        public static async Task<List<TeamData>> GetAllTeams(string tokenResponse)
        {
            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var client = new SimpleGraphClient(tokenResponse);
            return await client.GetAllTeams();
        }

        // Helper method to add user to the team.
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