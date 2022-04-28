using AdaptiveCards.Templating;
using MSGraphSearchSample.Interfaces;
using Newtonsoft.Json;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using MSGraphSearchSample.Models.Search;
using Microsoft.Extensions.Logging;
using MSGraphSearchSample.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System;
using MSGraphSearchSample.Constants.AdaptiveCards;
using MSGraphSearchSample.Constants;
using Microsoft.Graph;

namespace MSGraphSearchSample.Services
{
    public class AdaptiveCardService : IAdaptiveCardService
    {
        protected readonly AppConfigOptions appconfig;
        protected readonly ILogger logger;
        protected readonly IFileService fileService;

        public AdaptiveCardService(IOptions<AppConfigOptions> options,ILogger<AdaptiveCardService> _logger,IFileService _fileService)
        {
            fileService = _fileService;
            appconfig = options.Value;
            logger = _logger;

        }
        public string BindData<T>(string adaptiveCard, T data)
        {
            if (data == null)
                return adaptiveCard;

            var adaptiveCardObject = JsonConvert.DeserializeObject(adaptiveCard);
            var template = new AdaptiveCardTemplate(adaptiveCardObject);
            var newAdaptiveCard = template.Expand(data);
            return newAdaptiveCard;
        }

        public AdaptiveElement ConvertJsonToAdaptiveElement(string content)
        {
            return JsonConvert.DeserializeObject<AdaptiveElement>(content);
        }

        public List<AdaptiveElement> GetElements<T>(List<T> items)
        {
            var typeName = typeof(T).GetFriendlyName();
            var rowContainerString = fileService.GetCard($"{typeName}Container");
            List<AdaptiveElement> elements = new List<AdaptiveElement>();
            items.ForEach(item =>
            {
                var rowData = BindData(rowContainerString, item);
                var element = ConvertJsonToAdaptiveElement(rowData);
                elements.Add(element);
            });
            return elements;            
        }

