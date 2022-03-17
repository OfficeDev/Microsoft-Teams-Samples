
using IdentityLinkingWithSSO.helper;
using IdentityLinkingWithSSO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityLinkingWithSSO.Controllers
{
    public class TabController : Controller
    {
        public readonly IConfiguration _configuration;
        public static List<UserMapping> userMapping = new List<UserMapping>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TabController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

        }

        // Get user access token.
        [HttpPost]
        [Route("getUserDetailsOnBehalfOf")]
        public async Task<JsonResult> GetUserDetails(string idToken, string tenantId, string userPrincipleName)
        {
            try
            {
                var bearerToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, idToken);

                var client = new SimpleGraphClient(bearerToken);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                var photo = await client.GetPhotoAsync();

                var userDetail = new UserData()
                {
                    User = me,
                    Photo = photo,
                    Title = title
                };

                if (userMapping.Count < 1)
                {
                    userMapping.Add(new UserMapping { AadId = userPrincipleName, isAadSignedIn = true });
                }

                else
                {
                    var data = userMapping.Find(e => e.AadId == userPrincipleName);
                    if (data == null)
                    {
                        userMapping.Add(new UserMapping { AadId = userPrincipleName, isAadSignedIn = true });
                    }
                }

                var userDetailJson = JsonConvert.SerializeObject(userDetail);
                return Json(userDetailJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        // Get user details.
        [HttpPost]
        [Route("getUserDetails")]
        public async Task<JsonResult> GetUserProfile(string accessToken, string userPrincipleName)
        {
            try
            {
                var client = new SimpleGraphClient(accessToken);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                var photo = await client.GetPhotoAsync();

                var userInfo = new UserData()
                {
                    User = me,
                    Photo = photo,
                    Title = title
                };

                if (userMapping.Count < 1)
                {
                    userMapping.Add(new UserMapping { AadId = userPrincipleName, isAadSignedIn = true });
                }

                else
                {
                    var data = userMapping.Find(e => e.AadId == userPrincipleName);
                    if (data == null)
                    {
                        userMapping.Add(new UserMapping { AadId = userPrincipleName, isAadSignedIn = true });
                    }
                }

                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        // Get user mapping data. 
        [HttpPost]
        [Route("getUserMapData")]
        public JsonResult GetUserMapData(string userPrincipleName)
        {
            int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);

            if (index != -1)
            {
                var userInfo = new UserMapping()
                {
                    AadId = userMapping[index].AadId,
                    isAadSignedIn = userMapping[index].isAadSignedIn,
                    AadToken = "",
                    FacebookId = userMapping[index].FacebookId,
                    FacebookToken = "",
                    isFacebookSignedIn = userMapping[index].isFacebookSignedIn,
                    GoogleId = userMapping[index].GoogleId,
                    GoogleToken = "",
                    isGoogleSignedIn = userMapping[index].isGoogleSignedIn,
                };
                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }

            else
            {
                return Json("NoDataFound");
            }
        }

        // Get facebook profile of user.
        [HttpPost]
        [Route("getFbUserDetails")]
        public async Task<JsonResult> GetFbUserDetails(string accessToken, string userPrincipleName)
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
                Tuple.Create("fields", "name,picture,id"),
                Tuple.Create("access_token", token.Value));

                var res = await FacebookHelper.FacebookRequest<FacebookProfile>(profile);
                var result = new
                {
                    name = res.Name,
                    picture = res.ProfilePicture.data.url
                };

                int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);
                if (index != -1)
                {
                    var data = userMapping[index];
                    data.FacebookId = res.Id;
                    data.FacebookToken = token.Value;
                    data.isFacebookSignedIn = true;
                    userMapping[index] = data;
                }

                var jsonString = JsonConvert.SerializeObject(result);

                return Json(jsonString);
            }
            else
            {
                return Json("Error occoured");
            }
        }

        // Get facebook profile of exisiting mapped user.
        [HttpPost]
        [Route("getFbDetails")]
        public async Task<JsonResult> GetFbDetails(string userPrincipleName)
        {
            int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);
            var data = userMapping[index];
            var profile = FacebookHelper.GetUri("https://graph.facebook.com/me",
            Tuple.Create("fields", "name,picture,id"),
            Tuple.Create("access_token", data.FacebookToken));

            var res = await FacebookHelper.FacebookRequest<FacebookProfile>(profile);
            var result = new
            {
                name = res.Name,
                picture = res.ProfilePicture.data.url
            };

            var fbDetailsString = JsonConvert.SerializeObject(result);

            return Json(fbDetailsString);
        }

        // Disconnect facebook profile.
        [HttpPost]
        [Route("disconnectFromFb")]
        public JsonResult DisconnectFb(string userPrincipleName)
        {
            int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);
            if (index != -1)
            {
                var data = userMapping[index];
                data.FacebookId = "";
                data.FacebookToken = "";
                data.isFacebookSignedIn = false;
                userMapping[index] = data;
            }

            return Json("disconnected from facebook");
        }

        // Get google profile of user.
        [HttpPost]
        [Route("getGoogleUserDetails")]
        public async Task<JsonResult> GetGoogleUserDetails(string idToken, string userPrincipleName)
        {
            var redirectUrl = _configuration["ApplicationBaseUrl"] + "/google-auth-end";
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
                    var state = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                    List<UserData> items = ((JArray)state).Select(x => new UserData
                    {
                        DisplayName = (string)x["displayName"]
                    }).ToList();

                    var state2 = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                    List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                    {
                        Url = (string)x["url"]
                    }).ToList();

                    var state3 = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                    List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                    {
                        Value = (string)x["value"]
                    }).ToList();

                    var result = new
                    {
                        name = items[0].DisplayName,
                        picture = items2[0].Url,
                        email = items3[0].Value
                    };

                    int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);
                    if (index != -1)
                    {
                        var data = userMapping[index];
                        data.GoogleId = items3[0].Value;
                        data.GoogleToken = accessToken;
                        data.isGoogleSignedIn = true;
                        userMapping[index] = data;
                    }

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

        // Get google profile of existing mapped user.
        [HttpPost]
        [Route("getGoogleDetails")]
        public async Task<JsonResult> GetGoogleDetails(string userPrincipleName)
        {
            var client2 = new HttpClient();
            client2.DefaultRequestHeaders.Accept.Clear();
            int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);
            var data = userMapping[index];
            client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

            var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
            var jboject = JsonConvert.DeserializeObject(json);
            var profile = JObject.FromObject(jboject);
            var state = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
            List<UserData> items = ((JArray)state).Select(x => new UserData
            {
                DisplayName = (string)x["displayName"]
            }).ToList();

            var state2 = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
            List<UserData> items2 = ((JArray)state2).Select(x => new UserData
            {
                Url = (string)x["url"]
            }).ToList();

            var state3 = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
            List<UserData> items3 = ((JArray)state3).Select(x => new UserData
            {
                Value = (string)x["value"]
            }).ToList();

            var displayName = items[0].DisplayName;
            var photoUrl = items2[0].Url;
            var emailAddress = items3[0].Value;

            var result = new
            {
                name = displayName,
                picture = photoUrl,
                email = emailAddress
            };

            var jsonString = JsonConvert.SerializeObject(result);

            return Json(jsonString);
        }

        // Disconnect from google.
        [HttpPost]
        [Route("disconnectFromGoogle")]
        public JsonResult DisconnectGoogle(string userPrincipleName)
        {
            int index = userMapping.FindIndex(e => e.AadId == userPrincipleName);
            if (index != -1)
            {
                var data = userMapping[index];
                data.GoogleId = "";
                data.GoogleToken = "";
                data.isGoogleSignedIn = false;
                userMapping[index] = data;
            }

            return Json("disconnected from google");
        }
    }
}