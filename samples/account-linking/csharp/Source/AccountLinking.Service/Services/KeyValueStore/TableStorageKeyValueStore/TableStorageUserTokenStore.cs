using Azure;
using Azure.Data.Tables;

namespace Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage.TableStorage;

/// <summary>
/// The TableStorageKeyValueStore is an implementation of the <see cref="ITokenStore"> using Azure Tables.
/// </summary>
public sealed class TableStorageKeyValueStore : IKeyValueStore
{
    private static readonly string ValueKey = "value";
    private readonly TableClient _tableClient;
    private readonly ILogger<TableStorageKeyValueStore> _logger;
    
    public TableStorageKeyValueStore(
        ILogger<TableStorageKeyValueStore> logger,
        TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    public async Task SetAsync(string key, byte[] value, CancellationToken cancellationToken = default)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var entity = new TableEntity(partitionKey: key, rowKey: key)
        {
            { ValueKey, value }
        };
        await _tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await _tableClient.DeleteEntityAsync(partitionKey: key, rowKey: key, cancellationToken: cancellationToken);
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        try
        {
            var response = await _tableClient.GetEntityAsync<TableEntity>(partitionKey: key, rowKey: key, cancellationToken: cancellationToken);
            return response?.Value.GetBinary(ValueKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return default;
        }
    }
}
