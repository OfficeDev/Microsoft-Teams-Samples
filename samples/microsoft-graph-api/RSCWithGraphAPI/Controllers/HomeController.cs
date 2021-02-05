using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Graph;
using System.Net.Http.Headers;

namespace RSCWithGraphAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
       
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configure Tab
        /// </summary>
        /// <returns></returns>
        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        /// <summary>
        /// RSC Setup
        /// </summary>
        /// <returns></returns>
        [Route("RSCSetup")]
        public ActionResult Setup()
        {
            
            return View();
        }

        /// <summary>
        /// Get Channels List
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetChannelsList")]        
        public async Task<List<string>> GetChannelsList(string tenantId, string groupId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var result= await graphClient.Teams[groupId].Channels.Request()
                .GetAsync();

            List<string> channelsList = new List<string>();
            foreach(var channel in result)
            {
                channelsList.Add(channel.DisplayName.ToString());
            }

            return channelsList;
        }

        /// <summary>
        /// Get Token
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetToken(string tenantId)
        {

            var url = $"https://login.microsoftonline.com/" + tenantId + "/oauth2/v2.0/token";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");

            var values = new Dictionary<string, string>
                {
                    { "client_id",_configuration["ClientId"].ToString()},
                    { "client_secret",_configuration["ClientSecret"].ToString()},
                    { "grant_type", "client_credentials"},
                    { "scope", "https://graph.microsoft.com/.default"},
                };

            var formContent = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(url, formContent);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseString);

            if (response.StatusCode == HttpStatusCode.OK)
            {
               return (string)responseJsonObject.access_token;   
            }
            else
            {
                return (string)responseJsonObject.error;
            }
        }

        /// <summary>
        ///Get Authenticated Client
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private GraphServiceClient GetAuthenticatedClient(string token)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));
            return graphClient;
        }
    }
}
