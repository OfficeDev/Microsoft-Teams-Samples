using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using Microsoft.Identity.Web;
using System.Net;

namespace Microsoft.Teams.Samples.AccountLinking.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class AccountLinkingController : ControllerBase
{
    private readonly OAuthTokenProvider _tokenProvider;

    private readonly ILogger<AccountLinkingController> _logger;

    public AccountLinkingController(
        ILogger<AccountLinkingController> logger,
        OAuthTokenProvider tokenProvider)
    {
        _logger = logger;
        _tokenProvider = tokenProvider;
    }

    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [HttpPut("link")]
    public async Task<IActionResult> LinkAccountsAsync(
        [FromBody] AccountLinkRequestBody tokenClaimRequest)
    {
        var userId = User.FindFirstValue(ClaimConstants.ObjectId);
        var tenantId = User.FindFirstValue(ClaimConstants.TenantId);

        if (tokenClaimRequest.Code == default)
        {
            return new BadRequestObjectResult(new {
                Error = "No code in query parameters"
            });
        }

        if (tokenClaimRequest.CodeVerifier == default)
        {
            return new BadRequestObjectResult(new {
                Error = "No code verifier in query parameters"
            });
        }

        await _tokenProvider.ClaimTokenAsync(
            accountLinkingToken: tokenClaimRequest.Code, // our 'state' was given back to the caller as the 'code' for claiming
            tenantId: tenantId,
            userId: userId,
            codeVerifier: tokenClaimRequest.CodeVerifier);
        _logger.LogInformation("Linked GitHub account for: ({userId},{tenantId})", userId, tenantId);
        return new StatusCodeResult((int)HttpStatusCode.NoContent);
    }

    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [HttpDelete("link")]
    public async Task<IActionResult> UnlinkAccountsAsync()
    {
        var userId = User.FindFirstValue(ClaimConstants.ObjectId);
        var tenantId = User.FindFirstValue(ClaimConstants.TenantId);

        await _tokenProvider.LogoutAsync(tenantId: tenantId, userId: userId);

        return new StatusCodeResult((int)HttpStatusCode.NoContent);
    }
}