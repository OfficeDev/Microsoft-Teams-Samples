namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LogEventRepository
{
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This interface lists all the methods which are used to manage log event repository.
    /// </summary>
    public interface ILogEventRepository
    {
        /// <summary>
        /// Add an event log details.
        /// </summary>
        /// <param name="entity">Log event details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> AddEventLog(EventLogEntity entity);

        /// <summary>
        /// Get all log event details by learning id.
        /// </summary>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Return all event details by learning id.</returns>
        Task<IEnumerable<EventLogEntity>> GetLogEventLearningIdAsync(string learningId);
    }
}