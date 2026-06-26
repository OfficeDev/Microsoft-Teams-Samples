// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;


namespace Microsoft.BotBuilderSamples.Bots
{
     public class TeamsMessagingExtensionsSearchBot : TeamsActivityHandler
    {
        private readonly string _siteUrl;
        private readonly string _connectionName;
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<string> _userConfigProperty;


        public TeamsMessagingExtensionsSearchBot(IConfiguration configuration, UserState userState) :base()
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _siteUrl = configuration["SiteUrl"] ?? throw new NullReferenceException("SiteUrl");
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
            _userConfigProperty = userState.CreateProperty<string>("UserConfiguration");

        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // After the turn is complete, persist any UserState changes.
            await _userState.SaveChangesAsync(turnContext);
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            JObject valueObject = JObject.FromObject(turnContext.Activity.Value);
            if (valueObject["authentication"] != null)
            {
                JObject authenticationObject = JObject.FromObject(valueObject["authentication"]);
                if (authenticationObject["token"] != null)
                {
                    //If the token is NOT exchangeable, then return 412 to require user consent
                    if (await TokenIsExchangeable(turnContext, cancellationToken))
                    {
                        return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var response = new InvokeResponse();
                        response.Status = 412;
                        return response;
                    }
                }
            }
            return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {

            var text = query.Parameters?[0]?.Value as string ?? string.Empty;

            var attachments = new List<MessagingExtensionAttachment>();
            var userConfigSettings = await _userConfigProperty.GetAsync(turnContext, () => string.Empty);
            if (query.CommandId == "mailQuery")
            {
                // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
                var state = query.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

                    return new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "silentAuth",
                            SuggestedActions = new MessagingExtensionSuggestedAction
                            {
                                Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = signInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }

                var client = new SimpleGraphClient(tokenResponse.Token);

                var messages = await client.SearchMailInboxAsync(text);

                // Here we construct a ThumbnailCard for every attachment, and provide a HeroCard which will be
                // displayed if the selects that item.
                attachments = messages.Select(msg => new MessagingExtensionAttachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard
                    {
                        Title = msg.From.EmailAddress.Address,
                        Subtitle = msg.Subject,
                        Text = msg.Body.Content,
                    },
                    Preview = new ThumbnailCard
                    {
                        Title = msg.From.EmailAddress.Address,
                        Text = $"{msg.Subject}<br />{msg.BodyPreview}",
                        Images = new List<CardImage>()
                            {
                                new CardImage("https://raw.githubusercontent.com/microsoft/botbuilder-samples/master/docs/media/OutlookLogo.jpg", "Outlook Logo"),
                            },
                    }.ToAttachment()
                }
                ).ToList();
            }
            else
            {
                var packages = await FindPackages(text);
                // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
                // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
                attachments = packages.Select(package =>
                {
                    var previewCard = new ThumbnailCard { Title = package.Item1, Tap = new CardAction { Type = "invoke", Value = package } };
                    if (!string.IsNullOrEmpty(package.Item5))
                    {
                        previewCard.Images = new List<CardImage>() { new CardImage(package.Item5, "Icon") };
                    }

                    var attachment = new MessagingExtensionAttachment
                    {
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard { Title = package.Item1 },
                        Preview = previewCard.ToAttachment()
                    };

                    return attachment;
                }).ToList();
            }

            // The list of MessagingExtensionAttachments must we wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };


        }
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            /// The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            var (packageId, version, description, projectUrl, iconUrl) = query.ToObject<(string, string, string, string, string)>();

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
            var card = new ThumbnailCard
            {
                Title = $"{packageId}, {version}",
                Subtitle = description,
                Buttons = new List<CardAction>
                    {
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Nuget Package", Value = $"https://www.nuget.org/packages/{packageId}" },
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Project", Value = projectUrl },
                    },
            };

