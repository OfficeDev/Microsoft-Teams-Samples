using System.Net.Http.Headers;

namespace Microsoft.Teams.Samples.AccountLinking.GitHub;

/// <summary>
/// An example http client for the GitHub API service. 
/// <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0"/>
/// </summary>
public sealed class GitHubServiceClient
{
    private readonly ILogger<GitHubServiceClient> _logger;

    private readonly HttpClient _httpClient;

    public GitHubServiceClient(
        ILogger<GitHubServiceClient> logger,
        HttpClient httpClient
    )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ICollection<GitHubRepository>> GetRepositoriesAsync(string userAccessToken)
    {
        var user = await GetCurrentUserAsync(userAccessToken);
        _logger.LogInformation("Getting repositories for user: {user}", user.Login);
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"https://api.github.com/users/{user.Login}/repos"),
            Method = HttpMethod.Get
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("Result: {code}", response.StatusCode);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<ICollection<GitHubRepository>>();
        return content ?? throw new HttpRequestException("Failed to parse repositories");
    }

    public async Task<GitHubUser> GetCurrentUserAsync(string userAccessToken)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri("https://api.github.com/user"),
            Method = HttpMethod.Get
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("Result: {code}", response.StatusCode);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GitHubUser>();
        return content ?? throw new HttpRequestException("Failed to parse user");
    }
}



