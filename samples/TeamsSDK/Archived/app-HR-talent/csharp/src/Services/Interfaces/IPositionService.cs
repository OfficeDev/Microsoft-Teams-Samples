using Microsoft.Bot.Builder;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtApp.Models.Commands;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IPositionService
    {
        Task<ReadOnlyCollection<Position>> GetAllPositions(CancellationToken cancellationToken);
        Task<ReadOnlyCollection<Position>> Search(string searchText, int maxResults, CancellationToken cancellationToken = default);
        Task<Position> GetById(int positionId, CancellationToken cancellationToken = default);
        Task<Position> AddNewPosition(string tenantId, PositionCreateCommand positionCreateCommand, CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Position>> GetOpenPositions(string recruiterNameOrAlias, CancellationToken cancellationToken = default);
        Task<Position> GetByExternalId(string externalId, CancellationToken cancellationToken = default);
    }
}