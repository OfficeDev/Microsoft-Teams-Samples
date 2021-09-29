using System.Threading.Tasks;
using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Repositories.Feedback
{
    /// <summary>
    /// Interface for table operations for Feedback table
    /// </summary>
    public interface IFeedbackRepository
    {
        /// <summary>
        /// Store feedback in table storage.
        /// </summary>
        /// <param name="entity">Represents questionSet entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        Task<TableResult> StoreFeedbackAsync(FeedbackEntity entity);
    }
}
