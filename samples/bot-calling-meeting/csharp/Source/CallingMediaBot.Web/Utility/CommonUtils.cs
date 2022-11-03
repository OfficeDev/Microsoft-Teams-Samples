using System.Runtime.CompilerServices;
using Microsoft.Graph.Communications.Common.Telemetry;

namespace CallingMediaBot.Web.Utility;

public static class CommonUtils
{
    public static async Task ForgetAndLogExceptionAsync(
        this Task task,
        IGraphLogger logger,
        string? description = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            await task.ConfigureAwait(false);
            logger?.Verbose(
                $"Completed running task successfully: {description ?? string.Empty}",
                memberName: memberName,
                filePath: filePath,
                lineNumber: lineNumber);
        }
        catch (Exception e)
        {
            // Log and absorb all exceptions here.
            logger?.Error(
                e,
                $"Caught an Exception running the task: {description ?? string.Empty}",
                memberName: memberName,
                filePath: filePath,
                lineNumber: lineNumber);
        }
    }
}
