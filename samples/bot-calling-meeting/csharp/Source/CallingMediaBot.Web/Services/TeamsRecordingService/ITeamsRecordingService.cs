// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.TeamsRecordingService;

public interface ITeamsRecordingService
{
    Task<string> DownloadRecording(string recordingUrl, string accessToken);
}
