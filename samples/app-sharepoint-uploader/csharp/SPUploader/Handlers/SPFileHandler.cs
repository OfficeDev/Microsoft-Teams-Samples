// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Models.Sharepoint;
using MeetingExtension_SP.Repositories;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MessageExtension_SP.Handlers
{
    /// <summary>
    /// SPFileHandler will helps for approval and reject to upload
    /// </summary>
    public class SPFileHandler
    {
        public async Task ApproveFileAsync(IConfiguration configuration)
        {
            string readFileFromTemp = System.IO.File.ReadAllText(@"Temp/TempFile.txt");
            if (System.IO.File.Exists(readFileFromTemp))
            {
                SharePointRepository repository = new SharePointRepository(configuration);                
               await repository.UploadFileToSPAsync(readFileFromTemp,  false);                            
            }            
        }

        public async Task RejectFileAsync(IConfiguration configuration)
        {
            string readFileFromTemp = System.IO.File.ReadAllText(@"Temp/TempFile.txt");
            if (System.IO.File.Exists(readFileFromTemp))
            {
                SharePointRepository repository = new SharePointRepository(configuration);
                var data = await repository.GetAllItemsAsync<DocumentLibrary>(configuration["StaggingFolder"]);
                string filename = Path.GetFileName(readFileFromTemp).Split('_')[1];
                var recentFile = data.ToList().Where(x => x.Name.ToLower().Contains(filename.ToLower())).FirstOrDefault();                              
            }           
        }
    }
}
