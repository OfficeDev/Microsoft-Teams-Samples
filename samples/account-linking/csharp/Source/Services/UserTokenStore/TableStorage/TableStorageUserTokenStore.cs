using Azure;
using Azure.Data.Tables;

namespace Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

/// <summary>
/// The TableStorageUserTokenStore is an implementation of the <see cref="IUserTokenStore"> using Azure Tables.
/// </summary>
public sealed class TableStorageUserTokenStore : IUserTokenStore
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TableStorageUserTokenStore> _logger;
    
    public TableStorageUserTokenStore(
        ILogger<TableStorageUserTokenStore> logger,
        TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    public async Task SetTokenAsync(string tenantId, string userId, string token)
    {
        await _tableClient.CreateIfNotExistsAsync();
        var entity = new UserTokenEntity
        {
            TenantId = tenantId,
            UserId = userId,
            Token = token
        };
        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task DeleteTokenAsync(string tenantId, string userId)
    {
        await _tableClient.CreateIfNotExistsAsync();
        await _tableClient.DeleteEntityAsync(partitionKey: tenantId, rowKey: userId);
    }

    public async Task<string?> GetTokenAsync(string tenantId, string userId)
    {
        await _tableClient.CreateIfNotExistsAsync();
        try
        {
            var response = await _tableClient.GetEntityAsync<UserTokenEntity>(partitionKey: tenantId, rowKey: userId);
            _logger.LogInformation("Found token for user ([{tenantId}], [{userId}])", tenantId, userId);
            return response?.Value?.Token;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation("No token stored for user ([{tenantId}], [{userId}])", tenantId, userId);
            return null;
        }
    }
}
