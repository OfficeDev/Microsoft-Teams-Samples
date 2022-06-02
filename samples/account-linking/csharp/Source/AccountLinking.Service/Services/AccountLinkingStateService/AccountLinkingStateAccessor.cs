namespace Microsoft.Teams.Samples.AccountLinking.Service.State;

public interface IAccountLinkingStateAccessor
{
    public AccountLinkingState AccountLinkingState { get; }
}

public class AccountLinkingStateAccessor: IAccountLinkingStateAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AccountLinkingStateAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public AccountLinkingState AccountLinkingState 
    { 
        get
        {
            return _httpContextAccessor.HttpContext?.Features.Get<AccountLinkingState>() ?? throw new Exception("No account linking state set");
        }
    }
}