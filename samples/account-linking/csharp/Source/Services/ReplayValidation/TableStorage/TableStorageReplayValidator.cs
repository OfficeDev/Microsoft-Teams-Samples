using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace Microsoft.Teams.Samples.AccountLinking.ReplayValidation;

public sealed class TableStorageReplayValidator : IReplayValidator
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<TableStorageReplayValidator> _logger;

    private readonly TimeSpan _bucketSize;
    private readonly string _tablePrefix;

    public TableStorageReplayValidator(
        TableServiceClient tableServiceClient,
        ILogger<TableStorageReplayValidator> logger,
        IOptions<TableStorageReplayValidatorOptions> options)
    {
        _logger = logger;
        _tablePrefix = options.Value.TablePrefix;
        _bucketSize = options.Value.BucketSize;
        _tableServiceClient = tableServiceClient;
    }

    public async Task<bool> ClaimIdAsync(string jti, DateTimeOffset expiration)
    {
        var tableClient = await GetTableClientForTime(expiration);
        try
        {
            await tableClient.AddEntityAsync(new TableEntity(jti, jti));
        }
        catch (RequestFailedException)
        {
            _logger.LogInformation("Failed to add entity, JTI has been seen before");
            return false;
        }
        return true;
    }

    private async Task<TableClient> GetTableClientForTime(DateTimeOffset dt)
    {
        var bucketId = GetBucketNameForTimeSpan(dt);
        var tableName = GetTableName(bucketId);
        _logger.LogInformation("Getting client for table: [{tableName}]", tableName);

        var tc = _tableServiceClient.GetTableClient(tableName);
        await Task.WhenAll(
            tc.CreateIfNotExistsAsync(),
            DeleteExpiredTables(DateTimeOffset.UtcNow)
        );
        return tc;
    }

    private async Task DeleteExpiredTables(DateTimeOffset endTime)
    {
        // Note: in practice you would likely debounce / limit the number of times this method gets called
        // For brevity we just do it on every read operation.
        var minBucket = GetBucketNameForTimeSpan(endTime);
        _logger.LogInformation("Deleting tables before: [{bucket}]", minBucket);

        await foreach(var table in _tableServiceClient.QueryAsync())
        {
            // Skip tables that aren't a part of this, they've done nothing wrong.
            if (!table.Name.StartsWith(_tablePrefix))
            {
                continue;
            }

            var bucketId = GetBucketId(table.Name);
            if (bucketId < minBucket)
            {
                _logger.LogInformation("Deleting expired table: [{tableName}]", table.Name);
                await _tableServiceClient.DeleteTableAsync(table.Name);
            }
        }
    }

    private long GetBucketNameForTimeSpan(DateTimeOffset dt)
    {
        var unixSeconds = dt.ToUnixTimeSeconds();
        var deltaSeconds = unixSeconds % Convert.ToInt64(Math.Floor(_bucketSize.TotalSeconds));
        // remove the remainder
        return unixSeconds - deltaSeconds;
    }

    private string GetTableName(long bucketId)
    {
        return $"{_tablePrefix}{bucketId}";
    }

    private long? GetBucketId(string tableName)
    {
        var prefix = $"{_tablePrefix}";
        if(!tableName.StartsWith(prefix))
        {
            return null;
        }

        return long.TryParse(tableName.AsSpan(prefix.Length), out long result)
            ? result
            : default;
    }
}
