using System.Text.Json.Serialization;

namespace Microsoft.Teams.Samples.AccountLinking.GitHub;

/// <summary>
/// A record representing a subset of the Repository entity from the GitHub API. 
/// <see href="https://docs.github.com/en/rest/reference/repos">
/// </summary>
public record GitHubRepository(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("html_url")] string Url,
    [property: JsonPropertyName("watchers_count")] long Watchers,
    [property: JsonPropertyName("stargazers_count")] long Stars,
    [property: JsonPropertyName("forks_count")] long Forks
);