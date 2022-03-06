
using IdentityLinkingWithSSO.helper;
using IdentityLinkingWithSSO.Models;
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
        public static List<UserMapData> UserMapData = new List<UserMapData>();
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
        [Route("GetUserAccessToken")]
        public async Task<JsonResult> GetUserAccessToken(string accessToken, string tid, string userName)
        {
            try
            {
                var bearerToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, accessToken);

                var client = new SimpleGraphClient(bearerToken);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                var photo = await client.GetPhotoAsync();

                var userInfo = new UserData() {
                    User = me,
                    Photo = photo,
                    Title = title
                };

                if (UserMapData.Count < 1)
                {
                    UserMapData.Add(new UserMapData { AadId = userName, isAadSignedIn = true });
                }

                else
                {
                    var data = UserMapData.Find(e => e.AadId == userName);
                    if (data == null)
                    {
                        UserMapData.Add(new UserMapData { AadId = userName, isAadSignedIn = true });
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

        // Get user details.
        [HttpPost]
        [Route("GetUserDetails")]
        public async Task<JsonResult> GetUserProfile(string accessToken, string userName)
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

                if (UserMapData.Count < 1)
                {
                    UserMapData.Add(new UserMapData { AadId = userName, isAadSignedIn = true });
                }

                else
                {
                    var data = UserMapData.Find(e => e.AadId == userName);
                    if (data == null)
                    {
                        UserMapData.Add(new UserMapData { AadId = userName, isAadSignedIn = true });
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
        [Route("getUSerMapData")]
        public JsonResult GetUserMapData(string userName)
        {
            int index = UserMapData.FindIndex(e => e.AadId == userName);

            if (index != -1)
            {
                var userInfo = UserMapData[index];
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
        [Route("getFbAccessToken")]
        public async Task<JsonResult> GetFacebookAuthToken(string accessToken, string userName)
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

                int index = UserMapData.FindIndex(e => e.AadId == userName);
                if (index != -1)
                {
                    var data = UserMapData[index];
                    data.FacebookId = res.Id;
                    data.FacebookToken = token.Value;
                    data.isFacebookSignedIn = true;
                    UserMapData[index] = data;
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
        public async Task<JsonResult> GetFbDetails(string token)
        {
            var profile = FacebookHelper.GetUri("https://graph.facebook.com/me",
            Tuple.Create("fields", "name,picture,id"),
            Tuple.Create("access_token", token));

            var res = await FacebookHelper.FacebookRequest<FacebookProfile>(profile);
            var result = new
            {
                name = res.Name,
                picture = res.ProfilePicture.data.url
            };

            var jsonString = JsonConvert.SerializeObject(result);

            return Json(jsonString);
        }

        // Disconnect facebook profile.
        [HttpPost]
        [Route("disconnectFromFb")]
        public JsonResult DisconnectFb(string userName)
        {
            int index = UserMapData.FindIndex(e => e.AadId == userName);
            if (index != -1)
            {
                var data = UserMapData[index];
                data.FacebookId = "";
                data.FacebookToken = "";
                data.isFacebookSignedIn = false;
                UserMapData[index] = data;
            }

            return Json("disconnected from facebook");
        }

        // Get google profile of user.
        [HttpPost]
        [Route("getGoogleAccessToken")]
        public async Task<JsonResult> GetGoogleAccessToken(string accessToken, string userName)
        {
            var redirectUrl = _configuration["ApplicationBaseUrl"] + "/google-auth-end";
            var googleAppId = _configuration["GoogleAppId"];
            var googleAppPassword = _configuration["GoogleAppPassword"];
            var client = new HttpClient();
            HttpContent content = new StringContent("");
            string responseBody;
            var response = await client.PostAsync(string.Format("https://oauth2.googleapis.com/token?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&grant_type=authorization_code", googleAppId, googleAppPassword, accessToken, redirectUrl), content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                responseBody = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<dynamic>(responseBody).access_token;
                var client2 = new HttpClient();
                    client2.DefaultRequestHeaders.Accept.Clear();
                    client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

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

                    int index = UserMapData.FindIndex(e => e.AadId == userName);
                    if (index != -1)
                    {
                        var data = UserMapData[index];
                        data.GoogleId = emailAddress;
                        data.GoogleToken = token;
                        data.isGoogleSignedIn = true;
                        UserMapData[index] = data;
                    }

                    var jsonString = JsonConvert.SerializeObject(result);

                    return Json(jsonString);

            }catch (Exception ex)
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
        public async Task<JsonResult> GetGoogleDetails(string token)
        {
            var client2 = new HttpClient();
            client2.DefaultRequestHeaders.Accept.Clear();
            client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

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
        public JsonResult DisconnectGoogle(string userName)
        {
            int index = UserMapData.FindIndex(e => e.AadId == userName);
            if (index != -1)
            {
                var data = UserMapData[index];
                data.GoogleId = "";
                data.GoogleToken = "";
                data.isGoogleSignedIn = false;
                UserMapData[index] = data;
            }

            return Json("disconnected from google");
        }
    }
}