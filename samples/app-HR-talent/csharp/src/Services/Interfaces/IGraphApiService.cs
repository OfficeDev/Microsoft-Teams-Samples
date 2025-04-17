using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<(string upn, string chatId)> GetProactiveChatIdForUser(string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken);

        Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken);

        Task<InstallResult> InstallBotForUser(string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken);
    }

    public enum InstallResult
    {
        InstallSuccess,
        AliasNotFound,
        InstallFailed
    }
}
