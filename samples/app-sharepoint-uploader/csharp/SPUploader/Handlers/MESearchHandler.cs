// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Models;
using MeetingExtension_SP.Models.Sharepoint;
using MeetingExtension_SP.Repositories;
using MessageExtension_SP.Helpers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingExtension_SP.Handlers
{
    /// <summary>
    /// Search handler helps to search a file from a sharepoint library
    /// </summary>
    public class MESearchHandler
    {
        public static async Task<List<AssetCard>> Search(string command, string assetTitle, IConfiguration configuration)
        {
            List<DocumentLibrary> assetCards=new List<DocumentLibrary>();
            SharePointRepository repository = new SharePointRepository(configuration);
            var data = await repository.GetAllItemsAsync<DocumentLibrary>(configuration["ApprovedFolder"]);

            var filteredData = data.ToList().Where(x => x.Name.ToLower().Contains(assetTitle)).ToList();

            if (command == Constants.RecentlyAdded)
            {
                filteredData = data.ToList().Where(x => x.Name.ToLower().Contains(assetTitle)).OrderByDescending(x=>x.TimeLastModified).ToList();
            }


            List<AssetCard> assetData = new List<AssetCard>();
            foreach (var val in filteredData)
            {
                assetData.Add(new AssetCard()
                {
                    Description = val.Description,
                    ServerRelativeUrl =val.LinkingUri!=null?val.LinkingUri: configuration["BaseURL"] + val.ServerRelativeUrl,
                    Title = val.Name,
                    ImageUrl= configuration["BaseUri"] + "/Images/MSC17_cloud_006.png",
                });
            }
            return assetData.ToList();
        }
    }
}
