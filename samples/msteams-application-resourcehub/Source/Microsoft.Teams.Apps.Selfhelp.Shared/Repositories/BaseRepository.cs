namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Base repository for the data stored in the Azure Table Storage.
    /// </summary>
    /// <typeparam name="T">Entity class type.</typeparam>
    public class BaseRepository<T>
        where T : ITableEntity, new()
    {
        /// <summary>
        /// Storage connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets task for initialization.
        /// </summary>
        protected Lazy<Task> InitializeTask { get; set; }

        /// <summary>
        /// Gets or sets Microsoft Azure Table storage table name.
        /// </summary>
        protected string TableName { get; set; }

        /// <summary>
        /// Gets or sets Table storage table PartitionKey name.
        /// </summary>
        protected string PartitionKey { get; set; }

        /// <summary>
        /// Construct a new <see cref="TableClient" /> using a <see cref="TableSharedKeyCredential" />
        /// </summary>
        protected TableClient CloudTable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
        /// Handles Microsoft Azure Table creation.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="tableName">Azure Table storage table name.</param>
        /// <param name="logger">Logs errors and information.</param>
        public BaseRepository(
            string connectionString,
            string tableName,
            string partitionKey,
            ILogger<BaseRepository<T>> logger)
        {
            this.InitializeTask = new Lazy<Task>(() => this.InitializeAsync());
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.TableName = tableName;
            this.PartitionKey = partitionKey;
            this.logger = logger;
        }

        /// <summary>
        /// Create an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be created or updated.</param>
        /// <returns>A boolean that represents whether insert or update operation is succeeded.</returns>
        public async Task<bool> CreateAsync(T entity)
        {
            try
            {
                entity.PartitionKey = this.PartitionKey;
                Response result = await this.CloudTable.AddEntityAsync(entity);
                return result.Status == (int)HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Update an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be  updated.</param>
        /// <returns>A boolean that represents whether update operation is succeeded.</returns>
        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                Response result = await this.CloudTable.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Merge);
                return result.Status == (int)HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Create or update an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be  updated.</param>
        /// <returns>A boolean that represents whether update operation is succeeded.</returns>
        public async Task<bool> UpsertAsync(T entity)
        {
            try
            {
                Response result = await this.CloudTable.UpsertEntityAsync(entity, TableUpdateMode.Replace);
                return result.Status == (int)HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Delete an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be deleted.</param>
        /// <returns>A boolean that represents whether entity is deleted.</returns>
        public async Task<bool> DeleteAsync(T entity)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            try
            {
                var partitionKey = entity.PartitionKey;
                var rowKey = entity.RowKey;
                if (entity == null)
                {
                    throw new KeyNotFoundException(
                        $"Not found in table storage. PartitionKey = {partitionKey}, RowKey = {rowKey}");
                }

                var result = await this.CloudTable.DeleteEntityAsync(partitionKey, rowKey);
                return result.Status == (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get entities from the table storage with a filter.
        /// </summary>
        /// <param name="filter">Filter to the result.</param>
        /// <returns>All data entities.</returns>
        public async Task<IEnumerable<T>> GetWithFilterAsync<T>(string filter) where T : class, Azure.Data.Tables.ITableEntity, new()
        {
            try
            {
                Pageable<T> queryResultsFilter = this.CloudTable.Query<T>(filter: filter);

                Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");

                return (IEnumerable<T>)queryResultsFilter.ToList();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get an entity by the keys in the table storage rowKey.
        /// </summary>
        /// <param name="rowKey">The row key for the entity.</param>
        /// <returns>The entity matching the keys.</returns>
        public async Task<T> GetAsync<T>(string rowKey) where T : class, Azure.Data.Tables.ITableEntity, new()
        {
            try
            {
                var queryResultsFilter = await this.CloudTable.GetEntityAsync<T>(this.PartitionKey, rowKey);
                if (queryResultsFilter != null)
                {
                    return queryResultsFilter.Value as T;
                }

                return null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get an entity by the partitionKey key in the table storage.
        /// </summary>
        /// <returns>The entity matching the keys.</returns>
        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class, Azure.Data.Tables.ITableEntity, new()
        {
            try
            {
                Pageable<T> queryResultsFilter = this.CloudTable.Query<T>(filter: $"PartitionKey eq '{this.PartitionKey}'");

                return (IEnumerable<T>)queryResultsFilter.ToList();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get an entity by the partitionKey key and filter query in the table storage.
        /// </summary>
        /// <param name="query">The row key for the entity.</param>
        /// <returns>The entity matching the keys.</returns>
        public async Task<IEnumerable<T>> GetRecordsByPartitionkeyAndFilterQueryAsync<T>(string query) where T : class, Azure.Data.Tables.ITableEntity, new()
        {
            try
            {
                Pageable<T> queryResultsFilter = this.CloudTable.Query<T>(filter: $"PartitionKey eq '{this.PartitionKey}' and {query}");

                return (IEnumerable<T>)queryResultsFilter.ToList();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Ensures Microsoft Azure Table storage should be created before working on table.
        /// </summary>
        /// <returns>Represents an asynchronous operation.</returns>
        protected async Task EnsureInitializedAsync()
        {
            await this.InitializeTask.Value;
        }

        /// <summary>
        /// Create tables if it doesn't exist.
        /// </summary>
        /// <returns>Asynchronous task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync()
        {
            try
            {
                // Construct a new table client
                this.CloudTable = new TableClient(
                    this.connectionString, this.TableName);

                // Create the table in the service.
                await this.CloudTable.CreateIfNotExistsAsync();
                Console.WriteLine($"The created table's name is {this.TableName}.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while creating the table.");
                throw;
            }
        }
    }
}