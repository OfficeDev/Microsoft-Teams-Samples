using System.Text.Json.Serialization;

namespace Microsoft.Teams.Samples.AccountLinking.GitHub;

/// <summary>
/// A record representing a subset of the User entity from the GitHub API. 
/// <see href="https://docs.github.com/en/rest/reference/users">
/// </summary>
public record GitHubUser(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("avatar_url")] string AvatarUrl
);