using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Samples.AccountLinking.ReplayValidation;
using Microsoft.Teams.Samples.AccountLinking.State;
using Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

namespace Microsoft.Teams.Samples.AccountLinking.OAuth;

/// <summary>
/// Abstraction over the OAuth2.0 logic / flows to enable token caching, refreshing and fetching.
/// </summary>
public sealed class OAuthTokenProvider
{
    private readonly ILogger<OAuthTokenProvider> _logger;

    private readonly AccountLinkingStateService _oAuthStateService;

    private readonly OAuthServiceClient _oAuthServiceClient;

    private readonly IUserTokenStore _userTokenStore;

    private readonly OAuthOptions _options;
    
    private readonly IReplayValidator _replayValidator;

    public OAuthTokenProvider(
        ILogger<OAuthTokenProvider> logger,
        IOptions<OAuthOptions> options,
        AccountLinkingStateService oAuthStateService,
        OAuthServiceClient oAuthServiceClient,
        IUserTokenStore userTokenStore, 
        IReplayValidator replayValidator)
    {
        _logger = logger;
        _options = options.Value;
        _oAuthStateService = oAuthStateService;
        _oAuthServiceClient = oAuthServiceClient;
        _userTokenStore = userTokenStore;
        _replayValidator = replayValidator;
    }

    public async Task<AccessTokenResultBase> GetAccessTokenAsync(string tenantId, string userId)
    {
        var tokenDtoString = await _userTokenStore.GetTokenAsync(tenantId, userId);
        if (tokenDtoString == default)
        {
            _logger.LogInformation("Underlying store contained no token, returning null");
            return GetNeedsConsentResult();
        }

        var tokenDto = JsonSerializer.Deserialize<OAuthUserTokenDto>(tokenDtoString);
        if (tokenDto == default)
        {
            _logger.LogWarning("Token stored was valid json, but not valid DTO! Did the schema change?");
            return GetNeedsConsentResult();
        }

        _logger.LogInformation(
            "Token expired? [{isExpired}]: [{timeDelta}]",
            DateTime.Now >= tokenDto.AccessTokenExpiration,
            DateTime.Now - tokenDto.AccessTokenExpiration);

        // If the 'cached' token is still valid don't do the refresh.
        if (tokenDto.AccessTokenExpiration >= DateTime.Now)
        {
            return new AccessTokenResult
            {
                AccessToken = tokenDto.AccessToken,
            };
        }

        _logger.LogInformation("Performing oAuth refresh flow");
        var jsonBody = await _oAuthServiceClient.RefreshAccessTokenAsync(tokenDto.RefreshToken);
        if (jsonBody == default)
        {
            return GetNeedsConsentResult();
        }

        string accessToken = jsonBody.AccessToken != "" ? jsonBody.AccessToken : tokenDto.AccessToken;
        long expirationSeconds = jsonBody.ExpiresInSeconds;
        // If we get a refresh token in the response, we need to replace the refresh token. Otherwise re-use the current refresh token.
        string nextRefreshToken = jsonBody.RefreshToken ?? tokenDto.RefreshToken;
        
        var dto = new OAuthUserTokenDto
        {
            AccessToken = accessToken,
            AccessTokenExpiration = DateTimeOffset.Now + TimeSpan.FromSeconds(expirationSeconds),
            RefreshToken = nextRefreshToken
        };

        var serializedDto = JsonSerializer.Serialize(dto);
        await _userTokenStore.SetTokenAsync(
            tenantId: tenantId,
            userId:userId,
            token: serializedDto);

        return new AccessTokenResult
        {
            AccessToken = accessToken,
        };
    }

    public async Task ClaimTokenAsync(string accountLinkingToken, string tenantId, string userId, string codeVerifier)
    {
        var (acctState, exp) = await _oAuthStateService.GetAsync(accountLinkingToken);

        // ensure the PKCE verifier is correct
        var codeGuess = Pkce.Base64UrlEncodeSha256(codeVerifier);
        if (!string.Equals(acctState.CodeChallenge, codeGuess, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("PKCE verification failed:\nChallenge:{codeChallenge}\nVerifier:{verifier}\nH(Verifier):{guess}", acctState.CodeChallenge, codeVerifier, codeGuess);
            throw new Exception("PKCE code verification failed");
        }

        // ensure this isn't a replay 
        await _replayValidator.ClaimIdAsync(acctState.Id, exp);

        // Claim the oauth code from the downstream
        var oAuthResult = await _oAuthServiceClient.ClaimCodeAsync(acctState.OAuthCode);
        
        var dto = new OAuthUserTokenDto
        {
            AccessToken = oAuthResult.AccessToken,
            AccessTokenExpiration = DateTimeOffset.Now + TimeSpan.FromSeconds(oAuthResult.ExpiresInSeconds),
            RefreshToken = oAuthResult.RefreshToken
        };

        var serializedDto = JsonSerializer.Serialize(dto);

        await _userTokenStore.SetTokenAsync(tenantId: tenantId, userId: userId, serializedDto);
    }

    public async Task LogoutAsync(string tenantId, string userId)
    {
        await _userTokenStore.DeleteTokenAsync(tenantId: tenantId, userId: userId);
    }

    private NeedsConsentResult GetNeedsConsentResult()
    {
        return new NeedsConsentResult(new Uri(_options.AuthStartUri));
    }
}

public abstract class AccessTokenResultBase {}

public sealed class NeedsConsentResult : AccessTokenResultBase
{
    public Uri AuthorizeUri { get; }

    public NeedsConsentResult(Uri authorizeUri)
    {
        AuthorizeUri = authorizeUri;
    }
}

public sealed class AccessTokenResult : AccessTokenResultBase
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpirationTime { get; set; }
}
