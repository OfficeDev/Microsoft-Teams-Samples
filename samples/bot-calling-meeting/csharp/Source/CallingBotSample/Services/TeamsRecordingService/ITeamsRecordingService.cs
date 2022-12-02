// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace CallingBotSample.Services.TeamsRecordingService
{
    /// <summary>
    /// Handles recordings created by the Microsoft Teams/Microsoft Graph Interactive voice response (IVR) service
    /// </summary>
    public interface ITeamsRecordingService
    {
        /// <summary>
        /// Downloads a recording from the Teams/Microsoft Graph IVR service
        /// </summary>
        /// <param name="recordingUrl">URL of the recording</param>
        /// <param name="accessToken">Access token to download the URL</param>
        /// <returns>File location of the downloaded file on disk</returns>
        Task<string> DownloadRecording(string recordingUrl, string accessToken);

        /// <summary>
        /// Delete a recording that is saved locally.
        /// </summary>
        /// <param name="recordingRelativePath">
        /// File path of the recording.
        /// This should not include the base url, and should be relative to WebRootPath
        /// </param>
        /// <returns>Whether the file was deleted successfully</returns>
        bool DeleteRecording(string recordingRelativePath);
    }
}
