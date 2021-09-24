using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Repositories
{
    /// <summary>
    /// CandidateRepository class for table operations for CandidateDetails table
    /// </summary>
    public class CandidateRepository : ICandidateRepository
    {
        private readonly Lazy<Task> initializeTask;
        private CloudTable candidateCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="CandidateRepository"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public CandidateRepository(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Create CandidateDetails table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.candidateCloudTable = cloudTableClient.GetTableReference("CandidateDetails");

            await this.candidateCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get all candidate details from table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<IEnumerable<CandidateDetailEntity>> GetCandidateDetails()
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);

            var query = new TableQuery<CandidateDetailEntity>();

            TableContinuationToken continuationToken = null;
            var candidateDetails = new List<CandidateDetailEntity>();
            do
            {
                var queryResult = await this.candidateCloudTable.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
                if (queryResult != null)
                {
                    candidateDetails = queryResult.ToList();
                }

                continuationToken = queryResult?.ContinuationToken;
            }
            while (continuationToken != null);
            return candidateDetails;
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating table.
        /// </summary>
        /// <returns>Represent a task with initialized connection data.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value.ConfigureAwait(false);
        }
    }
}
