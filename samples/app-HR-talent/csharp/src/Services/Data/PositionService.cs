using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TeamsTalentMgmtApp.Models.Commands;
using TeamsTalentMgmtApp.Models.DatabaseContext;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Services.Data
{
    public sealed class PositionService : IPositionService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IIncludableQueryable<Position, Recruiter> _positionsIncludableGetQuery;

        public PositionService(
            DatabaseContext databaseContext,
            INotificationService notificationService,
            IMapper mapper)
        {
            _databaseContext = databaseContext;
            _notificationService = notificationService;
            _mapper = mapper;
            _positionsIncludableGetQuery = _databaseContext.Positions
                .Include(x => x.Candidates)
                    .ThenInclude(c => c.Location)
                .Include(x => x.Candidates)
                    .ThenInclude(c => c.Comments)
                .Include(x => x.Candidates)
                    .ThenInclude(c => c.Interviews)
                        .ThenInclude(i => i.Recruiter)
                            .ThenInclude(r => r.Positions)
                .Include(x => x.Location)
                .Include(x => x.HiringManager);
        }

        public async Task<ReadOnlyCollection<Position>> GetAllPositions(CancellationToken cancellationToken)
            => Array.AsReadOnly(await _positionsIncludableGetQuery.ToArrayAsync(cancellationToken));

        public async Task<ReadOnlyCollection<Position>> Search(string searchText, int maxResults, CancellationToken cancellationToken)
        {
            Position[] positions;
            if (string.IsNullOrEmpty(searchText))
            {
                positions = await _positionsIncludableGetQuery
                    .OrderBy(x => x.DaysOpen)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken);
            }
            else
            {
                searchText = searchText.ToLowerInvariant();

                positions = await _positionsIncludableGetQuery
                    .Where(x => x.Title.ToLowerInvariant().Contains(searchText) ||
                               x.PositionExternalId.ToLowerInvariant().Contains(searchText))
                    .OrderBy(x => x.DaysOpen)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken);
            }

            return Array.AsReadOnly(positions);
        }

        public Task<Position> GetById(int positionId, CancellationToken cancellationToken = default)
            => _positionsIncludableGetQuery
                .FirstOrDefaultAsync(x => x.PositionId == positionId, cancellationToken);

        public async Task<Position> AddNewPosition(string tenantId, PositionCreateCommand positionCreateCommand, CancellationToken cancellationToken)
        {
            var position = _mapper.Map<Position>(positionCreateCommand);

            await _databaseContext.Positions.AddAsync(position, cancellationToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            position = await GetById(position.PositionId, cancellationToken);
            await _notificationService.NotifyRecruiterAboutNewOpenPosition(tenantId, position, cancellationToken);

            return position;
        }

        public async Task<ReadOnlyCollection<Position>> GetOpenPositions(string recruiterNameOrAlias, CancellationToken cancellationToken)
        {
            var recruiter = await _databaseContext.Recruiters
                .FirstOrDefaultAsync(
                    x =>
                    string.Equals(x.Alias, recruiterNameOrAlias, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, recruiterNameOrAlias), cancellationToken);

            Position[] positions;
            if (recruiter is null)
            {
                positions = await _positionsIncludableGetQuery.ToArrayAsync(cancellationToken);
            }
            else
            {
                positions = await _positionsIncludableGetQuery
                    .Where(x => x.HiringManager.RecruiterId == recruiter.RecruiterId)
                    .ToArrayAsync(cancellationToken);
            }

            return Array.AsReadOnly(positions);
        }

        public Task<Position> GetByExternalId(string externalId, CancellationToken cancellationToken)
        {
            return _positionsIncludableGetQuery.FirstOrDefaultAsync(
                x => string.Equals(x.PositionExternalId, externalId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);
        }
    }
}
