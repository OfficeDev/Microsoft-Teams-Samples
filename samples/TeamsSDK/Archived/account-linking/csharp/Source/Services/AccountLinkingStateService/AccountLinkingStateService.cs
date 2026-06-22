using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using Microsoft.Extensions.Options;

using Microsoft.Teams.Samples.AccountLinking.ReplayValidation;

namespace Microsoft.Teams.Samples.AccountLinking.State;

/// <summary>
/// The OAuthStateService is responsible for encoding the tenant and user ids in a verifiable object with a limited lifespan. 
/// This "state" object is used in the OAuth flow to avoid csrf.
///  https://docs.microsoft.com/en-us/azure/active-directory/develop/reply-url#use-a-state-parameter 
/// </summary>
/// <remarks>
/// This uses the ASP.NET Core Data Protection library to handle the cryptographic verification of the encoded 'state' object.
/// To learn more see the documentation at: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/introduction
/// </remarks> 
public sealed class AccountLinkingStateService
{
    private readonly ILogger<AccountLinkingStateService> _logger;

    private readonly IReplayValidator _replayValidator;

    private readonly ITimeLimitedDataProtector _dataProtector;

    private readonly TimeSpan _lifeSpan;

    public AccountLinkingStateService(
        ILogger<AccountLinkingStateService> logger,
        IReplayValidator replayValidator, 
        IDataProtectionProvider dataProtectionProvider, 
        IOptions<AccountLinkingStateServiceOptions> options)
    {
        _logger = logger;
        _replayValidator = replayValidator;
        var protectorName = string.IsNullOrEmpty(options.Value.ProtectorName)
            ? (typeof(AccountLinkingStateService).Assembly.FullName ?? nameof(AccountLinkingStateService))
            : options.Value.ProtectorName;
        _dataProtector = dataProtectionProvider.CreateProtector(protectorName).ToTimeLimitedDataProtector();
        _lifeSpan = options.Value.ExpirationTime;
    }

    public Task<(AccountLinkingState state, DateTimeOffset expiration)> GetAsync(string accountLinkingState)
    {
        string unprotectedStateString = _dataProtector.Unprotect(accountLinkingState, out DateTimeOffset expiration);

        var stateObject = JsonSerializer.Deserialize<AccountLinkingState>(unprotectedStateString);
        if (stateObject == default)
        {
            _logger.LogWarning("Failed to deserialize the state object");
            throw new Exception("Verification failed, state object invalid");
        }
        return Task.FromResult((stateObject, expiration));
    }

    public Task<string> SetAsync(AccountLinkingState state, DateTimeOffset? expiration = default)
    {
        DateTimeOffset exp = expiration ?? DateTimeOffset.Now + _lifeSpan;
        var stateString = JsonSerializer.Serialize(state);
        var accountLinkingState = _dataProtector.Protect(stateString, exp);
        return Task.FromResult(accountLinkingState);
    }
}
