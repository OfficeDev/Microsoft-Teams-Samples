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
    public enum CardType
    {
        hero,
        thumbnail
    }

    public class WikipediaComposeExtension
    {
        const string SearchApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=[keyword]&srlimit=[limit]&sroffset=[offset]&format=json";
        const string ImageApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&formatversion=2&format=json&prop=pageimages&piprop=thumbnail&pithumbsize=250&titles=[title]";
        const string MaxComposeExtensionHistoryCountKey = "MaxComposeExtensionHistoryCount";
        public static HttpClient Client = new HttpClient();
        protected readonly IStatePropertyAccessor<UserData> _userState;
        private readonly IOptions<AzureSettings> azureSettings;
        public WikipediaComposeExtension(IStatePropertyAccessor<UserData> userState, IOptions<AzureSettings> azureSettings)
        {
            _userState = userState;
            this.azureSettings = azureSettings;
        }
        public async Task<ImageResult> SearchWikiImage(Search wikiSearch)
        {
            // a separate API call to Wikipedia is needed to fetch the page image, if it exists
            string imageApiUrl = ImageApiUrlFormat.Replace("[title]", wikiSearch.title);

            Uri apiUrl = new Uri(imageApiUrl);
            return await ProcessRequest<ImageResult>(apiUrl);
        }

        private async Task<T> ProcessRequest<T>(Uri uri)
        {
            var json = await Client.GetStringAsync(uri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string GetImageURL(ImageResult imageResult)
        {
            string imageUrl = string.Empty;

            if (imageResult != null && imageResult.query.pages != null && imageResult.query.pages.Count > 0 && imageResult.query.pages[0].thumbnail != null)
            {
                imageUrl = imageResult.query.pages[0].thumbnail.source;
            }
            else
            {
                // no image so use default Wikipedia image
                imageUrl = "https://upload.wikimedia.org/wikipedia/commons/d/de/Wikipedia_Logo_1.0.png";
            }

            return imageUrl;
        }

        public async Task<MessagingExtensionResponse> GetComposeExtensionResponseAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query)
        {
            MessagingExtensionResponse composeExtensionResponse = null;
            ImageResult imageResult = null;
            List<MessagingExtensionAttachment> composeExtensionAttachments = new List<MessagingExtensionAttachment>();
            var userData = await TemplateUtility.GetBotUserDataObject(_userState, turnContext, query);
            var userPreferredCardType = userData.ComposeExtensionCardType;
            var activity = turnContext.Activity;
            bool isSettingUrl = false;

            if (string.Equals(activity.Name.ToLower(), Strings.ComposeExtensionQuerySettingUrl))
            {
                isSettingUrl = true;
            }

            if (query.CommandId == null || query.Parameters == null)
            {
                return null;
            }
            var initialRunParameter = GetQueryParameterByName(query, Strings.manifestInitialRun);
            var queryParameter = GetQueryParameterByName(query, Strings.manifestParameterName);

            if (userData == null)
            {
                composeExtensionResponse = new MessagingExtensionResponse();
                string message = Strings.ComposeExtensionNoUserData;
                composeExtensionResponse.ComposeExtension = GetMessageResponseResult(message);
                return composeExtensionResponse;
            }

            /**
            * Below are the checks for various states that may occur
            * Note that the order of many of these blocks of code do matter
            */

            // situation where the incoming payload was received from the config popup

            if (!string.IsNullOrEmpty(query.State))
            {
                /**
                // need to keep going to return a response so do not return here
                // these variables are changed so if the word 'setting' kicked off the compose extension,
                // then the word setting will not retrigger the config experience
                **/

                queryParameter = "";
                initialRunParameter = "true";
            }

            // this is a sitaution where the user's preferences have not been set up yet
            if (userData.ComposeExtensionCardType == null && userData.ChannelId == DialogMatches.MsteamsChannelId)
            {
                composeExtensionResponse = GetConfig();
                return composeExtensionResponse;
            }
            else // This is for multi hub support (outlook and office)
            {
                userData.ComposeExtensionCardType = DialogMatches.MultiHubComposeExtensionCardType;
            }
            /**
            // this is the situation where the user has entered the word 'reset' and wants
            // to clear his/her settings
            // resetKeyword for English is "reset"
            **/

            if (string.Equals(queryParameter.ToLower(), Strings.ComposeExtensionResetKeyword))
            {
                composeExtensionResponse = new MessagingExtensionResponse();
                composeExtensionResponse.ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionResetText);
                return composeExtensionResponse;
            }

            /**
            // this is the situation where the user has entered "setting" or "settings" in order
            // to repromt the config experience
            // keywords for English are "setting" and "settings"
            **/

            if (string.Equals(queryParameter.ToLower(), Strings.ComposeExtensionSettingKeyword) || string.Equals(queryParameter.ToLower(), Strings.ComposeExtensionSettingsKeyword) || (isSettingUrl))
            {
                composeExtensionResponse = GetConfig();
                return composeExtensionResponse;
            }

            /**
            // this is the situation where the user in on the initial run of the compose extension
            // e.g. when the user first goes to the compose extension and the search bar is still blank
            // in order to get the compose extension to run the initial run, the setting "initialRun": true
            // must be set in the manifest for the compose extension
            **/

            if (initialRunParameter == "true")
            {
                // Signin Experience, please uncomment below code for Signin Experience
                // ComposeExtensionResponse = GetSignin(composeExtensionResponse);
                // Return composeExtensionResponse;

                composeExtensionResponse = new MessagingExtensionResponse();

                var historySearchWikiResult = userData.ComposeExtensionSelectedResults;
                if (historySearchWikiResult != null)
                {
                    foreach (var searchResult in historySearchWikiResult)
                    {
                        WikiHelperSearchResult wikiSearchResult = new WikiHelperSearchResult(searchResult.imageUrl, searchResult.highlightedTitle, searchResult.text);

                        // Create the card itself and the preview card based upon the information
                        var createdCardAttachment = TemplateUtility.CreateComposeExtensionCardsAttachments(wikiSearchResult, userPreferredCardType);
                        composeExtensionAttachments.Add(createdCardAttachment);
                    }

                    composeExtensionResponse = GetComposeExtensionQueryResult(composeExtensionAttachments);
                }
                else
                {
                    composeExtensionResponse.ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionInitialRunText);

                }
                return composeExtensionResponse;
            }

            /**
            * Below here is simply the logic to call the Wikipedia API and create the response for
            * a query; the general flow is to call the Wikipedia API for the query and then call the
            * Wikipedia API for each entry for the query to see if that entry has an image; in order
            * to get the asynchronous sections handled, an array of Promises for cards is used; each
            * Promise is resolved when it is discovered if an image exists for that entry; once all
            * of the Promises are resolved, the response is sent back to Teams
            */

            WikiResult wikiResult = await SearchWiki(queryParameter, query);

            // enumerate search results and build Promises for cards for response
            foreach (var searchResult in wikiResult.query.search)
            {
                //Get the Image result on the basis of Image Title one by one
                imageResult = await SearchWikiImage(searchResult);

                //Get the Image Url from imageResult
                string imageUrl = GetImageURL(imageResult);

                string cardText = searchResult.snippet + " ...";

                WikiHelperSearchResult wikiSearchResult = new WikiHelperSearchResult(imageUrl, searchResult.title, cardText);

                // Create the card itself and the preview card based upon the information
                var createdCardAttachment = TemplateUtility.CreateComposeExtensionCardsAttachments(wikiSearchResult, userPreferredCardType);
                composeExtensionAttachments.Add(createdCardAttachment);
            }

            composeExtensionResponse = GetComposeExtensionQueryResult(composeExtensionAttachments);

            return composeExtensionResponse;
        }

        /// <summary>
        /// Handle the callback received when the user selects an item from the results list        
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<MessagingExtensionResponse> HandleComposeExtensionSelectedItem(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query)
        {
            // Keep a history of recently-selected items in bot user data. The history will be returned in response to the initialRun query
            var userData = await TemplateUtility.GetBotUserDataObject(_userState, turnContext, query);

            //Get the Max number of History items from config file
            int maxComposeExtensionHistoryCount = Convert.ToInt32(this.azureSettings.Value.MaxComposeExtensionHistoryCount);

            WikiHelperSearchResult selectedItem = JsonConvert.DeserializeObject<WikiHelperSearchResult>(turnContext.Activity.Value.ToString());

            var historySearchWikiResult = userData.ComposeExtensionSelectedResults;

            //Removing other occurrences of the current selectedItem so there are not duplicates in the most recently used list
            if (historySearchWikiResult != null && historySearchWikiResult.Count > 0)
            {
                int index = 0;
                while (index < historySearchWikiResult.Count)
                {
                    if (string.Equals(historySearchWikiResult[index].highlightedTitle.ToLower(), selectedItem.highlightedTitle.ToLower()))
                    {
                        historySearchWikiResult.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            else
            {
                historySearchWikiResult = new List<WikiHelperSearchResult>();
            }

            //Add new item in list
            historySearchWikiResult.Insert(0, selectedItem);

            //Restrict the transaction History with Max Items.
            if (historySearchWikiResult.Count > maxComposeExtensionHistoryCount)
            {
                historySearchWikiResult = historySearchWikiResult.GetRange(0, maxComposeExtensionHistoryCount);
            }

            //Save the history Items in user Data
            await TemplateUtility.SaveBotUserDataObject(_userState, turnContext, historySearchWikiResult);

            MessagingExtensionResponse composeExtensionResponse = new MessagingExtensionResponse();
            List<MessagingExtensionAttachment> composeExtensionAttachment = new List<MessagingExtensionAttachment>();

            if (selectedItem != null)
            {
                // create the card itself and the preview card based upon the information
                // check user preference for which type of card to create
                var userPreferredCardType = userData.ComposeExtensionCardType;
                var createdCardAttachment = TemplateUtility.CreateComposeExtensionCardsAttachmentsSelectedItem(selectedItem, userPreferredCardType);
                composeExtensionAttachment.Add(createdCardAttachment);

                composeExtensionResponse = GetComposeExtensionQueryResult(composeExtensionAttachment);
            }
            else
            {
                composeExtensionResponse.ComposeExtension = GetMessageResponseResult(Strings.ComposeExtensionInitialRunText);
            }

            return composeExtensionResponse;
        }

        public async Task<WikiResult> SearchWiki(string queryParameter, MessagingExtensionQuery composeExtensionQuery)
        {
            string searchApiUrl = SearchApiUrlFormat.Replace("[keyword]", queryParameter);
            searchApiUrl = searchApiUrl.Replace("[limit]", composeExtensionQuery.QueryOptions.Count + "");
            searchApiUrl = searchApiUrl.Replace("[offset]", composeExtensionQuery.QueryOptions.Skip + "");

            Uri apiUrl = new Uri(searchApiUrl);
            return await ProcessRequest<WikiResult>(apiUrl);
        }

        public MessagingExtensionResult GetMessageResponseResult(string message)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult();
            composeExtensionResult.Type = "message";
            composeExtensionResult.Text = message;
            return composeExtensionResult;
        }
        public MessagingExtensionResponse GetSignin()
        {
            string configUrl = this.azureSettings.Value.BaseUri + "/Page/composeExtensionSettings.html";
            CardAction configExp = new CardAction(ActionTypes.OpenUrl, "Config", null, configUrl);
            List<CardAction> cardActions = new List<CardAction>();
            cardActions.Add(configExp);
            MessagingExtensionResponse composeExtensionResponse = new MessagingExtensionResponse();
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult();

            MessagingExtensionSuggestedAction objSuggestedAction = new MessagingExtensionSuggestedAction();
            objSuggestedAction.Actions = cardActions;

            composeExtensionResult.SuggestedActions = objSuggestedAction;
            composeExtensionResult.Type = "auth";
            composeExtensionResponse.ComposeExtension = composeExtensionResult;

            return composeExtensionResponse;
        }
        public MessagingExtensionResponse GetComposeExtensionQueryResult(List<MessagingExtensionAttachment> composeExtensionAttachments)
        {
            MessagingExtensionResponse composeExtensionResponse = new MessagingExtensionResponse();
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult();
            composeExtensionResult.Type = "result";
            composeExtensionResult.Attachments = composeExtensionAttachments;
            composeExtensionResult.AttachmentLayout = "list";
            composeExtensionResponse.ComposeExtension = composeExtensionResult;

            return composeExtensionResponse;
        }

        // return the value of the specified query parameter
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
        public MessagingExtensionResponse GetConfig()
        {
            string configUrl = this.azureSettings.Value.BaseUri + "/Page/composeExtensionSettings.html";
            CardAction configExp = new CardAction(ActionTypes.OpenUrl, "Config", null, null, null, configUrl);
            List<CardAction> cardActions = new List<CardAction>();
            cardActions.Add(configExp);
            MessagingExtensionResponse composeExtensionResponse = new MessagingExtensionResponse();
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult();

            MessagingExtensionSuggestedAction objSuggestedAction = new MessagingExtensionSuggestedAction();
            objSuggestedAction.Actions = cardActions;

            composeExtensionResult.SuggestedActions = objSuggestedAction;
            composeExtensionResult.Type = "config";
            composeExtensionResponse.ComposeExtension = composeExtensionResult;

            return composeExtensionResponse;
        }

    }
    public class WikiHelperSearchResult
    {
        public string imageUrl { get; set; }
        public string highlightedTitle { get; set; }
        public string text { get; set; }

        public WikiHelperSearchResult(string imageUrl, string highlightedTitle, string text)
        {
            this.imageUrl = imageUrl;
            this.highlightedTitle = highlightedTitle;
            this.text = text;
        }
    }

    //Wiki Json Result Object Classes
    public class Continue
    {
        public int sroffset { get; set; }
        public string @continue { get; set; }
    }

    public class Searchinfo
    {
        public int totalhits { get; set; }
    }

    public class Search
    {
        public int ns { get; set; }
        public string title { get; set; }
        public int pageid { get; set; }
        public int size { get; set; }
        public int wordcount { get; set; }
        public string snippet { get; set; }
        public string timestamp { get; set; }
    }

    public class Query
    {
        public Searchinfo searchinfo { get; set; }
        public List<Search> search { get; set; }
    }

    public class WikiResult
    {
        public string batchcomplete { get; set; }
        public Continue @continue { get; set; }
        public Query query { get; set; }
    }

    /// <summary>
    /// Image Json Object Classes
    /// </summary>
    public class Normalized
    {
        public bool fromencoded { get; set; }
        public string from { get; set; }
        public string to { get; set; }
    }

    public class Thumbnail
    {
        public string source { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Page
    {
        public int pageid { get; set; }
        public int ns { get; set; }
        public string title { get; set; }
        public Thumbnail thumbnail { get; set; }
    }

    public class QueryImage
    {
        public List<Normalized> normalized { get; set; }
        public List<Page> pages { get; set; }
    }

    public class ImageResult
    {
        public bool batchcomplete { get; set; }
        public QueryImage query { get; set; }
    }
}
