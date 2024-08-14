using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

public class MsalClient
{
    private static IPublicClientApplication pca;

    public MsalClient(string clientId, string tenantId)
    {
        pca = InitializePublicClientAsync(clientId, tenantId).GetAwaiter().GetResult();
    }

    public static async Task<IPublicClientApplication> InitializePublicClientAsync(string clientId, string tenantId)
    {
        Console.WriteLine("Starting initializePublicClient");

        var msalConfig = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .WithRedirectUri("http://localhost")  // Keeping the redirect URI from the original code
            .Build();

        pca = msalConfig;
        Console.WriteLine("Client app created");

        return pca;
    }

    public async Task<string> AcquireTokenAsync()
    {
        string accessToken = null;

        string[] scopes = new[] { "user.read" };

        try
        {
            // MSAL.NET exposes several account APIs, logic to determine which account to use is the responsibility of the developer
            var account = (await pca.GetAccountsAsync()).FirstOrDefault();

            if (account != null)
            {
                var accessTokenRequest = pca.AcquireTokenSilent(scopes, account);
                var accessTokenResponse = await accessTokenRequest.ExecuteAsync();
                accessToken = accessTokenResponse?.AccessToken;
            }
            else
            {
                var accessTokenRequest = pca.AcquireTokenInteractive(scopes);
                var accessTokenResponse = await accessTokenRequest.ExecuteAsync();
                accessToken = accessTokenResponse?.AccessToken;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while acquiring the token: {ex.Message}");
        }

        return accessToken;
    }


}
