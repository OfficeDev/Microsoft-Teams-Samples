using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema.Teams;
using Refit;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IConnectorService
    {
        [Post("/{webhookUrl}")]
        Task<HttpResponseMessage> SendToChannelAsync(
            string webhookUrl,
            [Body(BodySerializationMethod.Serialized)] O365ConnectorCard card,
            CancellationToken cancellationToken);
    }
}
