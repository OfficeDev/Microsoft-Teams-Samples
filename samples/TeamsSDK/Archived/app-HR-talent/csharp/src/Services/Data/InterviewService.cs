using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TeamsTalentMgmtApp.Models.Commands;
using TeamsTalentMgmtApp.Models.DatabaseContext;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Services.Data
{
    public sealed class InterviewService : IInterviewService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IIncludableQueryable<Candidate, Position> _candidatesIncludableGetQuery;

        public InterviewService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            _candidatesIncludableGetQuery = _databaseContext.Candidates
                .Include(x => x.Comments)
                .Include(x => x.Interviews)
                    .ThenInclude(i => i.Recruiter)
                        .ThenInclude(r => r.Positions)
                .Include(x => x.Location)
                .Include(x => x.Position);
        }

        public async Task ScheduleInterview(ScheduleInterviewCommand scheduleInterviewCommand, CancellationToken cancellationToken = default)
        {
            var candidate = await _candidatesIncludableGetQuery.FirstOrDefaultAsync(c => c.CandidateId == scheduleInterviewCommand.CandidateId, cancellationToken);
            if (candidate != null)
            {
                candidate.Stage = InterviewStageType.Interviewing;

                await _databaseContext.Interviews.AddAsync(
                    new Interview
                    {
                        CandidateId = candidate.CandidateId,
                        InterviewDate = scheduleInterviewCommand.InterviewDate,
                        RecruiterId = scheduleInterviewCommand.InterviewerId,
                        FeedbackText = "N/A"
                    }, cancellationToken);

                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
