using System.ComponentModel.DataAnnotations;

namespace Microsoft.Teams.Samples.AccountLinking.State;

/// <summary>
/// The configuration options for the <see cref="AccountLinkingStateService" />
/// </summary>
public sealed class AccountLinkingStateServiceOptions
{

    /// <summary>
    /// Gets or sets how long the state tokens should be valid for.
    /// </summary>
    /// <returns>The timespan of how long tokens are valid.</returns>
    /// <remarks>
    /// In practice this is a balancing act between how long we want to allow users to take to log in and how long we wish to 
    /// store token ids for detecting replays.
    /// </remarks>
    [Range(typeof(TimeSpan), "00:00:30", "10675199.02:48:05.4775807")]
    public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the name of the <see cref="IDataProtector" /> used to encrypt the state values.
    /// By default this will be derived from the namespace / assembly / name of the <see cref="AccountLinkingStateService" />
    /// </summary>
    /// <returns>The name of the data protector used to encrypt the state values.</returns>
    /// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/introduction" />
    public string? ProtectorName { get; set; }
}