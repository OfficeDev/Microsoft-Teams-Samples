namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.BingSearch
{
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    public class ContractualRule
    {
        public string _type { get; set; }
        public string targetPropertyName { get; set; }
        public bool mustBeCloseToContent { get; set; }
        public License license { get; set; }
        public string licenseNotice { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }

    public class DeepLink
    {
        public string name { get; set; }
        public string url { get; set; }
        public string snippet { get; set; }
    }

    public class Entities
    {
        public List<Value> value { get; set; }
    }

    public class EntityPresentationInfo
    {
        public string entityScenario { get; set; }
    }

    public class Image
    {
        public string name { get; set; }
        public string thumbnailUrl { get; set; }
        public List<Provider> provider { get; set; }
        public string hostPageUrl { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int sourceWidth { get; set; }
        public int sourceHeight { get; set; }
    }

    public class Item
    {
        public string answerType { get; set; }
        public int resultIndex { get; set; }
        public Value value { get; set; }
    }

    public class License
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Mainline
    {
        public List<Item> items { get; set; }
    }

    public class Pole
    {
        public List<Item> items { get; set; }
    }

    public class Provider
    {
        public string _type { get; set; }
        public string url { get; set; }
    }

    public class Publisher
    {
        public string name { get; set; }
    }

    public class QueryContext
    {
        public string originalQuery { get; set; }
    }

    public class RankingResponse
    {
        public Pole pole { get; set; }
        public Mainline mainline { get; set; }
        public Sidebar sidebar { get; set; }
    }

    public class BingSearchResult
    {
        public string _type { get; set; }
        public QueryContext queryContext { get; set; }
        public WebPages webPages { get; set; }
        public Entities entities { get; set; }
        public Videos videos { get; set; }
        public IEnumerable<ArticleEntity> articles { get; set; }
        public RankingResponse rankingResponse { get; set; }
    }

    public class Sidebar
    {
        public List<Item> items { get; set; }
    }

    public class Thumbnail
    {
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool isFamilyFriendly { get; set; }
        public string displayUrl { get; set; }
        public string snippet { get; set; }
        public List<DeepLink> deepLinks { get; set; }
        public DateTime dateLastCrawled { get; set; }
        public string language { get; set; }
        public bool isNavigational { get; set; }
        public List<ContractualRule> contractualRules { get; set; }
        public string webSearchUrl { get; set; }
        public Image image { get; set; }
        public string description { get; set; }
        public EntityPresentationInfo entityPresentationInfo { get; set; }
        public string bingId { get; set; }
        public string thumbnailUrl { get; set; }
        public DateTime datePublished { get; set; }
        public List<Publisher> publisher { get; set; }
        public bool isAccessibleForFree { get; set; }
        public string contentUrl { get; set; }
        public string hostPageUrl { get; set; }
        public string encodingFormat { get; set; }
        public string hostPageDisplayUrl { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string duration { get; set; }
        public string motionThumbnailUrl { get; set; }
        public string embedHtml { get; set; }
        public bool allowHttpsEmbed { get; set; }
        public int viewCount { get; set; }
        public Thumbnail thumbnail { get; set; }
        public bool allowMobileEmbed { get; set; }
        public bool isSuperfresh { get; set; }
    }

    public class Videos
    {
        public string id { get; set; }
        public string readLink { get; set; }
        public string webSearchUrl { get; set; }
        public bool isFamilyFriendly { get; set; }
        public List<Value> value { get; set; }
        public string scenario { get; set; }
    }

    public class WebPages
    {
        public string webSearchUrl { get; set; }
        //public int totalEstimatedMatches { get; set; }
        public List<Value> value { get; set; }
    }
}