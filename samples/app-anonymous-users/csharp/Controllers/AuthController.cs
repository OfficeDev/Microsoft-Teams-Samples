// <copyright file="AuthController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
using AnonymousUsers.Helper;
using AnonymousUsers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnonymousUsers.Controllers
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
        /// Get facebook profile of user.
        /// </summary>
        /// <param name="accessToken">Token</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getFacebookLoginUserInfo")]
        public async Task<JsonResult> getFacebookLoginUserInfo([FromBody]string accessToken)
        {
            var fbAppId = _configuration["FacebookAppId"];
            var fbPassword = _configuration["FacebookAppPassword"];
            var redirectUrl = _configuration["ApplicationBaseUrl"] + "/facebook-auth-end";

            var client = new HttpClient();
            string responseBody;
            var response = await client.GetAsync(string.Format("https://graph.facebook.com/v12.0/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}", fbAppId, redirectUrl, fbPassword, accessToken));

            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<dynamic>(responseBody).access_token;

                var profile = FacebookHelper.GetUri("https://graph.facebook.com/me",
                Tuple.Create("fields", "name,picture"),
                Tuple.Create("access_token", token.Value));

                var getFacebookUserDetails = await FacebookHelper.FacebookRequest<FacebookProfile>(profile);
                var result = new
                {
                    name = getFacebookUserDetails.Name,
                    picture = getFacebookUserDetails.ProfilePicture.data.url
                };

                var returnJsonData = JsonConvert.SerializeObject(result);

                return Json(returnJsonData);
            }
            else
            {
                return Json("Error occoured");
            }
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