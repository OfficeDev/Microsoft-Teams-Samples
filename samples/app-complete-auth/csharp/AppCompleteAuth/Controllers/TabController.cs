
using AppCompleteAuth.helper;
using AppCompleteAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppCompleteAuth.Controllers
{
    public class TabController : Controller
    {
        public readonly IConfiguration _configuration;
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
        [HttpGet]
        [Route("GetUserAccessToken")]
        public async Task<ActionResult<UserData>> GetUserAccessToken()
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

                    var photo = await client.GetPhotoAsync();

                    var userInfo = new UserData()
                    {
                        User = me,
                        Photo = photo,
                        Title = title
                    };

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

        // Get user details.
        [HttpPost]
        [Route("GetUserDetails")]
        public async Task<JsonResult> GetUserProfile(string accessToken)
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

                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        } 

        // Validate user credentials. 
        [HttpPost]
        [Route("tabCredentialsAuth")]
        public JsonResult AuthenticateUsingCredentials(string userName, string password)
        {
            if(userName == Constant.UserName && password == Constant.Password)
            {
                return Json("Authentication Sucessful");
            }
            else
            {
                return Json("Invalid username or password");
            }
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