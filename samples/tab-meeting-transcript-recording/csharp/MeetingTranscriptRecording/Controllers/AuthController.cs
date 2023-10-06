// <copyright file="AuthController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
using MeetingTranscriptRecording.Helper;
using MeetingTranscriptRecording.Models;
using Microsoft.AspNetCore.Mvc;

namespace MeetingTranscriptRecording.Controllers
{
    public class AuthController : Controller
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        public readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get user access token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetLoginUserInformation")]
        public async Task<ActionResult<UserData>> GetLoginUserInformation()
        {
            try
            {
                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                if (accessToken != null)
                {
                    var client = new SimpleGraphClient(accessToken);
                    var me = await client.GetMeAsync();
                    var title = !string.IsNullOrEmpty(me.JobTitle) ?
                                me.JobTitle : "Unknown";

                    var userInfo = new UserData() { User = me };

                    return userInfo;
                }

                else return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}