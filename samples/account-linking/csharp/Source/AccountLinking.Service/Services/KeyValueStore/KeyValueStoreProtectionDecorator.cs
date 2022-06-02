
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage;

public sealed class KeyValueStoreProtectionDecorator: IKeyValueStore
{
    private readonly IKeyValueStore _decorated;
    private readonly ILogger<KeyValueStoreProtectionDecorator> _logger;

    private readonly IPersistedDataProtector _baseProtector;
    
    public KeyValueStoreProtectionDecorator(
        IKeyValueStore decorated,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<KeyValueStoreProtectionDecorator> logger)
    {
        _decorated = decorated;
        _logger = logger;
        _baseProtector = dataProtectionProvider.CreateProtector(nameof(KeyValueStoreProtectionDecorator)) as IPersistedDataProtector ?? 
            throw new Exception("Protection provider doesn't support persisted data protector");
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        return _decorated.DeleteAsync(key, cancellationToken);
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var protectionProvider = GetProtectionProvider(key);

        var cipherText = await _decorated.GetAsync(key, cancellationToken);
        if (cipherText == default)
        {
            return default;
        }

        byte[]? plainText = default;
        try
        {
            // We need to use the DangerousUnprotect because the keys may have rotated since we last saved the token.
            // We can ignore 'old' keys in this instance we aren't as concerned with the integrity of the data as 
            // the attack being mitigated is the loss of the ciphertext, not the key.
            plainText = protectionProvider.DangerousUnprotect(
                    protectedData: cipherText,
                    ignoreRevocationErrors: true,
                    out bool requiresMigration,
                    out bool wasRevoked);
            if (requiresMigration || wasRevoked)
            {
                _logger.LogWarning("Token needs to be migrated to a new key, writing back pre-emptively: {requiresMigration} | {wasRevoked}", requiresMigration, wasRevoked);
                // pre-emptively write back the token with the most up-to-date certificate.
                await SetAsync(key, plainText, cancellationToken);
            }
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning("Failed to decrypt token: {reason}", ex.Message);
        }
        return plainText;
    }

    public Task SetAsync(string key, byte[] value, CancellationToken cancellationToken = default)
    {
        var protectionProvider = GetProtectionProvider(key);
        var cipherText = protectionProvider.Protect(value);
        return _decorated.SetAsync(key, cipherText, cancellationToken);
    }

    private IPersistedDataProtector GetProtectionProvider(string key)
    {
        // Get a salted key derived from the base key with the tenantId & userId.
        return _baseProtector.CreateProtector(key) as IPersistedDataProtector ??
            throw new Exception("Protection provider doesn't support persisted data protector");
    }
}