// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CallingBotSample.Services.TeamsRecordingService
{
    public class TeamsRecordingService : ITeamsRecordingService
    {
        private readonly IWebHostEnvironment environment;
        private readonly HttpClient httpClient;
        private readonly ILogger<TeamsRecordingService> logger;
        private const string DOWNLOAD_DIRECTORY = "temp";

        public TeamsRecordingService(IWebHostEnvironment environment, HttpClient httpClient, ILogger<TeamsRecordingService> logger)
        {
            this.environment = environment;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<string> DownloadRecording(string recordingUrl, string accessToken)
        {
            var fileName = $"{Guid.NewGuid()}.wav";
            var downloadDirectory = Path.Combine(environment.WebRootPath, DOWNLOAD_DIRECTORY);
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
            await File.WriteAllBytesAsync(downloadPath, content);

            return $"{DOWNLOAD_DIRECTORY}/{fileName}";
        }

        private async Task<byte[]?> GetUrlContent(string url, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            using (var result = await httpClient.GetAsync(url))
            {
                return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
            }
        }
    }
}
