using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using AppCompleteSample;
using AppCompleteSample.utility;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AppCompleteSample.Utility
{
    /// <summary>
    /// Enum representing card types.
    /// </summary>
    public enum CardType
    {
        Hero,
        Thumbnail
    }

    /// <summary>
    /// Class for handling Wikipedia compose extension.
    /// </summary>
    public class WikipediaComposeExtension
    {
        private const string SearchApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=[keyword]&srlimit=[limit]&sroffset=[offset]&format=json";
        private const string ImageApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&formatversion=2&format=json&prop=pageimages&piprop=thumbnail&pithumbsize=250&titles=[title]";
        private const string MaxComposeExtensionHistoryCountKey = "MaxComposeExtensionHistoryCount";
        private static readonly HttpClient Client = new HttpClient();
        private readonly IStatePropertyAccessor<UserData> _userState;
        private readonly IOptions<AzureSettings> _azureSettings;

        public WikipediaComposeExtension(IStatePropertyAccessor<UserData> userState, IOptions<AzureSettings> azureSettings)
        {
            _userState = userState;
            _azureSettings = azureSettings;
        }

        /// <summary>
        /// Searches for a Wikipedia image.
        /// </summary>
        /// <param name="wikiSearch">The Wikipedia search result.</param>
        /// <returns>The image result.</returns>
        public async Task<ImageResult> SearchWikiImage(Search wikiSearch)
        {
            var imageApiUrl = ImageApiUrlFormat.Replace("[title]", wikiSearch.Title);
            var apiUrl = new Uri(imageApiUrl);
            return await ProcessRequest<ImageResult>(apiUrl);
        }

        private async Task<T> ProcessRequest<T>(Uri uri)
        {
            var json = await Client.GetStringAsync(uri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Gets the image URL from the image result.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <returns>The image URL.</returns>
        public string GetImageURL(ImageResult imageResult)
        {
            if (imageResult?.Query?.Pages != null && imageResult.Query.Pages.Count > 0 && imageResult.Query.Pages[0].Thumbnail != null)
            {
                return imageResult.Query.Pages[0].Thumbnail.Source;
            }

            // No image, use default Wikipedia image
            return "https://upload.wikimedia.org/wikipedia/commons/d/de/Wikipedia_Logo_1.0.png";
        }

        /// <summary>
        /// Gets the compose extension response asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The messaging extension query.</param>
        /// <returns>The messaging extension response.</returns>
        public async Task<MessagingExtensionResponse> GetComposeExtensionResponseAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query)
        {
            var composeExtensionAttachments = new List<MessagingExtensionAttachment>();
            var userData = await TemplateUtility.GetBotUserDataObject(_userState, turnContext, query);
            var userPreferredCardType = userData.ComposeExtensionCardType;
            var activity = turnContext.Activity;
            var isSettingUrl = string.Equals(activity.Name.ToLower(), Strings.ComposeExtensionQuerySettingUrl);

            if (query.CommandId == null || query.Parameters == null)
            {
                return null;
            }

            var initialRunParameter = GetQueryParameterByName(query, Strings.manifestInitialRun);
            var queryParameter = GetQueryParameterByName(query, Strings.manifestParameterName);

            if (userData == null)
            {
                return new MessagingExtensionResponse
                {
                    ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionNoUserData)
                };
            }

            // Handle various states
            if (!string.IsNullOrEmpty(query.State))
            {
                queryParameter = "";
                initialRunParameter = "true";
            }

            if (userData.ComposeExtensionCardType == null && userData.ChannelId == DialogMatches.MsteamsChannelId)
            {
                return GetConfig();
            }
            else
            {
                userData.ComposeExtensionCardType = DialogMatches.MultiHubComposeExtensionCardType;
            }

            if (string.Equals(queryParameter.ToLower(), Strings.ComposeExtensionResetKeyword))
            {
                return new MessagingExtensionResponse
                {
                    ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionResetText)
                };
            }

            if (string.Equals(queryParameter.ToLower(), Strings.ComposeExtensionSettingKeyword) || string.Equals(queryParameter.ToLower(), Strings.ComposeExtensionSettingsKeyword) || isSettingUrl)
            {
                return GetConfig();
            }

            if (initialRunParameter == "true")
            {
                var historySearchWikiResult = userData.ComposeExtensionSelectedResults;
                if (historySearchWikiResult != null)
                {
                    foreach (var searchResult in historySearchWikiResult)
                    {
                        var wikiSearchResult = new WikiHelperSearchResult(searchResult.ImageUrl, searchResult.HighlightedTitle, searchResult.Text);
                        var createdCardAttachment = TemplateUtility.CreateComposeExtensionCardsAttachments(wikiSearchResult, userPreferredCardType);
                        composeExtensionAttachments.Add(createdCardAttachment);
                    }

                    return GetComposeExtensionQueryResult(composeExtensionAttachments);
                }
                else
                {
                    return new MessagingExtensionResponse
                    {
                        ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionInitialRunText)
                    };
                }
            }

            var wikiResult = await SearchWiki(queryParameter, query);

            foreach (var searchResult in wikiResult.Query.Search)
            {
                var imageResult = await SearchWikiImage(searchResult);
                var imageUrl = GetImageURL(imageResult);
                var cardText = searchResult.Snippet + " ...";
                var wikiSearchResult = new WikiHelperSearchResult(imageUrl, searchResult.Title, cardText);
                var createdCardAttachment = TemplateUtility.CreateComposeExtensionCardsAttachments(wikiSearchResult, userPreferredCardType);
                composeExtensionAttachments.Add(createdCardAttachment);
            }

            return GetComposeExtensionQueryResult(composeExtensionAttachments);
        }

        /// <summary>
        /// Handles the callback received when the user selects an item from the results list.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The messaging extension query.</param>
        /// <returns>The messaging extension response.</returns>
        public async Task<MessagingExtensionResponse> HandleComposeExtensionSelectedItem(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query)
        {
            var userData = await TemplateUtility.GetBotUserDataObject(_userState, turnContext, query);
            var maxComposeExtensionHistoryCount = Convert.ToInt32(_azureSettings.Value.MaxComposeExtensionHistoryCount);
            var selectedItem = JsonConvert.DeserializeObject<WikiHelperSearchResult>(turnContext.Activity.Value.ToString());
            var historySearchWikiResult = userData.ComposeExtensionSelectedResults ?? new List<WikiHelperSearchResult>();

            historySearchWikiResult.RemoveAll(item => string.Equals(item.HighlightedTitle.ToLower(), selectedItem.HighlightedTitle.ToLower()));
            historySearchWikiResult.Insert(0, selectedItem);

            if (historySearchWikiResult.Count > maxComposeExtensionHistoryCount)
            {
                historySearchWikiResult = historySearchWikiResult.GetRange(0, maxComposeExtensionHistoryCount);
            }

            await TemplateUtility.SaveBotUserDataObject(_userState, turnContext, historySearchWikiResult);

            var composeExtensionAttachments = new List<MessagingExtensionAttachment>();
            if (selectedItem != null)
            {
                var userPreferredCardType = userData.ComposeExtensionCardType;
                var createdCardAttachment = TemplateUtility.CreateComposeExtensionCardsAttachmentsSelectedItem(selectedItem, userPreferredCardType);
                composeExtensionAttachments.Add(createdCardAttachment);

                return GetComposeExtensionQueryResult(composeExtensionAttachments);
            }
            else
            {
                return new MessagingExtensionResponse
                {
                    ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionInitialRunText)
                };
            }
        }

        /// <summary>
        /// Searches Wikipedia.
        /// </summary>
        /// <param name="queryParameter">The query parameter.</param>
        /// <param name="composeExtensionQuery">The compose extension query.</param>
        /// <returns>The Wikipedia result.</returns>
        public async Task<WikiResult> SearchWiki(string queryParameter, MessagingExtensionQuery composeExtensionQuery)
        {
            var searchApiUrl = SearchApiUrlFormat.Replace("[keyword]", queryParameter)
                                                 .Replace("[limit]", composeExtensionQuery.QueryOptions.Count.ToString())
                                                 .Replace("[offset]", composeExtensionQuery.QueryOptions.Skip.ToString());

            var apiUrl = new Uri(searchApiUrl);
            return await ProcessRequest<WikiResult>(apiUrl);
        }

        /// <summary>
        /// Gets the message response result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The messaging extension result.</returns>
        public MessagingExtensionResult GetMessageResponseResult(string message)
        {
            return new MessagingExtensionResult
            {
                Type = "message",
                Text = message
            };
        }

        /// <summary>
        /// Gets the sign-in response.
        /// </summary>
        /// <returns>The messaging extension response.</returns>
        public MessagingExtensionResponse GetSignin()
        {
            var configUrl = _azureSettings.Value.BaseUri + "/Page/composeExtensionSettings.html";
            var configExp = new CardAction(ActionTypes.OpenUrl, "Config", null, configUrl);
            var cardActions = new List<CardAction> { configExp };
            var composeExtensionResult = new MessagingExtensionResult
            {
                SuggestedActions = new MessagingExtensionSuggestedAction { Actions = cardActions },
                Type = "auth"
            };

            return new MessagingExtensionResponse { ComposeExtension = composeExtensionResult };
        }

        /// <summary>
        /// Gets the compose extension query result.
        /// </summary>
        /// <param name="composeExtensionAttachments">The compose extension attachments.</param>
        /// <returns>The messaging extension response.</returns>
        public MessagingExtensionResponse GetComposeExtensionQueryResult(List<MessagingExtensionAttachment> composeExtensionAttachments)
        {
            var composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                Attachments = composeExtensionAttachments,
                AttachmentLayout = "list"
            };

            return new MessagingExtensionResponse { ComposeExtension = composeExtensionResult };
        }

        /// <summary>
        /// Gets the value of the specified query parameter.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public string GetQueryParameterByName(MessagingExtensionQuery query, string name)
        {
            foreach (var param in query.Parameters)
            {
                if (param.Name == name)
                {
                    return param.Value.ToString();
                }
            }

            return "";
        }

        /// <summary>
        /// Gets the configuration response.
        /// </summary>
        /// <returns>The messaging extension response.</returns>
        public MessagingExtensionResponse GetConfig()
        {
            var configUrl = _azureSettings.Value.BaseUri + "/Page/composeExtensionSettings.html";
            var configExp = new CardAction(ActionTypes.OpenUrl, "Config", null, null, null, configUrl);
            var cardActions = new List<CardAction> { configExp };
            var composeExtensionResult = new MessagingExtensionResult
            {
                SuggestedActions = new MessagingExtensionSuggestedAction { Actions = cardActions },
                Type = "config"
            };

            return new MessagingExtensionResponse { ComposeExtension = composeExtensionResult };
        }
    }
    /// <summary>
    /// Represents a Wikipedia search result.
    /// </summary>
    public class WikiHelperSearchResult
    {
        public string ImageUrl { get; set; }
        public string HighlightedTitle { get; set; }
        public string Text { get; set; }

        public WikiHelperSearchResult(string imageUrl, string highlightedTitle, string text)
        {
            ImageUrl = imageUrl;
            HighlightedTitle = highlightedTitle;
            Text = text;
        }
    }

    // Wiki JSON result object classes
    public class Continue
    {
        public int Sroffset { get; set; }
        public string ContinueToken { get; set; }
    }

    public class Searchinfo
    {
        public int Totalhits { get; set; }
    }

    public class Search
    {
        public int Ns { get; set; }
        public string Title { get; set; }
        public int Pageid { get; set; }
        public int Size { get; set; }
        public int Wordcount { get; set; }
        public string Snippet { get; set; }
        public string Timestamp { get; set; }
    }

    public class Query
    {
        public Searchinfo Searchinfo { get; set; }
        public List<Search> Search { get; set; }
    }

    public class WikiResult
    {
        public string Batchcomplete { get; set; }
        public Continue Continue { get; set; }
        public Query Query { get; set; }
    }

    /// <summary>
    /// Image JSON object classes.
    /// </summary>
    public class Normalized
    {
        public bool Fromencoded { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    public class Thumbnail
    {
        public string Source { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Page
    {
        public int Pageid { get; set; }
        public int Ns { get; set; }
        public string Title { get; set; }
        public Thumbnail Thumbnail { get; set; }
    }

    public class QueryImage
    {
        public List<Normalized> Normalized { get; set; }
        public List<Page> Pages { get; set; }
    }

    public class ImageResult
    {
        public bool Batchcomplete { get; set; }
        public QueryImage Query { get; set; }
    }
}
