// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CallingBotSample.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CallingBotSample.Services.TeamsRecordingService
{
    /// <inheritdoc/>
    public class TeamsRecordingService : ITeamsRecordingService
    {
        private readonly IWebHostEnvironment environment;
        private readonly HttpClient httpClient;
        private readonly BotOptions botOptions;
        private readonly ILogger<TeamsRecordingService> logger;

        public TeamsRecordingService(IWebHostEnvironment environment, HttpClient httpClient, IOptions<BotOptions> botOptions, ILogger<TeamsRecordingService> logger)
        {
            this.environment = environment;
            this.httpClient = httpClient;
            this.botOptions = botOptions.Value;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> DownloadRecording(string recordingUrl, string accessToken)
        {
            var fileName = $"{Guid.NewGuid()}.wav";
            var downloadDirectory = Path.Combine(environment.WebRootPath, botOptions.RecordingDownloadDirectory);
            var downloadPath = $"{downloadDirectory}/{fileName}";

            var content = await GetUrlContent(recordingUrl, accessToken);

            if (content == null)
            {
                throw new Exception("Unable to download file");
            }

            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            // In our sample we are downloading the recording to the server's content directory. In an actual deployment
            // if might be better to upload the file to blob storage where you can use SAS tokens and policies to limit
            // access to the files
            await File.WriteAllBytesAsync(downloadPath, content);

            return $"{botOptions.RecordingDownloadDirectory}/{fileName}";
        }

        public bool DeleteRecording(string recordingRelativePath)
        {
            var recordingPath = Path.Combine(environment.WebRootPath, recordingRelativePath);

            try
            {
                File.Delete(recordingPath);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting recording.");

                return false;
            }
        }

        private async Task<byte[]?> GetUrlContent(string url, string accessToken)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var result = await httpClient.SendAsync(requestMessage);

                return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
            }
        }
    }
}
