// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Web;
using TeamsAuth0Bot.Services;

namespace TeamsAuth0Bot.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ConfigOptions _config;
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(ConfigOptions config, IHttpClientFactory clientFactory)
        {
            _config = config;
            _clientFactory = clientFactory;
        }

        // Handles Auth0 OAuth callback, exchanges authorization code for access token, and stores it
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromServices] TokenStore tokenStore)
        {
            var client = _clientFactory.CreateClient();
            var tokenEndpoint = $"https://{_config.Auth0.Domain}/oauth/token";

            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"client_id", _config.Auth0.ClientId},
                {"client_secret", _config.Auth0.ClientSecret},
                {"code", code},
                {"redirect_uri", _config.ApplicationUrl + "/api/auth/callback"}
            }));

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Token exchange failed");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<Auth0TokenResponse>(content);

            tokenStore.SaveToken(state, tokenResult!.access_token);

            return Redirect($"/auth-end.html?token={HttpUtility.UrlEncode(content)}");
        }

        public class Auth0TokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string id_token { get; set; } = string.Empty;
            public string scope { get; set; } = string.Empty;
            public int expires_in { get; set; }
            public string token_type { get; set; } = string.Empty;
        }
    }
}
