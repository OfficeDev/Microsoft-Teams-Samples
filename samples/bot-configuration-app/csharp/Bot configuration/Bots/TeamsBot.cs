using AdaptiveCards;
using Bogus.DataSets;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Botconfiguration.Bots
{
    public class TeamsBot : TeamsActivityHandler
    {
        private string _chosenFlow = "";

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var imagePath = "Images/configbutton.png";
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            var adaptiveCardJson = @"
            {
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.0"",
                ""body"": [
                            {
                                ""type"": ""TextBlock"",
                                ""text"": ""Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card."",
                                ""wrap"": true,
                                ""size"": ""large"",
                                ""weight"": ""bolder""
                            },
                            {
                                ""type"": ""Image"",
                                ""url"": ""data:image/png;base64," + imageData + @""",
                                ""size"": ""auto""
                            }
                          ],
                ""fallbackText"": ""This card requires Adaptive Card support.""
            }";

            var attachment = new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };

            var reply = MessageFactory.Attachment(attachment);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                string text = turnContext.Activity.Text.ToLower().Trim();
                if (text == "chosen flow" || text == "<at>typeahead search adaptive card</at> chosen flow")
                {
                    await turnContext.SendActivityAsync($"Bot configured for {_chosenFlow} flow", cancellationToken: cancellationToken);
                }
            }
            else if (turnContext.Activity.Value != null)
            {
                var choiceselect = turnContext.Activity.Value;
                await turnContext.SendActivityAsync($"Selected option is: {choiceselect}", cancellationToken: cancellationToken);
            }
        }

        protected override Task<ConfigResponseBase> OnTeamsConfigFetchAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            /*
            Option 1: You can add a "config/auth" response as below code
            Note: The URL in value must be linked to a valid auth URL which can be opened in a browser. This code is only representative and not a working example.
            */
            /*ConfigResponseBase response = new ConfigResponse<BotConfigAuth>
            {
                Config = new BotConfigAuth
                {
                    SuggestedActions = new SuggestedActions
                    {
                        Actions = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = "openUrl",
                        Title = "Sign in to this app",
                        Value = "https://example.com/auth"
                    }
                }
                    },
                    Type = "auth"
                }
            };*/

            /*
            Option 2: You can add a "config/continue" response as below code
            */
            ConfigResponseBase response = adaptiveCardForContinue();

            return Task.FromResult(response);
        }

        protected override Task<ConfigResponseBase> OnTeamsConfigSubmitAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            JObject choice = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());
            try
            {
                _chosenFlow = choice["data"]["choiceselect"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_chosenFlow == "continue")
            {
                ConfigResponseBase response = adaptiveCardForSubmit();
                return Task.FromResult(response);
            }
            else
            {
                ConfigResponseBase response = new ConfigResponse<TaskModuleResponseBase>
                {
                    Config = new TaskModuleMessageResponse
                    {
                        Type = "message",
                        Value = "You have chosen to finish setting up bot"
                    }
                };

                return Task.FromResult(response);
            }
        }

        private ConfigResponseBase adaptiveCardForSubmit()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock()
                    {
                        Text = "Please hit submit to continue setting up bot",
                        Wrap = true,
                        Type = "TextBlock"
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction()
                    {
                        Type = AdaptiveSubmitAction.TypeName,
                        Id = "submitdynamic",
                        Title = "Submit"
                    }
                }
            };

            ConfigResponseBase response = new ConfigResponse<TaskModuleResponseBase>
            {
                Config = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 400,
                        Width = 400,
                        Title = "Task module submit response",
                        Card = new Microsoft.Bot.Schema.Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        }
                    },
                    Type = "continue"
                }
            };

            return response;
        }

        private ConfigResponseBase adaptiveCardForContinue()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock()
                    {
                        Text = "Please choose bot set up option",
                        Wrap = true,
                        Type = "TextBlock"
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "auto",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Option: ",
                                        Wrap = true,
                                        Height = AdaptiveHeight.Stretch,
                                        Type = "TextBlock"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice()
                                            {
                                                Title = "Continue with more options",
                                                Value = "continue"
                                            },
                                            new AdaptiveChoice()
                                            {
                                                Title = "Finish setting up bot",
                                                Value = "finish"
                                            }
                                        },
                                        Value = "Search for an option",
                                        Id = "choiceselect"
                                    }
                                }
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction()
                    {
                        Type = AdaptiveSubmitAction.TypeName,
                        Id = "submit",
                        Title = "Submit"
                    }
                }
            };

            ConfigResponseBase response = new ConfigResponse<TaskModuleResponseBase>
            {
                Config = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 400,
                        Width = 400,
                        Title = "Task module fetch response",
                        Card = new Microsoft.Bot.Schema.Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        }
                    },
                    Type = "continue"
                }
            };

            return response;
        }
    }
}