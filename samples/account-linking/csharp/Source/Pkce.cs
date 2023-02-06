using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Teams.Samples.AccountLinking;

/// <summary>
/// This class is a simplified implementation of the PKCE (https://datatracker.ietf.org/doc/html/rfc7636) for
/// OAuth. 
/// 
/// This doesn't support generic 'code_challenge_methods' and instead hard-codes to 'S256'
/// </summary>
public class Pkce
{
    public static (string codeChallenge, string codeVerifier) GeneratePkceCodes()
    {
        var codeVerifier = Guid.NewGuid().ToString();
        var codeChallenge = Base64UrlEncodeSha256(codeVerifier);
        return (codeChallenge, codeVerifier);
    }

    public static string Base64UrlEncodeSha256(string value)
    {
        using var hash = SHA256.Create();
        var hashBytes = hash.ComputeHash(Encoding.ASCII.GetBytes(value));
        return Base64UrlEncoder.Encode(hashBytes);
    } 
}