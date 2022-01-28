
using AppCompleteAuth.helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppCompleteAuth.Controllers
{
    [Route("GetUserAccessToken")]
    public class TabController : ControllerBase
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
        public async Task<ActionResult<string>> GetUserAccessToken()
        {
            try
            {
                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                return accessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
