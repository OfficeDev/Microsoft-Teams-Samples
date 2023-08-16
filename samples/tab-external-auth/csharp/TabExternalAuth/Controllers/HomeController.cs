// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TabExternalAuth.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using System.Linq;

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <param name="httpClientFactory">IHttpClientFactory instance.</param>
        /// <param name="httpContextAccessor">IHttpContextAccessor instance.</param>
        public HomeController(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            ViewBag.GoogleAppId = _configuration["GoogleAppId"];
            return View();
        }

        /// <summary>
        /// Retrieve access token and fetches user profile
        /// </summary>
        /// <returns>Returns user profile details</returns>
        [HttpPost]
        [Route("getGoogleAccessToken")]
        public async Task<JsonResult> GetGoogleAccessToken(string idToken)
        {
            var redirectUrl = _configuration["ApplicationBaseUrl"] + "/Auth/GoogleEnd";
            var googleAppId = _configuration["GoogleAppId"];
            var googleAppPassword = _configuration["GoogleAppPassword"];

            var client = new HttpClient();
            HttpContent content = new StringContent("");
            string responseBody;
            var response = await client.PostAsync(string.Format("https://oauth2.googleapis.com/token?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&grant_type=authorization_code", googleAppId, googleAppPassword, idToken, redirectUrl), content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                    var accessToken = JsonConvert.DeserializeObject<dynamic>(responseBody).access_token;
                    var googleClient = new HttpClient();
                    googleClient.DefaultRequestHeaders.Accept.Clear();
                    googleClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                    var json = await googleClient.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                    var jboject = JsonConvert.DeserializeObject(json);
                    var profile = JObject.FromObject(jboject);

                    var nameState = (JArray)profile.ToObject<UserJsonMapper<JArray>>()?.Names;
                    List<UserData> items = ((JArray)nameState).Select(x => new UserData
                    {
                        DisplayName = (string)x["displayName"]
                    }).ToList();

                    var photoState = (JArray)profile.ToObject<UserJsonMapper<JArray>>()?.Photos;
                    List<UserData> items2 = ((JArray)photoState).Select(x => new UserData
                    {
                        Url = (string)x["url"]
                    }).ToList();

                    var emailState = (JArray)profile.ToObject<UserJsonMapper<JArray>>()?.EmailAddresses;
                    List<UserData> items3 = ((JArray)emailState).Select(x => new UserData
                    {
                        Value = (string)x["value"]
                    }).ToList();

                    var result = new
                    {
                        name = items[0].DisplayName,
                        picture = items2[0].Url,
                        email = items3[0].Value
                    };

                    var googleUserDetailString = JsonConvert.SerializeObject(result);

                    return Json(googleUserDetailString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }
            else
            {
                return Json("Error occoured");
            }
        }

        // Handle error route
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}