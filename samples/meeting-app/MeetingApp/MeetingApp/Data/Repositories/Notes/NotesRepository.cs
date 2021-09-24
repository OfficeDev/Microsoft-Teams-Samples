using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Repositories.Notes
{
    /// <summary>
    /// NotesRepository class for table operations for Notes table
    /// </summary>
    public class NotesRepository: INotesRepository
    {
        private readonly Lazy<Task> initializeTask;
        private CloudTable notesCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionsRepository"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public NotesRepository(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Create Notes table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.notesCloudTable = cloudTableClient.GetTableReference("Notes");

            await this.notesCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update notes in table storage.
        /// </summary>
        /// <param name="entity">Represents notes entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents notes entity is saved or updated.</returns>
        public async Task<TableResult> StoreOrUpdateQuestionEntityAsync(NotesEntity entity)
        {
            entity.PartitionKey = entity.CandidateEmail;
            entity.RowKey = string.Format("{0:D19}", DateTime.UtcNow.Ticks);
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.notesCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }

        /// <summary>
        /// Get notes from table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<IEnumerable<NotesEntity>> GetNotes(string email)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);

            TableContinuationToken continuationToken = null;
            TableQuery<NotesEntity> query = new TableQuery<NotesEntity>()
            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email));
            var searchResult = await this.notesCloudTable.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);

            return searchResult.ToList();
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
