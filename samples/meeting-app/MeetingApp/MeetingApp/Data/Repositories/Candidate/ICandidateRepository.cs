using MeetingApp.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingApp.Data.Repositories
{
    public interface ICandidateRepository
    {
        Task<CandidateDetailEntity> GetCandidateDetailsByEmail(string email);
    }
}