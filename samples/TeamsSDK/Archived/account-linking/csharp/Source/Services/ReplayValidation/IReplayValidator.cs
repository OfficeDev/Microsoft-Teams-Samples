namespace Microsoft.Teams.Samples.AccountLinking.ReplayValidation;

/// <summary>
/// An IReplayValidator is responsible for atomically 'claiming' an opaque id for the purposes of 'at most once' semantics.
/// 
/// This is useful for detecting replayed tokens or de-duplication of inbound requests.
/// </summary>
/// <remarks>
/// </remarks>
public interface IReplayValidator
{
    /// <summary>
    /// Check if the id has been seen before
    /// </summary>
    /// <param name="id">The opaque id to claim</param>
    /// <param name="expiration">The time when id can safely be presumed to have 'expired' and will no longer be checked against.</param>
    /// <returns>True if the id is new & was claimed. False otherwise.</returns>
    Task<bool> ClaimIdAsync(string id, DateTimeOffset expiration);
}
