using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.BotBuilderSamples.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IConfiguration config, IHttpClientFactory clientFactory)
        {
            _config = config;
            _clientFactory = clientFactory;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromServices] TokenStore tokenStore)
        {
            var client = _clientFactory.CreateClient();
            var tokenEndpoint = $"https://{_config["Auth0:Domain"]}/oauth/token";

            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                {"grant_type", "authorization_code"},
                                {"client_id", _config["Auth0:ClientId"]},
                                {"client_secret", _config["Auth0:ClientSecret"]},
                                {"code", code},
                                {"redirect_uri", _config["ApplicationUrl"] + "/api/auth/callback"}
                            }));

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Token exchange failed");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<Auth0TokenResponse>(content);

            tokenStore.SaveToken(state, tokenResult.access_token);

            return Redirect($"/auth-end.html?token={HttpUtility.UrlEncode(content)}");
        }


        public class TokenStore
        {
            private readonly Dictionary<string, string> _tokens = new();

            public void SaveToken(string userId, string token)
            {
                _tokens[userId] = token;
            }

            public bool TryGetToken(string userId, out string token)
            {
                return _tokens.TryGetValue(userId, out token);
            }

            public void RemoveToken(string userId)
            {
                _tokens.Remove(userId);
            }
        }

        public class Auth0TokenResponse
        {
            public string access_token { get; set; }
            public string id_token { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
        }
    }
}
