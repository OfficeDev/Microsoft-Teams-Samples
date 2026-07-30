using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Microsoft.Teams.Samples.AccountLinking.OAuth;

public sealed class OAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OAuthServiceClient> _logger;

    private readonly string _oauthClientId;
    private readonly string _oauthClientSecret;
    private readonly string _oauthAccessTokenUrl;

    public OAuthServiceClient(
        ILogger<OAuthServiceClient> logger,
        IOptions<OAuthOptions> options,
        HttpClient httpClient
    )
    {
        _logger = logger;
        _httpClient = httpClient;

        _oauthClientId = options.Value.ClientId;
        _oauthClientSecret = options.Value.ClientSecret;
        _oauthAccessTokenUrl = options.Value.AccessTokenUrl;
    }

    public async Task<OAuthAccessTokenResponse?> RefreshAccessTokenAsync(string refreshToken)
    {
        var request = CreateRefreshTokenRequest(refreshToken);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Failed to refresh token: [{reason}]", response.ReasonPhrase);
            return null;
        }
        return await GetOAuthAccessTokenResponseAsync(response.Content);
    }

    public async Task<OAuthAccessTokenResponse> ClaimCodeAsync(string code)
    {
        var request = CreateCodeClaimRequest(code);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await GetOAuthAccessTokenResponseAsync(response.Content);
    }

    private HttpRequestMessage CreateRefreshTokenRequest(string refreshToken)
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_oauthAccessTokenUrl),
            Content = new MultipartFormDataContent()
            {
                { new StringContent("refresh_token"), "grant_type" },
                { new StringContent(_oauthClientId), "client_id" },
                { new StringContent(_oauthClientSecret), "client_secret" },
                { new StringContent(refreshToken), "refresh_token"}
            }
        };

        requestMessage.Headers.Add("Accept", "application/json");
        return requestMessage;
    }

    private HttpRequestMessage CreateCodeClaimRequest(string code)
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_oauthAccessTokenUrl),
            Content = new MultipartFormDataContent()
            {
                { new StringContent(_oauthClientId), "client_id" },
                { new StringContent(_oauthClientSecret), "client_secret" },
                { new StringContent(code), "code" }
            },
        };
        requestMessage.Headers.Add("Accept", "application/json");
        return requestMessage;
    }

    private static async Task<OAuthAccessTokenResponse> GetOAuthAccessTokenResponseAsync(
        HttpContent content)
    {
        var stream = await content.ReadAsStreamAsync();
        return 
            (await JsonSerializer.DeserializeAsync<OAuthAccessTokenResponse>(stream)) 
            ?? throw new InvalidDataException("OAuth response invalid");
    }
}
