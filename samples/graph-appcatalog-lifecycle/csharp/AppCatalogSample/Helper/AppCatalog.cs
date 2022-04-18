using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Microsoft.Extensions.Caching.Memory;

namespace AppCatalogSample.Helper
{
    public class AppCatalogHelper
    {
        private const string appName = "App Catalog Bot";
        public readonly GraphServiceClient graphServiceClient;

        private readonly string _token;

        public AppCatalogHelper(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            _token = token;
            GraphClient graphClient = new GraphClient(_token);
            graphServiceClient = graphClient.GetAuthenticatedClient();

        }
        public async Task<IList<TeamsApp>> GetAllapp()
        {
            try
            {

                if (string.IsNullOrEmpty(this._token))
                    return null;

                var teamsApps = await graphServiceClient.AppCatalogs.TeamsApps
                   .Request()
                   .Filter("distributionMethod eq 'organization'")
                   .GetAsync();
                return teamsApps.CurrentPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public async Task<IList<TeamsApp>> AppCatalogById()
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                    return null;
                var id = GetAppId();
                var teamsApps = await graphServiceClient.AppCatalogs.TeamsApps
            .Request()
            .Filter($"id eq '{id}'")
            .GetAsync();
                return teamsApps.CurrentPage;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public string GetAppId()
        {
            var listApp = GetAllapp().ConfigureAwait(false).GetAwaiter().GetResult();
            var id = listApp.Where(x => x.DisplayName == appName).Select(x => x.Id).FirstOrDefault();
            return id;
        }
        public string GetExternalId()
        {
            var listApp = GetAllapp().ConfigureAwait(false).GetAwaiter().GetResult();
            var id = listApp.Where(x => x.DisplayName == appName).Select(x => x.ExternalId).FirstOrDefault();
            return id;
        }

        public async Task<IList<TeamsApp>> FindApplicationByTeamsId()
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                    return null;
                var ExternalId = GetExternalId();
                var teamsApps = await graphServiceClient.AppCatalogs.TeamsApps
           .Request()
           .Filter($"externalId eq '{ExternalId}'")
           .GetAsync();
                return teamsApps.CurrentPage;
            }
            catch (Exception)
            {
                return null;
            }
        }
        //Return the status of App, Published or not
        public async Task<IList<TeamsApp>> AppStatus()
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                    return null;
                var appId = GetAppId();
                var teamsApps = await graphServiceClient.AppCatalogs.TeamsApps
        .Request()
        .Filter($"id eq '{appId}'")
        .Expand("appDefinitions")
        .GetAsync();
                return teamsApps.CurrentPage;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // Returns the list of app that contains a  Bot
        public async Task<IList<TeamsApp>> ListAppHavingBot()
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                    return null;
                var teamsApps = await graphServiceClient.AppCatalogs.TeamsApps
        .Request()
        .Filter("appDefinitions/any(a:a/bot ne null)")
        .Expand("appDefinitions($expand=bot)")
        .GetAsync();
                return teamsApps.CurrentPage;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<string> DeleteApp()
        {
            try
            {

                if (string.IsNullOrEmpty(_token))
                    return null;

                var appId = GetAppId(); ;
                await graphServiceClient.AppCatalogs.TeamsApps[appId]
                  .Request()
                  .DeleteAsync();
                return "Deleted";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        public async Task<string> UploadFileAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                var url = "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps";
                if (String.IsNullOrEmpty(_token))
                    return "require login";
                string token = _token;
                var multiForm = new MultipartFormDataContent();
                HttpContent con;
                string path = System.IO.Directory.GetCurrentDirectory();
                path = path + @"\Manifest\manifest.zip";
                using (var str = new FileStream(path, FileMode.Open))
                {
                    con = new StreamContent(str);
                    con.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "file_name",
                        FileName = "manifest.zip"
                    };
                    con.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                    multiForm.Add(con);
                    // add file and directly upload it
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var response = await client.PostAsync(url, con);
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Console.WriteLine("Resource Created");
                        return "App publish successfully";
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        Console.WriteLine("Resource not  Created");
                        return "Conflict in publishing app";
                    }
                    return "App not published";
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("ERROR" + Ex.Message.ToString());
                return Ex.Message.ToString();
            }
        }

