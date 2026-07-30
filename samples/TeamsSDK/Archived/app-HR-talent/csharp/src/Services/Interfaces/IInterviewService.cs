using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtApp.Models.Commands;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IInterviewService
    {
        Task ScheduleInterview(ScheduleInterviewCommand scheduleInterviewCommand, CancellationToken cancellationToken = default);
    }
}