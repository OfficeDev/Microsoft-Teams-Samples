using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IBotService
    {
        Task HandleMembersAddedAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            IList<ChannelAccount> membersAdded,
            CancellationToken cancellationToken);

        Task HandleInstallationUpdateAsync(
            ITurnContext<IInstallationUpdateActivity> turnContext,
            CancellationToken cancellationToken);

        Task<IMessageActivity> OpenPositionAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);

        Task<IMessageActivity> LeaveInternalCommentAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);

        Task<IMessageActivity> ScheduleInterviewAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);

        Task HandleFileAttachments(ITurnContext turnContext, CancellationToken cancellationToken);
    }
}
