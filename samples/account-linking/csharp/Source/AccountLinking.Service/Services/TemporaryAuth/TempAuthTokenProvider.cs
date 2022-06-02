using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Teams.Samples.AccountLinking.Service.TempAuth;

public class TempAuthTokenProvider
{
    private readonly ITimeLimitedDataProtector _dataProtector;
    
    public TempAuthTokenProvider(ITimeLimitedDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public async Task<TempAuthToken?> ParseTempAuthToken(string token)
    {
        await Task.CompletedTask;
        var serialized = _dataProtector.Unprotect(token);
        if (serialized == default)
        {
            return default;
        }
        return JsonSerializer.Deserialize<TempAuthToken>(serialized);
    }

    public Task<string> SerializeAuthToken(TempAuthToken token)
    {
        var serialized = JsonSerializer.Serialize(token);
        return Task.FromResult(_dataProtector.Protect(serialized, DateTimeOffset.Now + TimeSpan.FromMinutes(15)));
    }
}