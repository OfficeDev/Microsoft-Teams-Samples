using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Repositories.Notes
{
    /// <summary>
    /// Interface for table operations for Notes table
    /// </summary>
    public interface INotesRepository
    {
        /// <summary>
        /// Store or update notes in table storage.
        /// </summary>
        /// <param name="entity">Represents notes entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents notes entity is saved or updated.</returns>
        Task<TableResult> StoreOrUpdateQuestionEntityAsync(NotesEntity entity);

        /// <summary>
        /// Get notes from table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<IEnumerable<NotesEntity>> GetNotes(string email);
    }
}
