using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppCatalogSample.Helper
{
    public static class OAuthHelpers
    {
        public static async Task<IList<TeamsApp>> GetAllapp(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.GetAllapp();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public static async Task<IList<TeamsApp>> AppCatalogById(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.AppCatalogById();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public static async Task<IList<TeamsApp>> FindApplicationByTeamsId(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.FindApplicationByTeamsId();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public static async Task<IList<TeamsApp>> AppStatus(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.AppStatus();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public static async Task<IList<TeamsApp>> ListAppHavingBot(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.ListAppHavingBot();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public static async Task<string> DeleteApp(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.DeleteApp();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public static async Task<string> UploadFileAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.UploadFileAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public static async Task<string> UpdateFileAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            try
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                return await client.UpdateFileAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Your Action :" + " \r" + "-" + " List" + " \r" + "-" + " Publish" + " \r" + "-" + " Update" + " \r" + "-" + " Delete" + " \r");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {

                    new CardAction() { Title = "List", Type = ActionTypes.ImBack, Value = "list"},
                    new CardAction() { Title = "Update", Type = ActionTypes.ImBack, Value = "update"},
                    new CardAction() { Title = "Delete", Type = ActionTypes.ImBack, Value = "delete"},
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        public static async Task SendListActionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Your Action :" + " \r" + "-" + " Home" + " \r" + "-" + " ListApp: listapp" + " \r" + "-" + " ListApp by ID: app" + " \r" + "-" + " App based on manifest Id: findapp" + " \r" + "-" + " App Status:status" + " \r" + "-" + " List of bot:bot" + " \r");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Home", Type = ActionTypes.ImBack, Value = "home" },
                    new CardAction() { Title = "ListApp", Type = ActionTypes.ImBack, Value = "listapp"},
                    new CardAction() { Title = "ListApp by ID", Type = ActionTypes.ImBack, Value = "app" },
                    new CardAction() { Title = "App based on manifest Id", Type = ActionTypes.ImBack, Value = "findapp"},
                    new CardAction() { Title = "App Status", Type = ActionTypes.ImBack, Value = "status"},
                    new CardAction() { Title = "List of bot", Type = ActionTypes.ImBack, Value = "bot" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }


        public static List<CardData> ParseData(IList<TeamsApp> teamsApps)
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
        public static Microsoft.Bot.Schema.Attachment AgendaAdaptiveList(string Header, List<CardData> taskInfoData)
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

        public static Microsoft.Bot.Schema.Attachment AdaptivCardList(string Header, string response)
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
