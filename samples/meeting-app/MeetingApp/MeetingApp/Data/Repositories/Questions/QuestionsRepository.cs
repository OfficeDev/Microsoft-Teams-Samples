using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Repositories.Questions
{
    public class QuestionsRepository
    {
        private const string PartitionKey = "Questions";
        private readonly Lazy<Task> initializeTask;
        private CloudTable candidateCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionsRepository"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public QuestionsRepository(string connectionString)
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
            this.candidateCloudTable = cloudTableClient.GetTableReference("Questions");

            await this.candidateCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get all candidate details from table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        //public async Task<CandidateDetailEntity> GetCandidateDetailsByEmail(string email)
        //{
        //    await this.EnsureInitializedAsync().ConfigureAwait(false);

        //    var query = new TableQuery<CandidateDetailEntity>();
        //    //{
        //    //    FilterString = $"CandidateEmail eq '{email}'",
        //    //};

        //    TableContinuationToken continuationToken = null;
        //    var candidateDetails = new CandidateDetailEntity();
        //    do
        //    {
        //        var queryResult = await this.candidateCloudTable.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
        //        if (queryResult != null)
        //        {
        //            candidateDetails = queryResult.FirstOrDefault();
        //        }

        //        continuationToken = queryResult?.ContinuationToken;
        //    }
        //    while (continuationToken != null);
        //    return candidateDetails;
        //}

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
