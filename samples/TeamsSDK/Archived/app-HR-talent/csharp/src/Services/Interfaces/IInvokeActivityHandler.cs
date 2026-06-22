using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IInvokeActivityHandler
    {
        Task<InvokeResponse> HandleSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken);

        Task<MessagingExtensionResponse> HandleMessagingExtensionQueryAsync(
            ITurnContext turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken);

        Task<MessagingExtensionActionResponse> HandleMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken);

        Task<MessagingExtensionActionResponse> HandleMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken);

        Task HandleMessagingExtensionOnCardButtonClickedAsync(
            ITurnContext<IInvokeActivity> turnContext,
            JObject cardData,
            CancellationToken cancellationToken);

        Task<MessagingExtensionResponse> HandleAppBasedLinkQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            AppBasedLinkQuery query,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleFileConsentDeclineResponse(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleFileConsentAcceptResponse(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken);
    }
}
