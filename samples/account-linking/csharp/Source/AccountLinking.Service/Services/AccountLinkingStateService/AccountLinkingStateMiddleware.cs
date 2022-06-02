
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage;
using Microsoft.Teams.Samples.AccountLinking.Service.TempAuth;

namespace Microsoft.Teams.Samples.AccountLinking.Service.State;

public class AccountLinkingStateMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITypedKeyValueStore<AccountLinkingState> _store;
    private readonly TempAuthTokenProvider _tokenProvider;
    
    public AccountLinkingStateMiddleware(
        RequestDelegate next,
        ITypedKeyValueStore<AccountLinkingState> store,
        TempAuthTokenProvider tokenProvider)
    {
        _next = next;
        _store = store;
        _tokenProvider = tokenProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpointFeature = context.Features.Get<IEndpointFeature>();
        var attribute = endpointFeature?.Endpoint?.Metadata.GetMetadata<AccountLinkingStateAttribute>();
        
        if (attribute == default)
        {
            await _next(context);
            return;
        }

        var tokenString = context.Request.Query[attribute.QueryStringParameterName].FirstOrDefault();
        if (tokenString == default)
        {
            context.Response.StatusCode = 401;
            return;
        }
        var token = await _tokenProvider.ParseTempAuthToken(tokenString);
        if (token == default)
        {
            context.Response.StatusCode = 500;
            return;
        }

        var state = await _store.GetAsync(token.UserId, context.RequestAborted);
        state ??= new AccountLinkingState
        {
            Id = token.UserId,
        };
        context.Features.Set(state);
        // Call the next delegate/middleware in the pipeline.
        await _next(context);
        var nextState = context.Features.Get<AccountLinkingState>();
        //TODO(nibeauli): if default log that happened. 
        if (nextState != default)
        {
            await _store.SetAsync(token.UserId, nextState, context.RequestAborted);
        }
    }
}