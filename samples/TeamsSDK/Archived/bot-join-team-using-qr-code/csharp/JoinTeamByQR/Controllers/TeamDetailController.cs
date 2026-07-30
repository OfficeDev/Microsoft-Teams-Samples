using JoinTeamByQR.helper;
using JoinTeamByQR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoinTeamByQR.Controllers
{
    /// <summary>
    /// Class for sharepoint file upload
    /// </summary>
    [Route("getTeams")]
    public class TeamDetailController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, string> _Token;
        public readonly IConfiguration _configuration;
        public TeamDetailController(
            ConcurrentDictionary<string, string> token,IConfiguration configuration)
        {
            _Token = token;
            _configuration = configuration;
        }

        /// <summary>
        /// This endpoint is called lto get list of all teams of the organization.
        /// </summary>
        [HttpGet]
        public async Task<List<TeamData>> GetAllTeamsAsync()
        {
            var token = string.Empty;

            _Token.TryGetValue("Token", out token);

            var teamData = await JoinTeamHelper.GetAllTeams(token);

            return teamData;
        }
    }
}