        public async Task<string> UpdateFileAsync()
        {
            try
            {
                var appId = GetAppId();
                HttpClient client = new HttpClient();
                var url = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{appId}/appDefinitions";
                if (String.IsNullOrEmpty(_token))
                    return "require login";
                string token = _token;
                var multiForm = new MultipartFormDataContent();
                HttpContent con;
                string path = System.IO.Directory.GetCurrentDirectory() + @"\Manifest\manifest.zip";
                using (var str = new FileStream(path, FileMode.Open))
                {
                    con = new StreamContent(str);
                    con.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "file_name",
                        FileName = "manifest.zip"
                    };
                    con.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                    multiForm.Add(con);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var response = await client.PutAsync(url, con);
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        Console.WriteLine("Resource Created");
                        return "App update successfully";
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        Console.WriteLine("Resource not  Created");
                        return "Conflict in updating app";
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine("Resource not  Created");
                        return "BadRequest";
                    }
                    return "App not published";
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("ERROR" + Ex.Message.ToString());
                return Ex.Message.ToString();
            }
        }


        public async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Your Action :" + " \r" + "-" + " Publish: publish" + " \r" + "-" + " Update: update" + " \r" + "-" + " Delete: delete" + " \r" + " \r" + "-" + " ListApp: listapp" + " \r" + "-" + " ListApp by ID: app" + " \r" + "-" + " App based on manifest Id: findapp" + " \r" + "-" + " App Status:status" + " \r" + "-" + " List of bot:bot" + " \r");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Update", Type = ActionTypes.ImBack, Value = "update"},
                    new CardAction() { Title = "Publish", Type = ActionTypes.ImBack, Value = "publish"},
                    new CardAction() { Title = "Delete", Type = ActionTypes.ImBack, Value = "delete"},
                    new CardAction() { Title = "ListApp", Type = ActionTypes.ImBack, Value = "listapp"},
                    new CardAction() { Title = "ListApp by ID", Type = ActionTypes.ImBack, Value = "app" },
                    new CardAction() { Title = "App based on manifest Id", Type = ActionTypes.ImBack, Value = "findapp"},
                    new CardAction() { Title = "App Status", Type = ActionTypes.ImBack, Value = "status"},
                    new CardAction() { Title = "List of bot", Type = ActionTypes.ImBack, Value = "bot" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        public async Task SendContinueMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Please type something to contiue.");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }


        public List<CardData> ParseData(IList<TeamsApp> teamsApps)
        {

            List<CardData> InfoData = new List<CardData>();
            int DataCount = 0;
            foreach (var value in teamsApps)
            {
                CardData instance = new CardData();
                instance.DisplayName = value.DisplayName;
                instance.DistributionMethod = value.DistributionMethod.ToString();
                instance.ExternalId = value.ExternalId;
                instance.Id = value.Id;
                instance.OdataType = value.ODataType;
                if (value.AppDefinitions != null)
                {
                    instance.AppDefinitions = value.AppDefinitions;
                    instance.Published = value.AppDefinitions.CurrentPage[0].PublishingState;
                }
                InfoData.Add(instance);
                DataCount++;
                if (DataCount > 100)
                    break;
            }
            return InfoData;
        }
        public Microsoft.Bot.Schema.Attachment AgendaAdaptiveList(string Header, List<CardData> taskInfoData)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            adaptiveCard.Body = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock(){Text=Header, Weight=AdaptiveTextWeight.Bolder}
            };
            foreach (var agendaPoint in taskInfoData)
            {
                if (Header.Contains("bot") || Header.Contains("status"))
                {
                    AdaptiveTextBlock textBlock = new AdaptiveTextBlock()
                    {
                        Text = "- " + agendaPoint.DisplayName + " \r" +
                        " - " + agendaPoint.Id + " \r" + " - " + agendaPoint.Published + " \r",

                    };
                    adaptiveCard.Body.Add(textBlock);
                }
                else
                {
                    AdaptiveTextBlock textBlock = new AdaptiveTextBlock()
                    {
                        Text = "- " + agendaPoint.DisplayName + " \r" +
                                            " - " + agendaPoint.Id + " \r",

                    };
                    adaptiveCard.Body.Add(textBlock);
                }


            }

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
        }

        public Microsoft.Bot.Schema.Attachment AdaptivCardList(string Header, string response)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            adaptiveCard.Body = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock(){Text=Header, Weight=AdaptiveTextWeight.Bolder}
            };


            AdaptiveTextBlock textBlock = new AdaptiveTextBlock()
            {
                Text = "- " + response + " \r"

            };
            adaptiveCard.Body.Add(textBlock);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
        }
    }

}