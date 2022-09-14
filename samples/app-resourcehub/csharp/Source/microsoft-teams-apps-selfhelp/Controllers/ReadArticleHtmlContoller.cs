namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using System.Net;
    using System.Text;
    using HtmlAgilityPack;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Selfhelp.Models.Configuration;

    /// <summary>
    /// Controller for read article html content details.
    /// </summary>
    [Route("api/readarticle")]
    public class ReadArticleHtmlContoller : BaseController
    {
        private readonly ILogger<BingSearchContoller> logger;
        private const string article_url_supportSite = "support.microsoft.com";
        private const string article_url_techcommunitySite = "techcommunity.microsoft.com";
        private const string supportSite_html_tag_key = "ocpArticleContent";
        private const string techcommunitySite_html_tag_key = "custom-blog-article-wrapper";
        private const string techcommunitySite_publish_by_tag_key = "user-login";
        private const string techcommunitySite_publish_On_tag_key = "ba-published-date";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadArticleHtmlContoller"/> class.
        /// </summary>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        public ReadArticleHtmlContoller(
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.logger = loggerFactory?.CreateLogger<BingSearchContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Get article content from MS site.
        /// </summary>
        /// <param name="query">The query details.</param>
        /// <returns>Returns article HTML content.</returns>
        [HttpPost]
        public async Task<IActionResult> GetArticleFromUrl([FromBody] ArticleQuery query)
        {
            try
            {
                Uri myUri = new Uri(query.ArticleUrl);
                string host = myUri.Host;
                HttpClient http = new HttpClient();
                var response = await http.GetByteArrayAsync(query.ArticleUrl);
                string source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument resultat = new HtmlDocument();
                resultat.LoadHtml(source);

                if (host.Equals(article_url_supportSite))
                {
                    var htmlElement = resultat.DocumentNode.Descendants()
                        .Where(x => (x.Name == "article" && x.Attributes.Select(a => a.Value).Contains(supportSite_html_tag_key))).FirstOrDefault();

                    return this.Ok(new ArticleHtmlResponse()
                    {
                        Html = htmlElement?.InnerHtml,
                        PublishedBy = "",
                        PublishedOn = "",
                    });
                }
                else if (host.Equals(article_url_techcommunitySite))
                {
                    var htmlElement = resultat.DocumentNode.Descendants()
                        .Where(x => x.Attributes.Select(a => a.Value)
                        .Contains(techcommunitySite_html_tag_key)).FirstOrDefault();

                    var publishedby = resultat.DocumentNode.Descendants()
                        .Where(x => x.Attributes.Select(a => a.Value).Contains(techcommunitySite_publish_by_tag_key)).FirstOrDefault();

                    var publishedOn = resultat.DocumentNode.Descendants()
                        .Where(x => x.Attributes.Select(a => a.Value).Contains(techcommunitySite_publish_On_tag_key)).FirstOrDefault();

                    return this.Ok(new ArticleHtmlResponse()
                    {
                        Html = htmlElement?.InnerHtml,
                        PublishedBy = publishedby?.InnerText,
                        PublishedOn = publishedOn?.InnerText,
                    });
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetArticleFromUrl- The GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, $"Error occurred while fetching learning article from url {query.ArticleUrl}.");
            }

            return this.BadRequest();
        }
    }
}