        public AdaptiveElement GetSearchResultsBody(List<AdaptiveElement> elements, SearchResults searchResults)
        {
            var resultContainers = new List<AdaptiveElement>();
            var pageSize = appconfig.SearchPageSize;
            var currentPage = searchResults.CurrentPage;
            var pageNumber = searchResults.CurrentPage;
            var entityTypes = searchResults.EntityType;
            var totalPages = elements.Page(pageSize).Count();
            var totalResult = searchResults.Total;
            var action = searchResults.Action;
            var from = searchResults.From;
            var totalResultPages = ((totalResult - 1) / pageSize) + 1;
            var resultContainerId = $"results-{Guid.NewGuid()}";
            var rootContainerId = $"results-{Guid.NewGuid()}";

            var currentActivePage = 1;
            foreach (var page in elements.Page(pageSize))
            {
                
                var isVisible = false;
                var isFirstPage = currentActivePage == 1;
                var isLastPage = currentActivePage == totalPages;
                switch (action)
                {
                    case Actions.Previous:
                        isVisible = isLastPage;
                        break;
                    case Actions.Next:
                        isVisible = isFirstPage;
                        break;
                    default:
                        isVisible = pageNumber == 1;
                        break;
                }

                var pageContainer = new AdaptiveContainer()
                {
                    Id = $"page{pageNumber}",
                    IsVisible = isVisible,
                    Items = page.ToList()
                };

                if (totalResult > pageSize)
                {
                    var chunksSize = Enumerable.Range(1, appconfig.SearchSizeThreshold).Where(n => n % pageSize == 0).Last();
                    var previousFrom = from == 0 ? 0 : from - chunksSize;
                    var pagesPerQueryCount = ((appconfig.SearchSizeThreshold - 1) / pageSize) + 1;
                    var currentPagesCount = ((totalPages - 1) / pageSize) + 1;
                    var lastPaging = currentPagesCount < pagesPerQueryCount;
                    var previousData = new CardTaskFetchValue<PagingData>
                    {
                        Data = new PagingData { 
                            From = previousFrom, 
                            PageNumber = lastPaging ? currentPage - pagesPerQueryCount : currentPage - totalPages,
                            EntityType = entityTypes,
                            QueryString = searchResults.QueryString
                        },
                        Task = Tasks.Paging
                    };

                    var nextFrom = from + chunksSize;
                    var nextData = new CardTaskFetchValue<PagingData>
                    {
                        Data = new PagingData { 
                            From = nextFrom, 
                            PageNumber = currentPage + totalPages,
                            EntityType=entityTypes,
                            QueryString = searchResults.QueryString                           
                        },
                        Task = Tasks.Paging
                    };

                    pageContainer.Items.Add(new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveActionSet()
                                            {
                                                Actions = new List<AdaptiveAction>()
                                                {
                                                    pageNumber == 1 ? new AdaptiveToggleVisibilityAction()
                                                    {
                                                        Title = " ",
                                                        IconUrl = Icons.PreviousIcon,
                                                        TargetElements = new List<AdaptiveTargetElement>()
                                                    }: (
                                                    isFirstPage ?
                                                    new AdaptiveExecuteAction()
                                                    {
                                                        Title = " ",
                                                        IconUrl = Icons.PreviousIcon,
                                                        Verb = Actions.Previous,
                                                        Data = previousData

                                                    }
                                                    :new AdaptiveToggleVisibilityAction()
                                                    {
                                                        Title = " ",
                                                        IconUrl = Icons.PreviousIcon,
                                                        TargetElements = new List<AdaptiveTargetElement>() {
                                                            new AdaptiveTargetElement($"page{pageNumber}"),
                                                            new AdaptiveTargetElement($"page{pageNumber - 1}")}
                                                    }),
                                                    pageNumber == totalResultPages ?
                                                    new AdaptiveToggleVisibilityAction()
                                                    {
                                                        Title = " ",
                                                        IconUrl = Icons.NextIcon,
                                                        TargetElements = new List<AdaptiveTargetElement>()
                                                    }:
                                                    (
                                                    isLastPage?
                                                     new AdaptiveExecuteAction()
                                                    {
                                                        Title = " ",
                                                        IconUrl = Icons.NextIcon,
                                                        Verb = Actions.Next,
                                                        Data = nextData

                                                    }
                                                    :
                                                        new AdaptiveToggleVisibilityAction()
                                                        {
                                                            Title = " ",
                                                            IconUrl = Icons.NextIcon,
                                                            TargetElements = pageNumber != totalPages ?
                                                            new List<AdaptiveTargetElement>() {
                                                                new AdaptiveTargetElement($"page{pageNumber}"),
                                                                new AdaptiveTargetElement($"page{pageNumber + 1}")
                                                            }
                                                            : new List<AdaptiveTargetElement>()
                                                        }
                                                    )
                                                }
                                            }
                                        }
                                    },
                                    new AdaptiveColumn()
                                    {
                                        VerticalContentAlignment= AdaptiveVerticalContentAlignment.Center,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                                Text = $"{(pageNumber != 1 ? pageNumber * pageSize - (pageSize-1) : 1)}-{(pageNumber != totalResultPages ? (pageNumber * pageSize): totalResult)} of {totalResult}"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }

                resultContainers.Add(pageContainer);
                pageNumber += 1;
                currentActivePage += 1;
            }

            var resultContainer = new AdaptiveContainer();
            resultContainer.Id = resultContainerId;
            resultContainer.Items.AddRange(resultContainers);
            return resultContainer;
        }

        public List<AdaptiveElement> GetSearchResultsContainers(SearchResults results)
        {
            // populate header object
            var headerContent = fileService.GetCard("SearchResultHeader");
            var headerObject = BindData(headerContent, new HeaderData() { Total = results.Total });
            var headerElement = ConvertJsonToAdaptiveElement(headerObject);

            var bodyContainer = GetBodyContainer(results);

            var cardElements = new List<AdaptiveElement>();
            cardElements.Add(headerElement);
            cardElements.Add(bodyContainer);
            return cardElements;
        }

        private AdaptiveElement GetBodyContainer(SearchResults results)
        {
            var entityType = results.EntityType;
            AdaptiveElement bodyContainer = new AdaptiveContainer();
            switch (entityType)
            {
                case EntityType.Event:
                    var events = Mappers.DataMapper.GetEvents(results.Hits);
                    var elements = GetElements(events);
                    bodyContainer = GetSearchResultsBody(elements, results);
                    break;
                case EntityType.DriveItem:
                    var files = Mappers.DataMapper.GetFiles(results.Hits);
                    var fileElements =GetElements(files);
                    bodyContainer = GetSearchResultsBody(fileElements, results);
                    break;
                case EntityType.ListItem:
                    var items = Mappers.DataMapper.GetListItems(results.Hits);
                    var itemElements = GetElements(items);
                    bodyContainer = GetSearchResultsBody(itemElements, results);
                    break;
                case EntityType.Message:
                    var messages = Mappers.DataMapper.GetMessages(results.Hits);
                    var messageElements = GetElements(messages);
                    bodyContainer = GetSearchResultsBody(messageElements, results);
                    break;
            }
            return bodyContainer;
        }

    }
}
