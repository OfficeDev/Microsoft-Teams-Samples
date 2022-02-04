
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

                var client = new SimpleGraphClient(accessToken);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                var photo = await client.GetPhotoAsync();

                var userInfo = new UserData() {
                    User = me,
                    Photo = photo,
                    Title = title
                };

                return userInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        [HttpPost]
        [Route("GetUserDetails")]
        public async Task<JsonResult> GetUserProfile(string accessToken)
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

            return Json(userInfo);
        } 

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
    }
}
