using Microsoft.Graph;

namespace CallingMediaBot.Domain.Interfaces;

public interface ICallService
{
    Task Answer(string id);

    Task<Call> Create(params Identity[] users);

    Task<Call> Get(string id);

    /// <summary>
    /// Delete/Hang up a call
    /// </summary>
    /// <returns></returns>
    Task HangUp(string id);

    Task<PlayPromptOperation> PlayPrompt(string id, params MediaInfo[] mediaPrompts);

    Task<Call> Reject(string id);

    /// <summary>
    /// Redirect a call that has not been answered yet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Call> Redirect(string id);

    Task Transfer(string id);
}
