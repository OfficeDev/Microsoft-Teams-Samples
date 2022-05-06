using System.Collections.Concurrent;

namespace Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

/// <summary>
/// The InMemoryUserTokenSTore is a naive implementation of the <see cref="IUserTokenStore"> that keeps the tokens in an
/// in-memory concurrent dictionary.
/// 
/// This should not be used in production, but be treated as a minimal example of the semantics for user token storage.
/// </summary>
public sealed class InMemoryUserTokenStore : IUserTokenStore
{
    private readonly ConcurrentDictionary<string, string> _tokens = new();

    private readonly ILogger<InMemoryUserTokenStore> _logger;

    public InMemoryUserTokenStore(
        ILogger<InMemoryUserTokenStore> logger)
    {
        _logger = logger;
    }

    public Task DeleteTokenAsync(string tenantId, string userId)
    {
        _logger.LogInformation("Delete token for ({tenantId}, {userId})", tenantId, userId);
        var key = GenKey(tenantId, userId);
        _tokens.Remove(key, out string _);

        return Task.CompletedTask;
    }

    public Task<string?> GetTokenAsync(string tenantId, string userId)
    {
        var key = GenKey(tenantId, userId);

        var token = _tokens.ContainsKey(key) ? _tokens[key] : null;
        
        _logger.LogInformation("Get token for ({tenantId}, {userId}): ({token})", tenantId, userId, token);
        return Task.FromResult<string?>(token);
    }

    public Task SetTokenAsync(string tenantId, string userId, string token)
    {
        _logger.LogInformation("Set token for ({tenantId}, {userId}): ({token})", tenantId, userId, token);
        var key = GenKey(tenantId, userId);
        _tokens[key] = token;
        return Task.CompletedTask;
    }

    private static string GenKey(string tenantId, string userId)
    {
        return $"{tenantId}_{userId}";
    }
}
