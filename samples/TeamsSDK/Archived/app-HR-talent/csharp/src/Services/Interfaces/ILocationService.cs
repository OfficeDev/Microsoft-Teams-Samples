using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface ILocationService
    {
        Task<ReadOnlyCollection<Location>> GetAllLocations(CancellationToken cancellationToken = default);
    }
}