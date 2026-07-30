using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TeamsTalentMgmtApp.Models.Commands;
using TeamsTalentMgmtApp.Models.DatabaseContext;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Services.Data
{
    public sealed class CandidateService : ICandidateService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly INotificationService _notificationService;
        private readonly IIncludableQueryable<Candidate, Position> _candidatesIncludableGetQuery;

        public CandidateService(
            DatabaseContext databaseContext,
            INotificationService notificationService)
        {
            _databaseContext = databaseContext;
            _notificationService = notificationService;
            _candidatesIncludableGetQuery = _databaseContext.Candidates
                .Include(x => x.Comments)
                .Include(x => x.Interviews)
                    .ThenInclude(c => c.Recruiter)
                        .ThenInclude(r => r.Positions)
                .Include(x => x.Location)
                .Include(x => x.Position);
        }

        public async Task AddComment(LeaveCommentCommand leaveCommentCommand, string authorName, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(leaveCommentCommand?.Comment))
            {
                var candidate = await GetById(leaveCommentCommand.CandidateId, cancellationToken);
                var recruiter = await _databaseContext.Recruiters.FirstOrDefaultAsync(x => string.Equals(x.Name, authorName), cancellationToken);

                candidate.Comments.Add(new Comment
                {
                    CandidateId = leaveCommentCommand.CandidateId,
                    Text = leaveCommentCommand.Comment,
                    AuthorName = authorName,
                    AuthorProfilePicture = recruiter?.ProfilePicture,
                    AuthorRole = recruiter?.Role.ToString()
                });
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<ReadOnlyCollection<Candidate>> Search(string searchText, int maxResults, CancellationToken cancellationToken = default)
        {
            Candidate[] candidates;

            if (!string.IsNullOrEmpty(searchText))
            {
                if (int.TryParse(searchText, out var id))
                {
                    var candidate = await GetById(id, cancellationToken);
                    candidates = new[] { candidate };
                }
                else
                {
                    candidates = await _candidatesIncludableGetQuery
                        .Where(x => x.Name.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
                        .Take(maxResults)
                        .ToArrayAsync(cancellationToken);
                }
            }
            else
            {
                candidates = await _candidatesIncludableGetQuery
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken);
            }

            return Array.AsReadOnly(candidates);
        }

        public Task<Candidate> GetById(int id, CancellationToken cancellationToken)
            => _candidatesIncludableGetQuery.FirstOrDefaultAsync(x => x.CandidateId == id, cancellationToken);
    }
}
