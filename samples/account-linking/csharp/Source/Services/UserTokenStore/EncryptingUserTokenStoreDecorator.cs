using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

/// <summary>
/// The EncryptingUserTokenStoreDecorator is a <see cref="IUserTokenStore" /> decorator that is responsible for transparently 
/// encrypting and decrypting user tokens.
/// </summary>
/// <remarks>
/// This class uses the ASP.NET Data Protection APIs to encrypt and decrypt tokens
/// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/using-data-protection">
/// </remarks>
public sealed class EncryptingUserTokenStoreDecorator : IUserTokenStore
{
    private readonly IUserTokenStore _decorated;

    private readonly ILogger<EncryptingUserTokenStoreDecorator> _logger;

    private readonly IPersistedDataProtector _baseProtector;

    public EncryptingUserTokenStoreDecorator(
        IUserTokenStore decorated,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<EncryptingUserTokenStoreDecorator> logger)
    {
        _decorated = decorated;
        _logger = logger;

        _baseProtector = dataProtectionProvider.CreateProtector(nameof(EncryptingUserTokenStoreDecorator)) as IPersistedDataProtector ?? 
            throw new Exception("Protection provider doesn't support persisted data protector");
    }

    public Task DeleteTokenAsync(string tenantId, string userId)
    {
        return _decorated.DeleteTokenAsync(tenantId: tenantId, userId: userId);
    }

    public async Task<string?> GetTokenAsync(string tenantId, string userId)
    {
        var protectionProvider = GetProtectionProvider(tenantId: tenantId, userId: userId);

        var protectedTokenString = await _decorated.GetTokenAsync(tenantId: tenantId, userId: userId);
        if (protectedTokenString == default)
        {
            _logger.LogInformation("Decorated store did not have token, returning null");
            return default;
        }

        string? decryptedToken = default;
        try
        {
            var protectedTokenBytes = Convert.FromBase64String(protectedTokenString);
            // We need to use the DangerousUnprotect because the keys may have rotated since we last saved the token.
            // We can ignore 'old' keys in this instance we aren't as concerned with the integrity of the data as 
            // the attack being mitigated is the loss of the ciphertext, not the key.
            var decryptedTokenBytes = protectionProvider.DangerousUnprotect(
                    protectedData: protectedTokenBytes,
                    ignoreRevocationErrors: true,
                    out bool requiresMigration,
                    out bool wasRevoked);
            decryptedToken = Encoding.UTF8.GetString(decryptedTokenBytes);
            if (requiresMigration || wasRevoked)
            {
                _logger.LogWarning("Token needs to be migrated to a new key, writing back pre-emptively: {requiresMigration} | {wasRevoked}", requiresMigration, wasRevoked);
                // pre-emptively write back the token with the most up-to-date certificate.
                await SetTokenAsync(tenantId: tenantId, userId: userId, token: decryptedToken);
            }
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning("Failed to decrypt token: {reason}", ex.Message);
        }
        return decryptedToken;
    }

    public async Task SetTokenAsync(string tenantId, string userId, string token)
    {
        var protectionProvider = GetProtectionProvider(tenantId: tenantId, userId: userId);
        // We do this to ensure that the DangerousUnprotect uses the same encoding as it only has the byte[] signature.
        var tokenBytes = Encoding.UTF8.GetBytes(token); 
        var protectedToken = protectionProvider.Protect(tokenBytes);
        // send the wrapped token on its way to the decorated provider
        await _decorated.SetTokenAsync(tenantId: tenantId, userId: userId, token: Convert.ToBase64String(protectedToken));
    }

    private IPersistedDataProtector GetProtectionProvider(string tenantId, string userId)
    {
        // Get a salted key derived from the base key with the tenantId & userId.
        return _baseProtector.CreateProtector(tenantId, userId) as IPersistedDataProtector ??
            throw new Exception("Protection provider doesn't support persisted data protector");
    }
}