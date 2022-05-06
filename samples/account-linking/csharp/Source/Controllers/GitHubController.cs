using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Teams.Samples.AccountLinking.GitHub;

namespace Microsoft.Teams.Samples.AccountLinking.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public sealed class GitHubController : ControllerBase
{
    private readonly ILogger<GitHubController> _logger;
    private readonly OAuthTokenProvider _tokenProvider;
    private readonly GitHubServiceClient _githubServiceClient;

    public GitHubController(
        ILogger<GitHubController> logger,
        OAuthTokenProvider tokenProvider,
        GitHubServiceClient githubServiceClient)
    {
        _logger = logger;
        _tokenProvider = tokenProvider;
        _githubServiceClient = githubServiceClient;
    }

    [HttpGet("repositories")]
    public async Task<IActionResult> GetRepositoriesAsync()
    {
        var userId = User.FindFirstValue(ClaimConstants.ObjectId);
        var tenantId = User.FindFirstValue(ClaimConstants.TenantId);
        if (userId == default)
        {
            return new BadRequestObjectResult(new { Error = "Object (User) id in token null or undefined" });
        }
        if (tenantId == default)
        {
            return new BadRequestObjectResult(new { Error = "Tenant id in token null or undefined" });
        }

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(tenantId: tenantId, userId: userId);

        if (tokenResult is NeedsConsentResult needsConsentResult)
        {
            return new ObjectResult(new 
            {
                authorize_url = needsConsentResult.AuthorizeUri,
            })
            {
                StatusCode = (int)HttpStatusCode.PreconditionFailed,
            };
        }
        else if (tokenResult is AccessTokenResult accessTokenResult)
        {
            var repositories = await _githubServiceClient.GetRepositoriesAsync(accessTokenResult.AccessToken);
            _logger.LogInformation("Repo count: {count}", repositories.Count);
            return new JsonResult(repositories.ToList());
        }

        _logger.LogWarning("Unknown access token return type: [{type}]", tokenResult.GetType());
        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
    }
}
