using Azure;
using Azure.Data.Tables;

namespace Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

/// <summary>
/// The UserTokenEntity is an implementation of the <see cref="ITableEntity" /> for persisting opaque user tokens.
/// </summary>
public class UserTokenEntity : ITableEntity
{
    public string TenantId 
    { 
        get { return PartitionKey; }
        set { PartitionKey = value; }
    }

    public string UserId
    { 
        get { return RowKey; }
        set { RowKey = value; }
    }

    public string Token { get; set; } = string.Empty;

    public string PartitionKey { get; set; } = string.Empty;

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}