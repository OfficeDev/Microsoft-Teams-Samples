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

            var exists = await this.candidateCloudTable.ExistsAsync();
            if (!exists)
            {
                await this.candidateCloudTable.CreateAsync().ConfigureAwait(false);
                await this.InitializeCandidateTableWithRecords().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Initialize candidate table with test records.
        /// </summary>
        /// <param name="entity">Represents questionSet entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public async Task<bool> InitializeCandidateTableWithRecords()
        {
            var candidateEntitySet = new List<CandidateDetailEntity> {
           new CandidateDetailEntity{
                CandidateName = "Aaron Brooker",
                Role = "Software Engineer",
                Experience = "4 years 2 mos",
                Email = "aaron.b@gmail.com",
                Mobile = "+1 98765432",
                Skills = "React JS, .Net Core",
                Source = "Website",
                Attachments = "url1,url2",
                Education = "B Tech",
                LinkedInUrl = "https://in.linkedin.com/",
                TwitterUrl = "https://twitter.com/"
            },
                new CandidateDetailEntity{
                CandidateName = "John Doe",
                Role = "Data Scientist",
                Experience = "5 years",
                Email = "john.d@gmail.com",
                Mobile = "+1 2456789",
                Skills = "Python, ML",
                Source = "Website",
                Attachments = "url1,url2",
                Education = "M Tech",
                LinkedInUrl = "https://in.linkedin.com/",
                TwitterUrl = "https://twitter.com/"
            }
            };

            //Iterating through each batch  
            foreach (var entity in candidateEntitySet)
            {
                if (entity != null)
                {
                    entity.PartitionKey = entity.Email;
                    entity.RowKey = string.Format("{0:D19}", DateTime.UtcNow.Ticks);
                    // await this.EnsureInitializedAsync().ConfigureAwait(false);
                    TableOperation addOperation = TableOperation.InsertOrReplace(entity);
                    await this.candidateCloudTable.ExecuteAsync(addOperation).ConfigureAwait(false);
                }
            }
            return true;
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
