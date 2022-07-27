using System.ComponentModel.DataAnnotations;

namespace Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

/// <summary>
/// Options for the <see cref="TableStorageUserTokenStore" />.
/// </summary>
public sealed class TableStorageUserTokenStoreOptions
{
    /// <summary>
    /// The Azure Tables endpoint.
    /// E.G. https://examplestorageaccount.table.core.windows.net
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The name of the table to use / create in Azure Tables.
    /// </summary>
    /// <value></value>
    [Required(AllowEmptyStrings = false)]
    public string TableName { get; set; } = string.Empty;
}