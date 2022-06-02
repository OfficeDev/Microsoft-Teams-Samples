using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;

using Microsoft.Teams.Samples.AccountLinking.Service;
using Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage;
using Microsoft.Teams.Samples.AccountLinking.Service.ReplayValidation;
using Microsoft.Teams.Samples.AccountLinking.Service.State;
using Microsoft.Teams.Samples.AccountLinking.Service.TempAuth;

var builder = WebApplication.CreateBuilder(args);
var useAzure = builder.Configuration.GetValue<bool>("UseAzure");

var services = builder.Services;

services.AddOptions<TableStorageReplayValidatorOptions>()
    .BindConfiguration("StateReplay")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<KeyringConfiguration>()
    .BindConfiguration("Keyring")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddControllers();
services.AddHttpContextAccessor();
services.AddTransient<IAccountLinkingStateAccessor, AccountLinkingStateAccessor>();
services.AddDistributedMemoryCache();
services.AddDataProtection();

services.AddSingleton(sp => 
    ActivatorUtilities
        .CreateInstance<KeyValueStoreDistributedCacheAdatper>(
            sp,
            // Use a subset of the distributed cache keyspace for the account linking state
            sp.GetRequiredService<IDistributedCache>().CreateSubCache("account_linking"))
        .ToTypedKeyValueStore<AccountLinkingState>());

services.AddTransient(sp => 
    ActivatorUtilities.CreateInstance<TempAuthTokenProvider>(
        sp,
        sp.GetDataProtector("tmpTokens").ToTimeLimitedDataProtector()));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AccountLinkingStateMiddleware>();
app.MapControllers();
app.UseDefaultFiles();
// Allow for static files to be hosted in the wwwroot directory so that the index.html can be used for the
// Microsoft Teams tab.
app.UseStaticFiles();

app.Run();
