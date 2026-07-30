using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtApp.Models.Commands;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface ICandidateService
    {
        Task AddComment(LeaveCommentCommand leaveCommentCommand, string authorName, CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Candidate>> Search(string searchText, int maxResults, CancellationToken cancellationToken = default);
        Task<Candidate> GetById(int id, CancellationToken cancellationToken);
    }
}