            if (!string.IsNullOrEmpty(iconUrl))
            {
                card.Images = new List<CardImage>() { new CardImage(iconUrl, "Icon") };
            }

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        //Get SSO Token using the Bot Framework SDK 
        private async Task<TokenResponse> GetTokenResponse(ITurnContext<IInvokeActivity> turnContext, string state, CancellationToken cancellationToken)
        {
            var magicCode = string.Empty;

            if (!string.IsNullOrEmpty(state))
            {
                if (int.TryParse(state, out var parsed))
                {
                    magicCode = parsed.ToString();
                }
            }

            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            return tokenResponse;
        }

        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        private async Task<bool> TokenIsExchangeable(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            TokenResponse tokenExchangeResponse = null;
            try
            {
                JObject valueObject = JObject.FromObject(turnContext.Activity.Value);
                var tokenExchangeRequest =((JObject)valueObject["authentication"])?.ToObject<TokenExchangeInvokeRequest>();
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, new TokenExchangeRequest { Token = tokenExchangeRequest.Token }, cancellationToken)
                    .ConfigureAwait(false);
                // return to the console the tokenExchangeResponse which includes the token.  This is the token you can use to call Graph APIs.
                Console.WriteLine($"Token exchange response : {tokenExchangeResponse}");
            }
#pragma warning disable CA1031 //Do not catch general exception types (ignoring, see comment below)
            catch
#pragma warning restore CA1031 //Do not catch general exception types
            {
                //ignore exceptions
                //if token exchange failed for any reason, tokenExchangeResponse above remains null, and a failure invoke response is sent to the caller.
                //This ensures the caller knows that the invoke has failed.
            }
            if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
            {
                return false;
            }
            return true;
        }

        //Custom Part for Call Queues
        // Generate a set of substrings to illustrate the idea of a set of results coming back from a query. 
        private async Task<IEnumerable<(string, string, string, string, string)>> FindPackages(string text)
        {
            var obj = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{text}&prerelease=true"));
            return obj["data"].Select(item => (item["id"].ToString(), item["version"].ToString(), item["description"].ToString(), item["projectUrl"]?.ToString(), item["iconUrl"]?.ToString()));
        }
      

        public MessagingExtensionResponse GetAdaptiveCard()
        {
            var paths = new[] { ".", "Resources", "RestaurantCard.json" };
            string filepath = Path.Combine(paths); 
            var previewcard = new ThumbnailCard
            {
                Title = "Adaptive Card",
                Text = "Please select to get Adaptive card"
            };
            var adaptiveList = FetchAdaptive(filepath);
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = adaptiveList.Content,
                Preview = previewcard.ToAttachment()
            };

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            };
        }
        public MessagingExtensionResponse GetConnectorCard()
        {
            var path = new[] { ".", "Resources", "connectorCard.json" };
            var filepath = Path.Combine(path);   
            var previewcard = new ThumbnailCard
            {
                Title = "O365 Connector Card",
                Text = "Please select to get Connector card"
            };

            var connector = FetchConnector(filepath);
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = connector.Content,
                Preview = previewcard.ToAttachment()
            };

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            };
        }

        public  Attachment FetchAdaptive(string filepath)
        {
            var adaptiveCardJson = File.ReadAllText(filepath);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };      
            return adaptiveCardAttachment;
        }

        public Attachment FetchConnector(string filepath)
        {
            var connectorCardJson = File.ReadAllText(filepath);
            var connectorCardAttachment = new MessagingExtensionAttachment
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = JsonConvert.DeserializeObject(connectorCardJson),

            };
            return connectorCardAttachment;
        }

        public MessagingExtensionResponse GetResultGrid()
        {
            var imageFiles = Directory.EnumerateFiles("wwwroot", "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpg"));

            List<MessagingExtensionAttachment> attachments = new List<MessagingExtensionAttachment>();

            foreach (string img in imageFiles)
            {
                var image = img.Split("\\");                
                var thumbnailCard = new ThumbnailCard();
                thumbnailCard.Images = new List<CardImage>() { new CardImage(_siteUrl + "/" + image[1]) };
                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = thumbnailCard,
                };
                attachments.Add(attachment);
            }
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "grid",
                    Attachments = attachments
                }
            };
        }
    }
}