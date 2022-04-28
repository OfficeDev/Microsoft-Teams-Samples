using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Teams.Samples.AccountLinking.SampleClient.Services.Gmail;
using System.Net;
using System.Security.Claims;

namespace Microsoft.Teams.Samples.AccountLinking.SampleClient.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]

    public sealed class GmailController : ControllerBase
    {
        private readonly ILogger<GmailController> _logger;
        private readonly OAuthTokenProvider _tokenProvider;
        private readonly GmailServiceClient _gmailServiceClient;

        public GmailController(
        ILogger<GmailController> logger,
        OAuthTokenProvider tokenProvider,
        GmailServiceClient gmailServiceClient)
        {
            _logger = logger;
            _tokenProvider = tokenProvider;
            _gmailServiceClient = gmailServiceClient;
        }

        [HttpGet("gmailUserProfile")]
        public async Task<IActionResult> GetGmailUserProfile()
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
                var userProfile = await _gmailServiceClient.GetGmailUserProfile("externallinkingsample@gmail.com", accessTokenResult.AccessToken);
                _logger.LogInformation("Total {count} mails are found in the inbox", userProfile.totalMessages);
                return new JsonResult(userProfile);
            }

            _logger.LogWarning("Unknown access token return type: [{type}]", tokenResult.GetType());
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}
