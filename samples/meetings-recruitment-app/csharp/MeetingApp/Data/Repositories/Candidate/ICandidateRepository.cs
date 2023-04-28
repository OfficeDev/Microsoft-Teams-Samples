using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingApp.Data.Models;

namespace MeetingApp.Data.Repositories
{
    /// <summary>
    /// Interface for table operations for CandidateDetails table
    /// </summary>
    public interface ICandidateRepository
    {
        /// <summary>
        /// Get all candidate details from table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<IEnumerable<CandidateDetailEntity>> GetCandidateDetails();
    }
}