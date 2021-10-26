using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotWithSharePointFileViewer.helper
{
    public class GetSharePointFileHelper
    {
        // Get share point file.
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
        public static bool UploadFileInSharepointSite(TokenResponse tokenResponse, string sharepointSiteName, string sharepointTenantName, string fileName)
        {
            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            try
            {
                var client = new SimpleGraphClient(tokenResponse.Token);
                client.UploadFileInSharepointSite(sharepointSiteName, sharepointTenantName, fileName);

                return true;
            }
            catch (ServiceException ex)
            {
                throw ex;
            }
        }
    }
}