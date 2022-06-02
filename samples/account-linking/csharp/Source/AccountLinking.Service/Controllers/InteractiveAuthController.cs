using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Teams.Samples.AccountLinking.Service.State;
using Microsoft.Teams.Samples.AccountLinking.Service.TempAuth;

namespace Microsoft.Teams.Samples.AccountLinking.Service.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    private readonly IAccountLinkingStateAccessor _accountLinkingStateAccessor;
    private readonly TempAuthTokenProvider _tmpTokenProvider;

    public AuthController(
        ILogger<AuthController> logger,
        IAccountLinkingStateAccessor accountLinkingStateAccessor,
        TempAuthTokenProvider tmpTokenProvider)
    {
        _logger = logger;
        _accountLinkingStateAccessor = accountLinkingStateAccessor;
        _tmpTokenProvider = tmpTokenProvider;
    }

    [HttpPut("configuration")]
    [AccountLinkingState("token")]
    public async Task<IActionResult> SetLinkingConfigurationAsync(
        [FromBody] AccountLinkingConfiguration accountLinkingConfiguration)
    {
        var state = _accountLinkingStateAccessor.AccountLinkingState;
        if (state == default)
        {
            _logger.LogInformation("Unable to get account linking state, value is null");
            return new StatusCodeResult(500);
        }

        //TODO(nibeauli): Verify the keys in the connection configuration match the expected connection name(s)

        // Assign the configuration value
        state.ConnectionConfigurations = accountLinkingConfiguration.ConnectionConfigurations;

        //TODO(nibeauli): get the next token value and return it in the body
        var result = new ObjectResult(new { Token = string.Empty });
        await Task.CompletedTask;
        return result;
    }

    //TODO(nibeauli): remove after testing is complete
    [HttpGet("token")]
    public async Task<IActionResult> SetLinkingConfigurationAsync(
        [FromQuery] string userId = "test")
    {
        var token = await _tmpTokenProvider.SerializeAuthToken(new TempAuthToken {
            UserId = userId,
        });
        var result = new ObjectResult(new { Token = token });
        return result;
    }

    //TODO(nibeauli): remove this after testing is complete. 
    [HttpGet("configuration")]
    [AccountLinkingState("token")]
    public async Task<IActionResult> GetLinkingConfigurationAsync()
    {
        await Task.CompletedTask;
        var state = _accountLinkingStateAccessor.AccountLinkingState;
        return new OkObjectResult(state);
    }
}

