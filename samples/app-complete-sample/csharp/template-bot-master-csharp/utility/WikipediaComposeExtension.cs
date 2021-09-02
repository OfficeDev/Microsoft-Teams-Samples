using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Utility
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
        const string ComposeExtensionSelectedResultsKey = "ComposeExtensionSelectedResults";
        const string MaxComposeExtensionHistoryCountKey = "MaxComposeExtensionHistoryCount";
        public static HttpClient Client = new HttpClient();

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
}
