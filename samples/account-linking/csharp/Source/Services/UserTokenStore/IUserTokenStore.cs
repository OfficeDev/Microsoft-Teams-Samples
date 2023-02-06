namespace Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

/// <summary>
/// An IUserTokenStore is responsible for storing a token securely.
/// </summary>
public interface IUserTokenStore
{
    /// <summary>
    /// Persist the user's token.
    /// </summary>
    /// <param name="tenantId">The user's tenant id.</param>
    /// <param name="userId"The user's id.></param>
    /// <param name="token"The user's token.></param>
    /// <returns>A Task that resolves once the operation is complete.</returns>
    Task SetTokenAsync(string tenantId, string userId, string token);

    /// <summary>
    /// Expunge the user's token from storage.
    /// </summary>
    /// <param name="tenantId">The user's tenant id.</param>
    /// <param name="userId"The user's id.></param>
    /// <returns>A Task that resolves once the operation is complete.</returns>
    Task DeleteTokenAsync(string tenantId, string userId);

    /// <summary>
    /// Retrieve the user's token from storage.
    /// </summary>
    /// <param name="tenantId">The user's tenant id.</param>
    /// <param name="userId"The user's id.></param>
    /// <returns>A Task that resolves to the user's opaque token string or null if the token isn't in storage.</returns>
    Task<string?> GetTokenAsync(string tenantId, string userId);
}