using Microsoft.Graph;
using MSGraphSearchSample.Models;
using MSGraphSearchSample.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Interfaces
{
    public interface IGraphService
    {
        void SetAccessToken(string token);
        Task<User> GetCurrentUserInfo();

        Task<SearchResults> Search(EntityType entityTypes, string queryString, int from = 0);
    }
}
