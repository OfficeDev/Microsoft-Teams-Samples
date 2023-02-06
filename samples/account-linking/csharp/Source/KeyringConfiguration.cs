using System.ComponentModel.DataAnnotations;

namespace Microsoft.Teams.Samples.AccountLinking;

/// <summary>
/// Configuration for the ASP.NET Core Data Protections using KeyVault & Blob Storage
/// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-6.0#protectkeyswithazurekeyvault" />
/// </summary>
public class KeyringConfiguration
{
    /// <summary>
    /// The URI to the blob where the application should store the keyring file.
    /// This should be a blob storage url, E.G. https://examplestorageaccount.blob.core.windows.net/keyring/keyring.xml
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BlobUri { get; set; } = string.Empty;

    /// <summary>
    /// The URI to root key in Azure KeyVault
    /// This should be a KeyVault uri, E.G. https://example-kv.vault.azure.net/keys/KeyRingKey/ac8b36df771645b4bacf960b18c03433
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string KeyIdentifierUri { get; set; } = string.Empty;
}