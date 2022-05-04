using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using MSGraphSearchSample.Constants.AdaptiveCards;
using MSGraphSearchSample.Constants.Search;
using MSGraphSearchSample.Interfaces;
using MSGraphSearchSample.Models;
using MSGraphSearchSample.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Services
{
    public class GraphService : IGraphService
    {
        protected readonly AppConfigOptions _appconfig;
        protected readonly IGraphHelper graphHelper;
        protected readonly ILogger logger;
        protected GraphServiceClient _graphClient = null;

        public GraphService(IOptions<AppConfigOptions> options, IGraphHelper _graphHelper, ILogger<GraphService> _logger)
        {
            _appconfig = options.Value;
            graphHelper = _graphHelper;
            logger = _logger;

        }
        public void SetAccessToken(string token)
        {
            _graphClient = graphHelper.GetDelegatedServiceClient(token);
        }

        public async Task<User> GetCurrentUserInfo()
        {
            try
            {
                var user = await _graphClient
                .Me
                .Request()
                .GetAsync();

                return user;
            }
            catch (Exception ex)
            {
                logger.LogError($"Graph Service | GetCurrentUserInfo | {ex.Message}");
                return null;
            }
        }

        public async Task<SearchResults> Search(EntityType entityType, string queryString, int from = 0)
        {
            try
            {
                logger.LogInformation("Start searching...");
                var threshold = _appconfig.SearchSizeThreshold;
                var pageSize = _appconfig.SearchPageSize;
                var size = Enumerable.Range(1, threshold).Where(n => n % pageSize == 0).Last();
                var request = new List<SearchRequestObject>()
                {
                    new SearchRequestObject
                    {
                        From =from,
                        Size = size,
                        EntityTypes = new List<EntityType>(){entityType},
                        Query = new SearchQuery
                        {
                            QueryString =$"{QueryTemplates.GetQuery(entityType,queryString)}",
                            
                        },                        
                        Fields = SearchFields.GetFieldsByEntityType(entityType)
                    }
                };

                var searchResult = await
                 _graphClient
                .Search
                .Query(request)
                .Request()
                .PostAsync();

                var hitsContainer = searchResult
                        .CurrentPage
                        .FirstOrDefault()
                        .HitsContainers
                        .FirstOrDefault();

                var total = hitsContainer.Total.Value;
                var moreResultsAvailable = hitsContainer.MoreResultsAvailable.Value;
                var hits = hitsContainer.Hits;

                logger.LogInformation($"Results count: {total}");

                if (hits != null)
                {
                    logger.LogInformation($"Hints count: {hits.Count()}");

                    return new SearchResults
                    {
                        Hits = hits.ToList(),
                        Total = total,
                        EntityType = entityType,
                        From = from,
                        QueryString = queryString,
                        Action = Actions.None,
                        CurrentPage = 1
                    };
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                logger.LogError($"Graph Service | SearchClients | {ex.Message}");
                return null;
            }
        }
    }
}
