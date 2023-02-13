// <copyright file="AuthController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
using AnonymousUsers.Helper;
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
        public readonly IConfiguration _configuration;

        public AuthController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Get facebook profile of user.
        [HttpPost]
        [Route("getFbAccessToken")]
        public async Task<JsonResult> GetFacebookAuthToken(string accessToken)
        {
            var fbAppId = _configuration["FacebookAppId"];
            var fbPassword = _configuration["FacebookAppPassword"];
            var redirectUrl = _configuration["ApplicationBaseUrl"] + "/facebook-auth-end";

            var client = new HttpClient();
            string responseBody;
            var response = await client.GetAsync(string.Format("https://graph.facebook.com/v12.0/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}", fbAppId, redirectUrl, fbPassword,accessToken));
            
            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<dynamic>(responseBody).access_token;

                var profile = FacebookHelper.GetUri("https://graph.facebook.com/me",
                Tuple.Create("fields", "name,picture"),
                Tuple.Create("access_token", token.Value));

                var res = await FacebookHelper.FacebookRequest<FacebookProfile>(profile);
                var result = new
                {
                    name = res.Name,
                    picture = res.ProfilePicture.data.url
                };

                var jsonString = JsonConvert.SerializeObject(result);

                return Json(jsonString);
            }
            else
            {
                return Json("Error occoured");
            }
        }
    }
}