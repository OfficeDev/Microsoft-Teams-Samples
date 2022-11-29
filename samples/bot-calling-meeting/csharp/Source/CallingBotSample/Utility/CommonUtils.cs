// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Graph.Communications.Common.Telemetry;

namespace CallingBotSample.Utility
{
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
}
