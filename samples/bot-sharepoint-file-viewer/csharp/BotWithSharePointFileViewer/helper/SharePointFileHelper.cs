// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BotWithSharePointFileViewer.helper
{
    public class SharePointFileHelper
    {
        // Get sharepoint file list.
        public static async Task<List<string>> GetSharePointFile(TokenResponse tokenResponse, string sharepointSiteName, string sharepointTenantName)
        {
            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            try
            {
                var client = new SimpleGraphClient(tokenResponse.Token);
                var fileNameList = await client.GetSharePointFile(sharepointSiteName, sharepointTenantName);

                return fileNameList;
            }
            catch (ServiceException ex)
            {
                throw ex;
            }
        }

        // Upload file in sharepoint site.
        public static void UploadFileInSharepointSite(string token, string sharepointSiteName, string sharepointTenantName, string fileName, Stream stream)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            try
            {
                var client = new SimpleGraphClient(token);
                client.UploadFileInSharepointSite(sharepointSiteName, sharepointTenantName, fileName, stream);
            }
            catch (ServiceException ex)
            {
                throw ex;
            }
        }
    }
}