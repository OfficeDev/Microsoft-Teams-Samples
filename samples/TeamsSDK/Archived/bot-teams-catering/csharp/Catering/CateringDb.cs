using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model = Catering.Models;

namespace Catering
{
    public class CateringDb : IAsyncDisposable
    {
        private readonly string _endpointUri;
        private readonly string _primaryKey;

        // Cosmos client 
        private readonly CosmosClient _cosmosClient;
        private readonly Lazy<Database> _database;
        private readonly Lazy<Container> _container;

        private readonly string DatabaseId = "UserDB";
        private readonly string ContainerId = "Orders";
        private readonly string PartitionKeyValue = "AllUsers";

        public CateringDb(IConfiguration configuration)
        {
            _endpointUri = configuration.GetSection("CosmosEndpointUri")?.Value ?? throw new ArgumentNullException("CosmosEndpointUri");
            _primaryKey = configuration.GetSection("CosmosKey")?.Value ?? throw new ArgumentNullException("CosmosKey");

            _cosmosClient = new CosmosClient(_endpointUri, _primaryKey);
            _database = new Lazy<Database>(() =>
            {
                var task = _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
                task.Wait();
                return task.Result;
            }, isThreadSafe: true);

            _container = new Lazy<Container>(() =>
            {
                var task = _database.Value.CreateContainerIfNotExistsAsync(ContainerId, "/partitionKey");
                task.Wait();
                return task.Result;
            }, isThreadSafe: true);
        }

        public async Task<CosmosResult<Model.User>> GetRecentOrdersAsync(string? continuationToken = null)
        {
            var sqlQueryText = "SELECT * FROM c ORDER BY c.lunch.orderTimestamp DESC OFFSET 0 LIMIT 5";

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.Value.GetItemQueryIterator<Model.User>(queryDefinition, continuationToken);

            var results = new CosmosResult<Model.User>()
            {
                Items = new List<Model.User>()
            };

            if (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                results.ContinuationToken = currentResultSet.ContinuationToken;
                foreach (var conversationInfo in currentResultSet)
                {
                    results.Items.Add(conversationInfo);
                }
            }

            return results;
        }

        public async Task<Model.User> UpsertOrderAsync(Model.User user)
        {
            if (user.Id == null)
            {
                user.Id = Guid.NewGuid().ToString();
            }

            user.PartitionKey = PartitionKeyValue;
            user.Lunch.OrderTimestamp = DateTime.UtcNow;

            var response = await _container.Value.UpsertItemAsync(user, new PartitionKey(PartitionKeyValue));
            return response.Resource;
        }

        public async ValueTask DisposeAsync()
        {
            if (_cosmosClient is not null)
            {
                _cosmosClient.Dispose();
            }
        }
    }
}
